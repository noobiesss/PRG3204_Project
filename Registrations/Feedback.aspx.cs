using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PRG3204_Project_Group4.Registrations
{
    // Student feedback belongs under Registrations, NOT Staff/Admin-only Reports.
    public partial class Feedback : Page
    {
        private int CurrentUserId
        {
            get
            {
                int userId;
                return Int32.TryParse(Convert.ToString(Session["UserId"]), out userId) ? userId : 0;
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
            if (!String.Equals(Convert.ToString(Session["Role"]), "Student", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/AccessDenied.aspx", true);
                return;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadEligibleRegistrations();
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

        private void LoadEligibleRegistrations()
        {
            const string sql = @"
SELECT r.RegistrationId, e.Title, e.StartDateTime
FROM dbo.Registrations AS r
JOIN dbo.Events AS e ON e.EventId = r.EventId
LEFT JOIN dbo.Feedback AS f ON f.RegistrationId = r.RegistrationId
WHERE r.UserId = @UserId AND r.Status = 'Registered'
  AND e.Status <> 'Cancelled' AND e.EndDateTime <= SYSUTCDATETIME()
  AND f.FeedbackId IS NULL
ORDER BY e.EndDateTime DESC;";
            ddlRegistration.Items.Clear();
            ddlRegistration.Items.Add(new ListItem("-- Choose a completed event --", ""));
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = CurrentUserId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime utcStart = DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc);
                        string title = reader.GetString(1) + " (" + utcStart.AddHours(8).ToString("dd MMM yyyy HH:mm") + " MYT)";
                        ddlRegistration.Items.Add(new ListItem(title, reader.GetInt32(0).ToString()));
                    }
                }
            }
            int requested;
            if (Int32.TryParse(Request.QueryString["registrationId"], out requested))
            {
                ListItem item = ddlRegistration.Items.FindByValue(requested.ToString());
                if (item != null) ddlRegistration.SelectedValue = item.Value;
            }
            pnlForm.Visible = ddlRegistration.Items.Count > 1;
            lblNoEvents.Visible = !pnlForm.Visible;
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (CurrentUserId <= 0 || !String.Equals(Convert.ToString(Session["Role"]), "Student", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/AccessDenied.aspx", true);
                return;
            }

            int registrationId, rating;
            if (!Int32.TryParse(ddlRegistration.SelectedValue, out registrationId) ||
                !Int32.TryParse(ddlRating.SelectedValue, out rating) || rating < 1 || rating > 5)
            {
                ShowMessage("Please select an event and a rating between 1 and 5.", false);
                return;
            }
            string comments = (txtComments.Text ?? String.Empty).Trim();
            if (comments.Length > 1000)
            {
                ShowMessage("Comments must contain 1,000 characters or fewer.", false);
                return;
            }
            // ValidateRequestMode="Disabled" on txtComments stops a raw <b> input from causing HTTP 500.
            // Reject markup in the application with a friendly message; always HTML-encode output.
            if (comments.IndexOf('<') >= 0 || comments.IndexOf('>') >= 0)
            {
                ShowMessage("Please use plain text only. HTML tags such as <b> are not allowed.", false);
                return;
            }
            object recommend = DBNull.Value;
            if (ddlRecommend.SelectedValue == "1") recommend = true;
            else if (ddlRecommend.SelectedValue == "0") recommend = false;
            else if (ddlRecommend.SelectedValue != "")
            {
                ShowMessage("Invalid recommendation choice.", false);
                return;
            }

            const string sql = @"
INSERT INTO dbo.Feedback (RegistrationId, Rating, Comments, WouldRecommend, SubmittedAt)
SELECT r.RegistrationId, @Rating, @Comments, @WouldRecommend, SYSUTCDATETIME()
FROM dbo.Registrations AS r
JOIN dbo.Events AS e ON e.EventId = r.EventId
WHERE r.RegistrationId = @RegistrationId
  AND r.UserId = @UserId
  AND r.Status = 'Registered'
  AND e.Status <> 'Cancelled'
  AND e.EndDateTime <= SYSUTCDATETIME()
  AND NOT EXISTS (SELECT 1 FROM dbo.Feedback AS f WHERE f.RegistrationId = r.RegistrationId);";
            try
            {
                int affected;
                using (SqlConnection connection = OpenConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = registrationId;
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = CurrentUserId;
                    command.Parameters.Add("@Rating", SqlDbType.Int).Value = rating;
                    command.Parameters.Add("@Comments", SqlDbType.NVarChar, 1000).Value = comments.Length == 0 ? (object)DBNull.Value : comments;
                    command.Parameters.Add("@WouldRecommend", SqlDbType.Bit).Value = recommend;
                    affected = command.ExecuteNonQuery();
                }
                if (affected != 1)
                {
                    ShowMessage("The selected registration is not eligible, or feedback was already submitted.", false);
                    LoadEligibleRegistrations();
                    return;
                }
                LoadEligibleRegistrations();
                ddlRating.SelectedIndex = 0;
                ddlRecommend.SelectedIndex = 0;
                txtComments.Text = String.Empty;
                ShowMessage("Thank you! Your feedback has been submitted.", true);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    ShowMessage("Feedback was already submitted for this event.", false);
                    LoadEligibleRegistrations();
                    return;
                }
                ShowMessage("Unable to submit feedback right now. Please try again later.", false);
            }
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.CssClass = "alert alert-" + (success ? "success" : "danger") + " d-block";
            lblMessage.Text = Server.HtmlEncode(message);
        }
    }
}
