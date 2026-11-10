using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace PRG3204_Project_Group4.Events
{
    public partial class Default : Page
    {
        private string ConnectionString { get { return ConfigurationManager.ConnectionStrings["CampusEventDB"].ConnectionString; } }
        protected void Page_Load(object sender, EventArgs e) { if (!IsPostBack) LoadEvents(); }
        private void LoadEvents()
        {
            // Compute remaining seats from active registrations; M2 must re-check capacity in a transaction.
            const string sql = @"SELECT e.EventId,e.Title,e.Description,e.Location,e.Capacity,c.CategoryName,
                DATEADD(HOUR,8,e.StartDateTime) StartMY,DATEADD(HOUR,8,e.EndDateTime) EndMY,
                e.Capacity-(SELECT COUNT(*) FROM dbo.Registrations r WHERE r.EventId=e.EventId AND r.Status='Registered') AS SeatsRemaining
                FROM dbo.Events e JOIN dbo.Categories c ON c.CategoryId=e.CategoryId
                WHERE e.Status='Upcoming' AND e.StartDateTime>SYSUTCDATETIME() ORDER BY e.StartDateTime";
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
                rptEvents.DataSource = dt; rptEvents.DataBind(); lblEmpty.Visible = dt.Rows.Count == 0;
            }
        }

        protected void rptEvents_ItemDataBound(
    object sender,
    System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType !=
                System.Web.UI.WebControls.ListItemType.Item &&
                e.Item.ItemType !=
                System.Web.UI.WebControls.ListItemType.AlternatingItem)
            {
                return;
            }

            var row = e.Item.DataItem as System.Data.DataRowView;

            if (row == null)
                return;

            int eventId = Convert.ToInt32(row["EventId"]);

            var rptFiles = e.Item.FindControl("rptFiles")
                as System.Web.UI.WebControls.Repeater;

            if (rptFiles != null)
            {
                rptFiles.DataSource = GetResources(eventId);
                rptFiles.DataBind();
            }
        }
        protected DataTable GetResources(int eventId)
        {
            const string sql = "SELECT FileName,FilePath FROM dbo.EventResources WHERE EventId=@EventId ORDER BY UploadedAt DESC";
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
                return dt;
            }
        }
    }
}