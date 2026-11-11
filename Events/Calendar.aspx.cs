using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PRG3204_Project_Group4.Events
{
    public partial class Calendar : Page
    {
        private HashSet<DateTime> dates = new HashSet<DateTime>();
        private string ConnectionString { get { return ConfigurationManager.ConnectionStrings["CampusEventDB"].ConnectionString; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            Helpers.EventStatus.UpdateStatuses();
            if (!IsPostBack) { calEvents.SelectedDate = DateTime.UtcNow.AddHours(8).Date; LoadDay(); }
            // DayRender runs after Load; always load the visible month's dates on each request.
            LoadMonthDates();
        }
        private void LoadMonthDates()
        {
            // Fetch an expanded window to allow for the grid's previous/next month days.
            DateTime first = calEvents.VisibleDate == DateTime.MinValue ? DateTime.UtcNow.AddHours(8).Date : calEvents.VisibleDate.Date;
            first = new DateTime(first.Year, first.Month, 1).AddDays(-7);
            DateTime end = first.AddMonths(2).AddDays(14);
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"SELECT DISTINCT CAST(DATEADD(HOUR,8,StartDateTime) AS DATE) AS EventDay FROM dbo.Events
                WHERE Status='Upcoming' AND StartDateTime>SYSUTCDATETIME() AND StartDateTime >= @StartUtc AND StartDateTime < @EndUtc", conn))
            {
                cmd.Parameters.Add("@StartUtc", SqlDbType.DateTime2).Value = first.AddHours(-8);
                cmd.Parameters.Add("@EndUtc", SqlDbType.DateTime2).Value = end.AddHours(-8);
                conn.Open(); using (SqlDataReader r = cmd.ExecuteReader()) while (r.Read()) dates.Add(r.GetDateTime(0).Date);
            }
        }
        protected void calEvents_DayRender(object sender, DayRenderEventArgs e)
        {
            if (dates.Contains(e.Day.Date))
            {
                e.Cell.BackColor = System.Drawing.Color.LightCyan;
                e.Cell.ToolTip = "Upcoming events on this day";
                e.Cell.Controls.Add(new LiteralControl("<div class='small text-primary'>Event</div>"));
            }
        }
        protected void calEvents_VisibleMonthChanged(object sender, MonthChangedEventArgs e)
        {
            calEvents.VisibleDate = e.NewDate; LoadMonthDates();
        }
        protected void calEvents_SelectionChanged(object sender, EventArgs e) { LoadDay(); }
        private void LoadDay()
        {
            DateTime day = calEvents.SelectedDate == DateTime.MinValue ? DateTime.UtcNow.AddHours(8).Date : calEvents.SelectedDate.Date;
            litSelectedDay.Text = "Events on " + day.ToString("dd MMM yyyy");
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"SELECT EventId,Title,Location,DATEADD(HOUR,8,StartDateTime) StartMY,
                DATEADD(HOUR,8,EndDateTime) EndMY FROM dbo.Events
                WHERE Status='Upcoming' AND StartDateTime>SYSUTCDATETIME() AND StartDateTime>=@StartUtc AND StartDateTime<@EndUtc ORDER BY StartDateTime", conn))
            {
                cmd.Parameters.Add("@StartUtc", SqlDbType.DateTime2).Value = day.AddHours(-8);
                cmd.Parameters.Add("@EndUtc", SqlDbType.DateTime2).Value = day.AddDays(1).AddHours(-8);
                DataTable dt = new DataTable(); using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
                rptDayEvents.DataSource = dt; rptDayEvents.DataBind(); lblEmptyDay.Visible = dt.Rows.Count == 0;
            }
        }
    }
}