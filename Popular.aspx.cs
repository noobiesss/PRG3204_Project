using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PRG3204_Project_Group4.Reports
{
    public partial class Popular : Page
    {
        // Popularity = number of currently registered participants, not cancelled registrations.
        // Staff may see only events where they are the organizer.
        private const string PopularSql = @"
SELECT e.EventId, e.Title, c.CategoryName,
       DATEADD(HOUR,8,e.StartDateTime) AS StartMalaysia, e.Capacity,
       ISNULL(x.RegisteredCount,0) AS RegisteredCount,
       ISNULL(x.AttendedCount,0) AS AttendedCount,
       CAST(100.0*ISNULL(x.RegisteredCount,0)/NULLIF(e.Capacity,0) AS decimal(6,1)) AS FillRate
FROM dbo.Events e
JOIN dbo.Categories c ON c.CategoryId=e.CategoryId
LEFT JOIN (
    SELECT EventId, COUNT(*) AS RegisteredCount,
           SUM(CASE WHEN IsAttended=1 THEN 1 ELSE 0 END) AS AttendedCount
    FROM dbo.Registrations WHERE Status='Registered' GROUP BY EventId
) x ON x.EventId=e.EventId
WHERE e.Status<>'Cancelled'
  AND (@IsAdmin=1 OR e.OrganizerId=@UserId)
  AND (@CategoryId=0 OR e.CategoryId=@CategoryId)
  AND (@Days=0 OR e.StartDateTime>=DATEADD(DAY,-@Days,SYSUTCDATETIME()))
ORDER BY ISNULL(x.RegisteredCount,0) DESC, e.StartDateTime DESC, e.EventId DESC";

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
                throw new HttpException(403, "Only staff and admins may view analytics.");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) { LoadCategories(); LoadAnalytics(); }
        }

        private SqlConnection Connect()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["CampusEventsDb"];
            if (settings == null) throw new ConfigurationErrorsException("CampusEventsDb is missing in Web.config.");
            SqlConnection connection = new SqlConnection(settings.ConnectionString);
            connection.Open();
            return connection;
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

        private void LoadAnalytics()
        {
            int categoryId, days;
            if (!Int32.TryParse(ddlCategory.SelectedValue, out categoryId)) categoryId = 0;
            if (!Int32.TryParse(ddlDays.SelectedValue, out days)) days = 0;
            DataTable all = new DataTable();
            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand(PopularSql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                command.Parameters.Add("@IsAdmin", SqlDbType.Bit).Value = IsAdmin;
                command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = categoryId;
                command.Parameters.Add("@Days", SqlDbType.Int).Value = days;
                adapter.Fill(all);
            }

            int registeredTotal = 0, attendedTotal = 0, totalCapacity = 0;
            Dictionary<string, int> categoryCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (DataRow row in all.Rows)
            {
                int registered = Convert.ToInt32(row["RegisteredCount"]);
                registeredTotal += registered;
                attendedTotal += Convert.ToInt32(row["AttendedCount"]);
                totalCapacity += Convert.ToInt32(row["Capacity"]);
                string name = Convert.ToString(row["CategoryName"]);
                if (!categoryCounts.ContainsKey(name)) categoryCounts.Add(name, 0);
                categoryCounts[name] += registered;
            }
            lblEvents.Text = all.Rows.Count.ToString("N0");
            lblRegistrations.Text = registeredTotal.ToString("N0");
            lblAttended.Text = attendedTotal.ToString("N0");
            lblFillRate.Text = totalCapacity == 0 ? "0%" : (100.0 * registeredTotal / totalCapacity).ToString("0.0") + "%";

            DataTable topTen = all.Clone();
            for (int i = 0; i < Math.Min(10, all.Rows.Count); i++) topTen.ImportRow(all.Rows[i]);
            gvPopular.DataSource = topTen;
            gvPopular.DataBind();

            var serializer = new JavaScriptSerializer();
            hidLabels.Value = serializer.Serialize(topTen.AsEnumerable().Select(r => Convert.ToString(r["Title"])).ToArray());
            hidRegistered.Value = serializer.Serialize(topTen.AsEnumerable().Select(r => Convert.ToInt32(r["RegisteredCount"])).ToArray());
            hidAttended.Value = serializer.Serialize(topTen.AsEnumerable().Select(r => Convert.ToInt32(r["AttendedCount"])).ToArray());
            var sortedCategories = categoryCounts.Where(x => x.Value > 0).OrderByDescending(x => x.Value).ToArray();
            hidCategoryLabels.Value = serializer.Serialize(sortedCategories.Select(x => x.Key).ToArray());
            hidCategoryTotals.Value = serializer.Serialize(sortedCategories.Select(x => x.Value).ToArray());
        }

        protected void ApplyButton_Click(object sender, EventArgs e)
        {
            LoadAnalytics();
        }
    }
}
