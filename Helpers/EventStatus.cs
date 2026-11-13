using System.Configuration;
using System.Data.SqlClient;

namespace PRG3204_Project_Group4.Helpers
{
    /// <summary>
    /// Synchronizes Upcoming/Ongoing event states using database UTC time.
    /// Cancelled and already Completed events are never changed.
    /// </summary>
    public static class EventStatus
    {
        public static void UpdateStatuses()
        {
            const string sql = @"
UPDATE dbo.Events
SET Status = CASE WHEN EndDateTime <= SYSUTCDATETIME() THEN 'Completed' ELSE 'Ongoing' END,
    UpdatedAt = SYSUTCDATETIME()
WHERE Status IN ('Upcoming', 'Ongoing')
  AND StartDateTime <= SYSUTCDATETIME()
  AND Status <> CASE WHEN EndDateTime <= SYSUTCDATETIME() THEN 'Completed' ELSE 'Ongoing' END;";

            string cs = ConfigurationManager.ConnectionStrings["CampusEventDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
