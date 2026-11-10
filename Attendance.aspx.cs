using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PRG3204_Project_Group4.Reports
{
    public partial class Attendance : Page
    {
        // UTC is stored in SQL Server. DATEADD converts UTC -> Malaysia Time for display.
        // Only Registered records count as active registrations; IsAttended denotes real attendance.
        private const string SummarySql = @"
SELECT e.Title, c.CategoryName, DATEADD(HOUR, 8, e.StartDateTime) AS StartMalaysia,
       e.Capacity, e.Status,
       ISNULL(x.RegisteredCount,0) AS RegisteredCount,
       ISNULL(x.AttendedCount,0) AS AttendedCount,
       ISNULL(x.CancelledCount,0) AS CancelledCount,
       CASE WHEN e.Capacity > ISNULL(x.RegisteredCount,0)
            THEN e.Capacity-ISNULL(x.RegisteredCount,0) ELSE 0 END AS RemainingSeats,
       CAST(CASE WHEN ISNULL(x.RegisteredCount,0)=0 THEN 0
                 ELSE 100.0*ISNULL(x.AttendedCount,0)/x.RegisteredCount END AS decimal(6,1)) AS AttendanceRate
FROM dbo.Events e
JOIN dbo.Categories c ON c.CategoryId=e.CategoryId
LEFT JOIN (
    SELECT EventId,
      SUM(CASE WHEN Status='Registered' THEN 1 ELSE 0 END) AS RegisteredCount,
      SUM(CASE WHEN Status='Registered' AND IsAttended=1 THEN 1 ELSE 0 END) AS AttendedCount,
      SUM(CASE WHEN Status='Cancelled' THEN 1 ELSE 0 END) AS CancelledCount
    FROM dbo.Registrations GROUP BY EventId
) x ON x.EventId=e.EventId
WHERE (@IsAdmin=1 OR e.OrganizerId=@UserId)
  AND (@CategoryId=0 OR e.CategoryId=@CategoryId)
  AND (@Status='' OR e.Status=@Status)
  AND (@Search='' OR e.Title LIKE '%' + @Search + '%')
ORDER BY e.StartDateTime DESC, e.EventId DESC";

        private int UserId
        {
            get { int id; return Int32.TryParse(Convert.ToString(Session["UserId"]), out id) ? id : 0; }
        }
        private bool IsAdmin
        {
            get { return String.Equals(Convert.ToString(Session["Role"]), "Admin", StringComparison.OrdinalIgnoreCase); }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (UserId <= 0) { Response.Redirect("~/Account/Login.aspx", true); return; }
            string role = Convert.ToString(Session["Role"]);
            if (!IsAdmin && !String.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase))
                throw new HttpException(403, "Only staff and admins may view attendance reports.");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) { LoadCategories(); LoadReport(); }
        }

        private SqlConnection Connect()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["CampusEventsDb"];
            if (settings == null) throw new ConfigurationErrorsException("CampusEventsDb is missing in Web.config.");
            SqlConnection connection = new SqlConnection(settings.ConnectionString);
            connection.Open();
            return connection;
        }

        private void AddScope(SqlCommand command)
        {
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
            command.Parameters.Add("@IsAdmin", SqlDbType.Bit).Value = IsAdmin;
        }

        private void LoadCategories()
        {
            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand("SELECT CategoryId,CategoryName FROM dbo.Categories ORDER BY CategoryName", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataTable categories = new DataTable(); adapter.Fill(categories);
                ddlCategory.DataSource = categories;
                ddlCategory.DataTextField = "CategoryName";
                ddlCategory.DataValueField = "CategoryId";
                ddlCategory.DataBind();
                ddlCategory.Items.Insert(0, new ListItem("All categories", "0"));
            }
        }

        private DataTable FetchRows()
        {
            int categoryId;
            if (!Int32.TryParse(ddlCategory.SelectedValue, out categoryId)) categoryId = 0;
            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand(SummarySql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                AddScope(command);
                command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = categoryId;
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = ddlStatus.SelectedValue;
                command.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = txtSearch.Text.Trim();
                DataTable rows = new DataTable(); adapter.Fill(rows); return rows;
            }
        }

        private void LoadReport()
        {
            DataTable rows = FetchRows();
            gvAttendance.DataSource = rows;
            gvAttendance.DataBind();
            int active = 0, attended = 0;
            foreach (DataRow row in rows.Rows)
            {
                active += Convert.ToInt32(row["RegisteredCount"]);
                attended += Convert.ToInt32(row["AttendedCount"]);
            }
            lblEventCount.Text = rows.Rows.Count.ToString("N0");
            lblRegistered.Text = active.ToString("N0");
            lblAttended.Text = attended.ToString("N0");
            lblAttendanceRate.Text = active == 0 ? "0%" : (100.0 * attended / active).ToString("0.0") + "%";
        }

        protected void FilterButton_Click(object sender, EventArgs e)
        {
            gvAttendance.PageIndex = 0;
            LoadReport();
        }

        protected void AttendanceGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAttendance.PageIndex = e.NewPageIndex;
            LoadReport();
        }
    }
}
