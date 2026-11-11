using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PRG3204_Project_Group4.Reports
{
    // Staff/Admin feedback analytics only. Student submissions moved to Registrations/Feedback.aspx.
    public partial class Feedback : Page
    {
        private int UserId
        {
            get { int id; return Int32.TryParse(Convert.ToString(Session["UserId"]), out id) ? id : 0; }
        }
        private string UserRole { get { return Convert.ToString(Session["Role"]); } }
        private bool IsAdmin { get { return String.Equals(UserRole, "Admin", StringComparison.OrdinalIgnoreCase); } }
        private bool IsManager { get { return IsAdmin || String.Equals(UserRole, "Staff", StringComparison.OrdinalIgnoreCase); } }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ViewStateUserKey = Session.SessionID;
            if (UserId <= 0) { Response.Redirect("~/Account/Login.aspx", true); return; }
            if (!IsManager)
                throw new HttpException(403, "Only Staff/Admin can view feedback reports.");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            pnlManager.Visible = IsManager;
            if (!IsPostBack)
            {
                if (IsManager) { LoadEvents(); LoadResults(); }
            }
        }

        private SqlConnection Connect()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["CampusEventDb"];
            if (settings == null) throw new ConfigurationErrorsException("CampusEventDb is missing in Web.config.");
            SqlConnection connection = new SqlConnection(settings.ConnectionString);
            connection.Open();
            return connection;
        }

        private void AddScope(SqlCommand command)
        {
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
            command.Parameters.Add("@IsAdmin", SqlDbType.Bit).Value = IsAdmin;
        }

        private void LoadEvents()
        {
            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand(@"
SELECT EventId,Title FROM dbo.Events
WHERE (@IsAdmin=1 OR OrganizerId=@UserId)
ORDER BY StartDateTime DESC,Title", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                AddScope(command);
                DataTable events = new DataTable(); adapter.Fill(events);
                ddlEvent.DataSource = events;
                ddlEvent.DataTextField = "Title";
                ddlEvent.DataValueField = "EventId";
                ddlEvent.DataBind();
                ddlEvent.Items.Insert(0, new ListItem("All accessible events", "0"));
            }
        }

        private DataTable FetchManagerData(string sql)
        {
            int eventId;
            if (!Int32.TryParse(ddlEvent.SelectedValue, out eventId)) eventId = 0;
            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                AddScope(command);
                command.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                DataTable result = new DataTable(); adapter.Fill(result); return result;
            }
        }

        private const string ManagerFrom = @"
FROM dbo.Feedback f
JOIN dbo.Registrations r ON r.RegistrationId=f.RegistrationId
JOIN dbo.Events e ON e.EventId=r.EventId
WHERE (@IsAdmin=1 OR e.OrganizerId=@UserId)
  AND (@EventId=0 OR e.EventId=@EventId)";

        private void LoadResults()
        {
            DataRow summary = FetchManagerData(@"
SELECT COUNT(*) AS Responses,
       AVG(CAST(f.Rating AS decimal(10,2))) AS AverageRating,
       SUM(CASE WHEN f.WouldRecommend=1 THEN 1 ELSE 0 END) AS Recommended,
       COUNT(f.WouldRecommend) AS Answered" + ManagerFrom).Rows[0];
            int responses = Convert.ToInt32(summary["Responses"]);
            int answered = Convert.ToInt32(summary["Answered"]);
            int recommended = summary.IsNull("Recommended") ? 0 : Convert.ToInt32(summary["Recommended"]);
            lblResponses.Text = responses.ToString("N0");
            lblAverage.Text = responses == 0 ? "N/A" : Convert.ToDecimal(summary["AverageRating"]).ToString("0.0");
            lblRecommend.Text = answered == 0 ? "N/A" : (100.0 * recommended / answered).ToString("0.0") + "%";

            int[] starCounts = new int[5];
            DataTable ratingRows = FetchManagerData("SELECT f.Rating,COUNT(*) AS Responses " + ManagerFrom + " GROUP BY f.Rating");
            foreach (DataRow row in ratingRows.Rows)
            {
                int score = Convert.ToInt32(row["Rating"]);
                if (score >= 1 && score <= 5) starCounts[score-1] = Convert.ToInt32(row["Responses"]);
            }
            hidRatings.Value = new JavaScriptSerializer().Serialize(starCounts);

            gvEventRatings.DataSource = FetchManagerData(@"
SELECT e.Title, COUNT(*) AS Responses,
       CAST(AVG(CAST(f.Rating AS decimal(10,2))) AS decimal(4,1)) AS AverageRating
" + ManagerFrom + " GROUP BY e.EventId,e.Title ORDER BY Responses DESC,e.Title");
            gvEventRatings.DataBind();

            gvComments.DataSource = FetchManagerData(@"
SELECT TOP (200) e.Title, f.Rating, f.Comments,
       CASE WHEN f.WouldRecommend=1 THEN 'Yes' WHEN f.WouldRecommend=0 THEN 'No' ELSE 'N/A' END AS Recommendation,
       DATEADD(HOUR,8,f.SubmittedAt) AS SubmittedMalaysia
" + ManagerFrom + @"
  AND f.Comments IS NOT NULL AND LEN(LTRIM(RTRIM(f.Comments)))>0
ORDER BY f.SubmittedAt DESC");
            gvComments.DataBind();
        }

        protected void ResultsButton_Click(object sender, EventArgs e)
        {
            if (!IsManager) throw new HttpException(403, "Access denied.");
            gvComments.PageIndex = 0;
            LoadResults();
        }

        protected void CommentsGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (!IsManager) throw new HttpException(403, "Access denied.");
            gvComments.PageIndex = e.NewPageIndex;
            LoadResults();
        }

    }
}
