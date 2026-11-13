using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PRG3204_Project_Group4.Reports
{
    public partial class MarkAttendance : Page
    {
        private int CurrentUserId
        {
            get
            {
                int userId;
                return Int32.TryParse(Convert.ToString(Session["UserId"]), out userId) ? userId : 0;
            }
        }

        private bool IsAdmin
        {
            get { return String.Equals(Convert.ToString(Session["Role"]), "Admin", StringComparison.OrdinalIgnoreCase); }
        }

        private bool CanManage
        {
            get
            {
                return IsAdmin || String.Equals(Convert.ToString(Session["Role"]), "Staff", StringComparison.OrdinalIgnoreCase);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Session != null) ViewStateUserKey = Session.SessionID;
            if (CurrentUserId <= 0)
            {
                Response.Redirect("~/Account/Login.aspx", true);
                return;
            }
            if (!CanManage) throw new HttpException(403, "Only staff and administrators can mark attendance.");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadEvents();
                int requestedEvent;
                if (Int32.TryParse(Request.QueryString["eventId"], out requestedEvent))
                {
                    ListItem item = ddlEvents.Items.FindByValue(requestedEvent.ToString());
                    if (item != null) ddlEvents.SelectedValue = item.Value;
                }
                LoadStudents();
            }
        }

        private SqlConnection OpenConnection()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["CampusEventDb"];
            if (settings == null)
                throw new ConfigurationErrorsException("Missing CampusEventDb connection string; check ConnectionStrings.config.");
            SqlConnection connection = new SqlConnection(settings.ConnectionString);
            connection.Open();
            return connection;
        }

        private void AddScope(SqlCommand command)
        {
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = CurrentUserId;
            command.Parameters.Add("@IsAdmin", SqlDbType.Bit).Value = IsAdmin;
        }

        private void LoadEvents()
        {
            const string sql = @"
SELECT EventId, Title, StartDateTime
FROM dbo.Events
WHERE Status <> 'Cancelled'
  AND StartDateTime <= SYSUTCDATETIME()
  AND (@IsAdmin = 1 OR OrganizerId = @UserId)
ORDER BY StartDateTime DESC, Title;";
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddScope(command);
                ddlEvents.Items.Clear();
                ddlEvents.Items.Add(new ListItem("-- Choose an event --", ""));
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime eventUtc = DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc);
                        string label = reader.GetString(1) + " — " + eventUtc.AddHours(8).ToString("dd MMM yyyy HH:mm") + " MYT";
                        ddlEvents.Items.Add(new ListItem(label, reader.GetInt32(0).ToString()));
                    }
                }
            }
        }

        protected void Events_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void LoadStudents()
        {
            pnlRegistrations.Visible = false;
            lblSummary.Text = "";
            int eventId;
            if (!Int32.TryParse(ddlEvents.SelectedValue, out eventId)) return;

            // Repeat the organizer and event checks in SQL; never trust a dropdown selection alone.
            const string sql = @"
SELECT r.RegistrationId, u.MatricOrStaffNo, u.FullName, r.IsAttended
FROM dbo.Registrations AS r
JOIN dbo.Users AS u ON u.UserId = r.UserId
JOIN dbo.Events AS e ON e.EventId = r.EventId
WHERE r.EventId = @EventId AND r.Status = 'Registered' AND u.Role = 'Student'
  AND e.Status <> 'Cancelled'
  AND e.StartDateTime <= SYSUTCDATETIME()
  AND (@IsAdmin = 1 OR e.OrganizerId = @UserId)
ORDER BY u.FullName, u.MatricOrStaffNo;";
            DataTable students = new DataTable();
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                AddScope(command);
                command.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                adapter.Fill(students);
            }
            gvStudents.DataSource = students;
            gvStudents.DataBind();
            pnlRegistrations.Visible = true;
            int attended = 0;
            foreach (DataRow row in students.Rows)
                if (Convert.ToBoolean(row["IsAttended"])) attended++;
            lblSummary.Text = students.Rows.Count + " registered | " + attended + " marked attended";
            btnSave.Visible = students.Rows.Count > 0;
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!CanManage || CurrentUserId <= 0) throw new HttpException(403, "Access denied.");
            int eventId;
            if (!Int32.TryParse(ddlEvents.SelectedValue, out eventId) || eventId <= 0)
            {
                ShowMessage("Please select an event first.", false);
                return;
            }
            if (gvStudents.Rows.Count == 0)
            {
                ShowMessage("There are no registrations to update.", false);
                return;
            }

            // Every UPDATE rechecks the registration, organizer, event status and UTC start time.
            // Transaction prevents a partially saved batch if a row becomes ineligible.
            const string sql = @"
UPDATE dbo.Registrations
SET IsAttended = @IsAttended, UpdatedAt = SYSUTCDATETIME()
WHERE RegistrationId = @Id
  AND EventId = @EventId
  AND Status = 'Registered'
  AND EXISTS (SELECT 1 FROM dbo.Users AS u WHERE u.UserId = dbo.Registrations.UserId AND u.Role = 'Student')
  AND EXISTS (
      SELECT 1 FROM dbo.Events AS e
      WHERE e.EventId = dbo.Registrations.EventId
        AND e.Status <> 'Cancelled'
        AND e.StartDateTime <= SYSUTCDATETIME()
        AND (@IsAdmin = 1 OR e.OrganizerId = @UserId)
  );";

            try
            {
                using (SqlConnection connection = OpenConnection())
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                    {
                        AddScope(command);
                        command.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                        SqlParameter idParam = command.Parameters.Add("@Id", SqlDbType.Int);
                        SqlParameter attendedParam = command.Parameters.Add("@IsAttended", SqlDbType.Bit);
                        foreach (GridViewRow row in gvStudents.Rows)
                        {
                            if (row.RowType != DataControlRowType.DataRow) continue;
                            CheckBox box = row.FindControl("chkAttended") as CheckBox;
                            if (box == null) throw new InvalidOperationException("Attendance checkbox is missing.");
                            idParam.Value = Convert.ToInt32(gvStudents.DataKeys[row.RowIndex].Value);
                            attendedParam.Value = box.Checked;
                            if (command.ExecuteNonQuery() != 1)
                            {
                                transaction.Rollback();
                                ShowMessage("The event or registration changed, or you do not have permission. No changes were saved.", false);
                                LoadStudents();
                                return;
                            }
                        }
                    }
                    transaction.Commit();
                }
                LoadStudents();
                ShowMessage("Attendance saved successfully.", true);
            }
            catch (SqlException)
            {
                ShowMessage("Unable to save attendance. Please try again or contact the administrator.", false);
            }
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.CssClass = "alert alert-" + (success ? "success" : "danger") + " d-block";
            lblMessage.Text = Server.HtmlEncode(message);
        }
    }
}
