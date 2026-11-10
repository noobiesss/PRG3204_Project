using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PRG3204_Project_Group4.Events
{
    public partial class Manage : Page
    {
        private string ConnectionString { get { return ConfigurationManager.ConnectionStrings["CampusEventDB"].ConnectionString; } }
        private int UserId { get { int id; return int.TryParse(Convert.ToString(Session["UserId"]), out id) ? id : 0; } }
        private bool IsAdmin { get { return string.Equals(Convert.ToString(Session["Role"]), "Admin", StringComparison.OrdinalIgnoreCase); } }
        private bool CanManage { get { string role = Convert.ToString(Session["Role"]); return UserId > 0 && (role == "Admin" || role == "Staff"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CanManage) { Response.Redirect("~/Account/Login.aspx", true); return; }
            if (!IsPostBack) { LoadCategories(); LoadMyEvents(); }
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT CategoryId, CategoryName FROM dbo.Categories ORDER BY CategoryName", conn))
            {
                conn.Open();
                ddlCategory.DataSource = cmd.ExecuteReader();
                ddlCategory.DataValueField = "CategoryId";
                ddlCategory.DataTextField = "CategoryName";
                ddlCategory.DataBind();
            }
            ddlCategory.Items.Insert(0, new ListItem("-- Select Category --", ""));
        }

        private void LoadMyEvents()
        {
            const string sql = @"SELECT e.EventId, e.Title, c.CategoryName, DATEADD(HOUR,8,e.StartDateTime) AS StartMY, e.Capacity, e.Status,
                (SELECT COUNT(*) FROM dbo.Registrations r WHERE r.EventId=e.EventId AND r.Status='Registered') AS RegisteredCount
                FROM dbo.Events e JOIN dbo.Categories c ON c.CategoryId=e.CategoryId
                WHERE (@IsAdmin=1 OR e.OrganizerId=@UserId) ORDER BY e.StartDateTime DESC";
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@IsAdmin", SqlDbType.Bit).Value = IsAdmin;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                DataTable table = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(table);
                gvEvents.DataSource = table; gvEvents.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!CanManage) return;
            int categoryId, capacity; DateTime startMY, endMY;
            string title = txtTitle.Text.Trim(), location = txtLocation.Text.Trim();
            if (title.Length == 0 || title.Length > 200 || location.Length == 0 || location.Length > 200 ||
                !int.TryParse(ddlCategory.SelectedValue, out categoryId) || categoryId <= 0 ||
                !int.TryParse(txtCapacity.Text, out capacity) || capacity <= 0 ||
                !ParseMY(txtStartDate.Text, out startMY) || !ParseMY(txtEndDate.Text, out endMY))
            { Message("Fill in valid title, category, location, dates and capacity.", false); return; }
            if (endMY <= startMY) { Message("End time must be after start time.", false); return; }
            // All database event timestamps are UTC, while HTML form values are Malaysia time (UTC+8).
            DateTime startUTC = startMY.AddHours(-8), endUTC = endMY.AddHours(-8);
            int eventId;
            if (!int.TryParse(hidEventId.Value, out eventId)) eventId = 0;
            if (eventId == 0 && startUTC <= DateTime.UtcNow) { Message("New event start must be in the future.", false); return; }
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    if (eventId == 0)
                    {
                        const string sql = @"INSERT dbo.Events(CategoryId,OrganizerId,Title,Description,Location,StartDateTime,EndDateTime,Capacity,Status)
                            OUTPUT INSERTED.EventId VALUES(@CategoryId,@UserId,@Title,@Description,@Location,@Start,@End,@Capacity,'Upcoming')";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            AddEventParameters(cmd, categoryId, title, txtDescription.Text.Trim(), location, startUTC, endUTC, capacity);
                            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                            eventId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        Message("Event created. Select Edit / Upload to attach resources.", true);
                    }
                    else
                    {
                        // Lock the Events row to coordinate with registration-capacity changes from M2/M3.
                        using (SqlTransaction tx = conn.BeginTransaction(IsolationLevel.Serializable))
                        {
                            const string lockSql = @"SELECT Status, OrganizerId FROM dbo.Events WITH (UPDLOCK, HOLDLOCK) WHERE EventId=@EventId";
                            string status = null; int owner = 0;
                            using (SqlCommand cmd = new SqlCommand(lockSql, conn, tx))
                            {
                                cmd.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                { if (reader.Read()) { status = reader.GetString(0); owner = reader.GetInt32(1); } }
                            }
                            if (status == null || status == "Cancelled" || (!IsAdmin && owner != UserId))
                            { tx.Rollback(); Message("Event not found or editing is not permitted.", false); return; }
                            int registered;
                            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Registrations WHERE EventId=@EventId AND Status='Registered'", conn, tx))
                            { cmd.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId; registered = Convert.ToInt32(cmd.ExecuteScalar()); }
                            if (capacity < registered)
                            { tx.Rollback(); Message("Capacity cannot be less than the number of registered participants (" + registered + ").", false); return; }
                            const string sql = @"UPDATE dbo.Events SET CategoryId=@CategoryId,Title=@Title,Description=@Description,Location=@Location,
                                StartDateTime=@Start,EndDateTime=@End,Capacity=@Capacity,UpdatedAt=SYSUTCDATETIME() WHERE EventId=@EventId";
                            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
                            {
                                AddEventParameters(cmd, categoryId, title, txtDescription.Text.Trim(), location, startUTC, endUTC, capacity);
                                cmd.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                                cmd.ExecuteNonQuery();
                            }
                            tx.Commit();
                        }
                        Message("Event updated successfully.", true);
                    }
                }
                ResetForm(); LoadMyEvents();
            }
            catch (SqlException) { Message("Database could not save the event. Check the event data and your account.", false); }
        }

        private static bool ParseMY(string value, out DateTime result)
        {
            return DateTime.TryParseExact(value, new[] { "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }
        private static void AddEventParameters(SqlCommand cmd, int catId, string title, string description, string location, DateTime startUtc, DateTime endUtc, int capacity)
        {
            cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value = catId;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = title;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(description) ? (object)DBNull.Value : description;
            cmd.Parameters.Add("@Location", SqlDbType.NVarChar, 200).Value = location;
            cmd.Parameters.Add("@Start", SqlDbType.DateTime2).Value = startUtc;
            cmd.Parameters.Add("@End", SqlDbType.DateTime2).Value = endUtc;
            cmd.Parameters.Add("@Capacity", SqlDbType.Int).Value = capacity;
        }

        protected void gvEvents_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id; if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;
            if (e.CommandName == "EditEvent") OpenEvent(id);
            else if (e.CommandName == "CancelEvent") CancelEvent(id);
        }
        private void OpenEvent(int id)
        {
            const string sql = @"SELECT CategoryId,Title,Description,Location,StartDateTime,EndDateTime,Capacity,Status FROM dbo.Events
                WHERE EventId=@EventId AND (@Admin=1 OR OrganizerId=@UserId)";
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@EventId", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@Admin", SqlDbType.Bit).Value = IsAdmin;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read()) { Message("Event not found.", false); return; }
                    if (r.GetString(7) == "Cancelled") { Message("Cancelled events cannot be edited.", false); return; }
                    string cat = Convert.ToString(r.GetInt32(0), CultureInfo.InvariantCulture);
                    if (ddlCategory.Items.FindByValue(cat) != null) ddlCategory.SelectedValue = cat;
                    txtTitle.Text = r.GetString(1); txtDescription.Text = r.IsDBNull(2) ? "" : r.GetString(2);
                    txtLocation.Text = r.GetString(3);
                    txtStartDate.Text = r.GetDateTime(4).AddHours(8).ToString("yyyy-MM-ddTHH:mm");
                    txtEndDate.Text = r.GetDateTime(5).AddHours(8).ToString("yyyy-MM-ddTHH:mm");
                    txtCapacity.Text = Convert.ToString(r.GetInt32(6), CultureInfo.InvariantCulture);
                }
            }
            hidEventId.Value = Convert.ToString(id); litFormHeading.Text = "Update Event #" + id;
            btnSave.Text = "Update Event"; pnlUpload.Visible = true; LoadResources(id);
            Message("Editing event #" + id + ".", true);
        }
        private void CancelEvent(int id)
        {
            const string sql = @"UPDATE dbo.Events SET Status='Cancelled',UpdatedAt=SYSUTCDATETIME()
                WHERE EventId=@EventId AND Status<>'Cancelled' AND (@Admin=1 OR OrganizerId=@UserId)";
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@EventId", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@Admin", SqlDbType.Bit).Value = IsAdmin;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                conn.Open(); int changed = cmd.ExecuteNonQuery();
                Message(changed == 1 ? "Event cancelled. Registration history was preserved." : "Event not found or already cancelled.", changed == 1);
            }
            ResetForm(); LoadMyEvents();
        }
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            int eventId;
            if (!CanManage || !int.TryParse(hidEventId.Value, out eventId) || eventId <= 0)
            { Message("Select an event first.", false); return; }
            if (!fuResource.HasFile || fuResource.PostedFile.ContentLength <= 0 || fuResource.PostedFile.ContentLength > 3 * 1024 * 1024)
            { Message("Choose a file up to 3 MB.", false); return; }
            string ext = Path.GetExtension(fuResource.FileName).ToLowerInvariant();
            if (ext != ".pdf" && ext != ".jpg" && ext != ".jpeg" && ext != ".png")
            { Message("Only PDF, JPG and PNG files are allowed.", false); return; }
            string original = Path.GetFileName(fuResource.FileName);
            if (original.Length > 255) original = original.Substring(original.Length - 255);
            // Check file signatures, not just the extension.
            byte[] magic = new byte[8];
            using (Stream stream = fuResource.PostedFile.InputStream)
            {
                int read = stream.Read(magic, 0, magic.Length);
                bool pdf = read >= 5 && magic[0] == 0x25 && magic[1] == 0x50 && magic[2] == 0x44 && magic[3] == 0x46 && magic[4] == 0x2D;
                bool jpg = read >= 3 && magic[0] == 0xFF && magic[1] == 0xD8 && magic[2] == 0xFF;
                bool png = read >= 8 && magic[0] == 137 && magic[1] == 80 && magic[2] == 78 && magic[3] == 71 && magic[4] == 13 && magic[5] == 10 && magic[6] == 26 && magic[7] == 10;
                if (!((ext == ".pdf" && pdf) || ((ext == ".jpg" || ext == ".jpeg") && jpg) || (ext == ".png" && png)))
                { Message("File content does not match its PDF or image extension.", false); return; }
                stream.Position = 0;
            }
            string saved = Guid.NewGuid().ToString("N") + ext;
            string dir = Server.MapPath("~/Uploads/EventResources/");
            Directory.CreateDirectory(dir);
            string physicalPath = Path.Combine(dir, saved);
            string virtualPath = "~/Uploads/EventResources/" + saved;
            try
            {
                // Authorization is checked using database ownership as well as Session.
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"SELECT COUNT(*) FROM dbo.Events WHERE EventId=@EventId AND Status<>'Cancelled' AND (@Admin=1 OR OrganizerId=@UserId)", conn))
                    {
                        cmd.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                        cmd.Parameters.Add("@Admin", SqlDbType.Bit).Value = IsAdmin;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0) { Message("You cannot upload to this event.", false); return; }
                    }
                    fuResource.SaveAs(physicalPath);
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(@"INSERT INTO dbo.EventResources(EventId,FileName,FilePath,FileType) VALUES(@EventId,@Name,@Path,@Type)", conn))
                        {
                            cmd.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 255).Value = original;
                            cmd.Parameters.Add("@Path", SqlDbType.NVarChar, 500).Value = virtualPath;
                            cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 50).Value = ext.TrimStart('.').ToUpperInvariant();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch { if (File.Exists(physicalPath)) File.Delete(physicalPath); throw; }
                }
                Message("Resource uploaded successfully.", true); pnlUpload.Visible = true; LoadResources(eventId);
            }
            catch (Exception ex) when (ex is SqlException || ex is IOException || ex is UnauthorizedAccessException)
            { Message("Upload failed. Check database or upload-folder permissions.", false); }
        }
        private void LoadResources(int id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT FileName,FilePath,FileType FROM dbo.EventResources WHERE EventId=@Id ORDER BY UploadedAt DESC", conn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
                rptResources.DataSource = dt; rptResources.DataBind();
            }
        }
        protected void btnClear_Click(object sender, EventArgs e) { ResetForm(); lblMessage.Text = ""; }
        private void ResetForm()
        {
            hidEventId.Value = ""; txtTitle.Text = txtDescription.Text = txtLocation.Text = txtStartDate.Text = txtEndDate.Text = txtCapacity.Text = "";
            if (ddlCategory.Items.Count > 0) ddlCategory.SelectedIndex = 0;
            btnSave.Text = "Create Event"; litFormHeading.Text = "Create Event"; pnlUpload.Visible = false;
        }
        private void Message(string message, bool success)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.CssClass = success ? "alert alert-success d-block mb-3" : "alert alert-danger d-block mb-3";
        }
    }
}