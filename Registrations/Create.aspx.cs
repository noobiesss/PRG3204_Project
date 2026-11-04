using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Cryptography;
using PRG3204_Project_Group4.Helpers;
using PRG3204_Project_Group4.Services;

namespace PRG3204_Project_Group4.Registrations
{
    public partial class Create : System.Web.UI.Page
    {
        private const string TokenKey = "M2RegistrationCsrf";
        protected bool CanShowEvent { get; private set; }
        protected bool NeedsLogin { get; private set; }
        protected string Notice { get; private set; }
        protected string NoticeClass { get; private set; }
        protected string EventTitle { get; private set; }
        protected string EventLocation { get; private set; }
        protected string EventStatus { get; private set; }
        protected DateTime EventStart { get; private set; }
        protected DateTime EventEnd { get; private set; }
        protected int Capacity { get; private set; }
        protected long SeatsRemaining { get; private set; }
        private int currentEventId;

        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            NoticeClass = "alert-info";
            btnRegister.Visible = false;
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            if (!IsPostBack)
            {
                if (Session[TokenKey] == null)
                {
                    byte[] bytes = new byte[32];
                    using (RandomNumberGenerator random = RandomNumberGenerator.Create()) random.GetBytes(bytes);
                    Session[TokenKey] = Convert.ToBase64String(bytes);
                }
                csrfToken.Value = Convert.ToString(Session[TokenKey]);
            }
            if (!int.TryParse(Request.QueryString["eventId"], out currentEventId) || currentEventId <= 0)
            {
                Notice = "Select an event from the event list to continue.";
                return;
            }
            try { LoadEventAndEligibility(); }
            catch (SqlException exception) { ShowDatabaseError(exception); }
        }

        private void LoadEventAndEligibility()
        {
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(@"
SELECT e.Title,e.Location,e.Status,e.StartDateTime,e.EndDateTime,e.Capacity,
       (SELECT COUNT_BIG(*) FROM dbo.Registrations r
        WHERE r.EventId=e.EventId AND r.Status=N'Registered') AS RegisteredCount,
       CASE WHEN e.StartDateTime<=SYSUTCDATETIME() THEN 1 ELSE 0 END AS HasStarted
FROM dbo.Events e WHERE e.EventId=@EventId;", connection))
                {
                    command.Parameters.Add("@EventId", SqlDbType.Int).Value = currentEventId;
                    bool started;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Notice = "The selected event could not be found.";
                            Response.StatusCode = 404;
                            Response.TrySkipIisCustomErrors = true;
                            return;
                        }
                        EventTitle = reader.GetString(0);
                        EventLocation = reader.GetString(1);
                        EventStatus = reader.GetString(2);
                        EventStart = reader.GetDateTime(3);
                        EventEnd = reader.GetDateTime(4);
                        Capacity = reader.GetInt32(5);
                        SeatsRemaining = Math.Max(0L, (long)Capacity - reader.GetInt64(6));
                        started = reader.GetInt32(7) != 0;
                        CanShowEvent = true;
                    }
                    int userId;
                    if (!TryGetStudent(out userId)) return;

                    using (SqlCommand account = new SqlCommand(@"
SELECT COUNT(*) FROM dbo.Users WHERE UserId=@UserId AND IsActive=1 AND Role=N'Student';", connection))
                    {
                        account.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        if (Convert.ToInt32(account.ExecuteScalar()) != 1)
                        {
                            DenyStudent();
                            return;
                        }
                    }
                    if (started || !string.Equals(EventStatus, "Upcoming", StringComparison.OrdinalIgnoreCase))
                    {
                        Notice = "Registration is closed for this event.";
                        return;
                    }
                    object status;
                    using (SqlCommand existing = new SqlCommand(@"
SELECT Status FROM dbo.Registrations WHERE EventId=@EventId AND UserId=@UserId;", connection))
                    {
                        existing.Parameters.Add("@EventId", SqlDbType.Int).Value = currentEventId;
                        existing.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        status = existing.ExecuteScalar();
                    }
                    if (string.Equals(Convert.ToString(status), "Registered", StringComparison.OrdinalIgnoreCase))
                    {
                        Notice = "You are already registered for this event. View it in My Registrations.";
                        return;
                    }
                    if (SeatsRemaining == 0)
                    {
                        Notice = "This event is full.";
                        return;
                    }
                    btnRegister.Text = string.Equals(Convert.ToString(status), "Cancelled", StringComparison.OrdinalIgnoreCase)
                        ? "Confirm Re-registration" : "Confirm Registration";
                    btnRegister.Visible = true;
                }
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            // Validate every POST, including identity, event eligibility and CSRF.
            int userId;
            if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)
                || !TryGetStudent(out userId)) return;
            string expected = Session[TokenKey] as string;
            if (string.IsNullOrEmpty(expected) || !string.Equals(expected, csrfToken.Value, StringComparison.Ordinal))
            {
                Notice = "Your request could not be verified. Reopen the event page and try again.";
                NoticeClass = "alert-warning";
                btnRegister.Visible = false;
                return;
            }
            if (!CanShowEvent || !btnRegister.Visible || currentEventId <= 0) return;
            RegistrationResult result;
            try { result = RegistrationService.Register(userId, currentEventId); }
            catch (SqlException exception) { ShowDatabaseError(exception); return; }
            if (result.Success)
            {
                // POST-Redirect-GET: refreshing history cannot submit another booking.
                Response.Redirect(ResolveUrl("~/Registrations/Default.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            Notice = result.Message;
            NoticeClass = "alert-warning";
            btnRegister.Visible = false;
        }

        private bool TryGetStudent(out int userId)
        {
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out userId) || userId <= 0)
            {
                NeedsLogin = true;
                Notice = "Please sign in to register for this event.";
                return false;
            }
            if (!string.Equals(Convert.ToString(Session["Role"]), "Student", StringComparison.OrdinalIgnoreCase))
            {
                DenyStudent();
                return false;
            }
            return true;
        }

        private void DenyStudent()
        {
            Notice = "An active student account is required to register.";
            Response.StatusCode = 403;
            Response.TrySkipIisCustomErrors = true;
        }

        private void ShowDatabaseError(SqlException exception)
        {
            System.Diagnostics.Trace.TraceError("M2 confirmation page database error: {0}", exception);
            Notice = "The request could not be completed. Check My Registrations before trying again.";
            NoticeClass = "alert-warning";
            btnRegister.Visible = false;
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
        }

        protected string FormatDate(DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc).AddHours(8)
                .ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
