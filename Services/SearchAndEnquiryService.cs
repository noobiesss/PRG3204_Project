using System;
using System.Data;
using System.Data.SqlClient;
using PRG3204_Project_Group4.Helpers;

namespace PRG3204_Project_Group4.Services
{
    // M3: search upcoming events
    public static class EventSearchService
    {
        public static DataTable GetCategories()
        {
            return Query("SELECT CategoryId, CategoryName FROM dbo.Categories ORDER BY CategoryName;");
        }

        // Only staff/admin who actually organise at least one event
        public static DataTable GetOrganizers()
        {
            return Query(@"
SELECT u.UserId, u.FullName FROM dbo.Users u
WHERE u.IsActive = 1 AND u.Role IN (N'Staff', N'Admin')
  AND EXISTS (SELECT 1 FROM dbo.Events e WHERE e.OrganizerId = u.UserId)
ORDER BY u.FullName;");
        }

        // myDate is a calendar date in Malaysia time; each filter is optional
        public static DataTable Search(DateTime? myDate, int? categoryId, int? organizerId)
        {
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"
SELECT e.EventId, e.Title, c.CategoryName, u.FullName AS OrganizerName, e.Location,
       DATEADD(HOUR, 8, e.StartDateTime) AS StartMyt,
       DATEADD(HOUR, 8, e.EndDateTime) AS EndMyt,
       e.Capacity - (SELECT COUNT(*) FROM dbo.Registrations r
                     WHERE r.EventId = e.EventId AND r.Status = N'Registered') AS SeatsLeft
FROM dbo.Events e
JOIN dbo.Categories c ON c.CategoryId = e.CategoryId
JOIN dbo.Users u ON u.UserId = e.OrganizerId
WHERE e.Status = N'Upcoming'
  AND (@CategoryId IS NULL OR e.CategoryId = @CategoryId)
  AND (@OrganizerId IS NULL OR e.OrganizerId = @OrganizerId)
  AND (@FromUtc IS NULL OR (e.StartDateTime >= @FromUtc AND e.StartDateTime < @ToUtc))
ORDER BY e.StartDateTime;", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                // Malaysia day 00:00 = UTC previous day 16:00
                DateTime? fromUtc = myDate.HasValue ? myDate.Value.Date.AddHours(-8) : (DateTime?)null;
                DateTime? toUtc = fromUtc.HasValue ? fromUtc.Value.AddDays(1) : (DateTime?)null;

                command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = categoryId.HasValue ? (object)categoryId.Value : DBNull.Value;
                command.Parameters.Add("@OrganizerId", SqlDbType.Int).Value = organizerId.HasValue ? (object)organizerId.Value : DBNull.Value;
                command.Parameters.Add("@FromUtc", SqlDbType.DateTime2).Value = fromUtc.HasValue ? (object)fromUtc.Value : DBNull.Value;
                command.Parameters.Add("@ToUtc", SqlDbType.DateTime2).Value = toUtc.HasValue ? (object)toUtc.Value : DBNull.Value;

                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        internal static DataTable Query(string sql)
        {
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connection))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }
    }

    // M3: contact organizer (Enquiries table)
    public static class EnquiryService
    {
        public static DataTable GetUpcomingEvents()
        {
            return EventSearchService.Query(
                "SELECT EventId, Title FROM dbo.Events WHERE Status = N'Upcoming' ORDER BY StartDateTime;");
        }

        // Returns null on success, otherwise the error message to show.
        // userId must come from Session. If an event is chosen, the organiser is taken from that event.
        public static string Submit(int userId, int? eventId, int? organizerId, string subject, string message)
        {
            if (userId <= 0) return "Please log in again.";
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
                return "Subject and message are required.";
            subject = subject.Trim();
            message = message.Trim();
            if (subject.Length > 150) return "Subject must be 150 characters or fewer.";
            if (message.Length > 1000) return "Message must be 1000 characters or fewer.";

            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            {
                connection.Open();
                int resolvedOrganizer;

                if (eventId.HasValue)
                {
                    using (SqlCommand command = new SqlCommand("SELECT OrganizerId FROM dbo.Events WHERE EventId = @EventId;", connection))
                    {
                        command.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId.Value;
                        object value = command.ExecuteScalar();
                        if (value == null) return "The selected event does not exist.";
                        resolvedOrganizer = Convert.ToInt32(value);
                    }
                }
                else if (organizerId.HasValue)
                {
                    using (SqlCommand command = new SqlCommand(@"
SELECT COUNT(*) FROM dbo.Users WHERE UserId = @OrganizerId AND IsActive = 1 AND Role IN (N'Staff', N'Admin');", connection))
                    {
                        command.Parameters.Add("@OrganizerId", SqlDbType.Int).Value = organizerId.Value;
                        if (Convert.ToInt32(command.ExecuteScalar()) != 1) return "The selected organiser is not valid.";
                    }
                    resolvedOrganizer = organizerId.Value;
                }
                else
                {
                    return "Please choose an event or an organiser.";
                }

                using (SqlCommand insert = new SqlCommand(@"
INSERT INTO dbo.Enquiries (UserId, EventId, OrganizerId, Subject, Message)
VALUES (@UserId, @EventId, @OrganizerId, @Subject, @Message);", connection))
                {
                    insert.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    insert.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId.HasValue ? (object)eventId.Value : DBNull.Value;
                    insert.Parameters.Add("@OrganizerId", SqlDbType.Int).Value = resolvedOrganizer;
                    insert.Parameters.Add("@Subject", SqlDbType.NVarChar, 150).Value = subject;
                    insert.Parameters.Add("@Message", SqlDbType.NVarChar, 1000).Value = message;
                    insert.ExecuteNonQuery();
                }
            }
            return null;
        }

        public static DataTable GetMyEnquiries(int userId)
        {
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"
SELECT Subject, Status, DATEADD(HOUR, 8, SubmittedAt) AS SubmittedMyt, OrganizerReply
FROM dbo.Enquiries WHERE UserId = @UserId ORDER BY SubmittedAt DESC;", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }
    }
}