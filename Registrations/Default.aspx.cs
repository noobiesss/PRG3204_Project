using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using PRG3204_Project_Group4.Services;

namespace PRG3204_Project_Group4.Registrations
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            pnlAccess.Visible = false;
            pnlEmpty.Visible = false;
            pnlHistory.Visible = false;
            lnkLogin.Visible = false;

            int userId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out userId) || userId <= 0)
            {
                ShowAccessMessage("Please sign in to view your registrations.");
                lnkLogin.Visible = true;
                return;
            }
            if (!string.Equals(Convert.ToString(Session["Role"]), "Student", StringComparison.OrdinalIgnoreCase))
            {
                DenyAccess();
                return;
            }
            try
            {
                DataTable history = RegistrationService.GetHistory(userId);
                if (history.Rows.Count == 0)
                {
                    pnlEmpty.Visible = true;
                    return;
                }
                litTotal.Text = history.Rows.Count.ToString(CultureInfo.InvariantCulture);
                rptHistory.DataSource = history;
                rptHistory.DataBind();
                pnlHistory.Visible = true;
            }
            catch (UnauthorizedAccessException)
            {
                DenyAccess();
            }
            catch (SqlException exception)
            {
                System.Diagnostics.Trace.TraceError("M2 history database error: {0}", exception);
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
                ShowAccessMessage("Your registrations could not be loaded. Please try again later.");
            }
        }

        private void ShowAccessMessage(string message)
        {
            litAccess.Text = message;
            pnlAccess.Visible = true;
        }

        private void DenyAccess()
        {
            Response.StatusCode = 403;
            Response.TrySkipIisCustomErrors = true;
            ShowAccessMessage("This page is available to active student accounts only.");
        }

        protected string FormatDate(object value)
        {
            if (value == null || value == DBNull.Value) return "-";
            DateTime utc = DateTime.SpecifyKind(Convert.ToDateTime(value), DateTimeKind.Utc);
            return utc.AddHours(8).ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture);
        }

        protected bool IsCancelled(object value)
        {
            return string.Equals(Convert.ToString(value), "Cancelled", StringComparison.OrdinalIgnoreCase);
        }

        protected string FormatAttendance(object value)
        {
            return Convert.ToBoolean(value) ? "Attended" : "Not marked";
        }
    }
}
