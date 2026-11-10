using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using PRG3204_Project_Group4.Helpers;

namespace PRG3204_Project_Group4.Services
{
    // Call these after the registration change is saved.
    // A notification problem is recorded and never stops the registration itself.
    public static class NotificationService
    {
        private const int SubjectMaxLength = 150;
        private const int SmtpTimeoutMilliseconds = 10000;

        public static void RegistrationConfirmed(int userId, int eventId)
        {
            NotifyRegistration(userId, eventId, "Registration confirmed", "Your registration for this event is confirmed.");
        }

        public static void RegistrationUpdated(int userId, int eventId)
        {
            NotifyRegistration(userId, eventId, "Registration updated", "Your registration for this event has been updated.");
        }

        public static void RegistrationCancelled(int userId, int eventId)
        {
            NotifyRegistration(userId, eventId, "Registration cancelled", "Your registration for this event has been cancelled.");
        }

        private static void NotifyRegistration(int userId, int eventId, string heading, string intro)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.CreateConnection())
                {
                    connection.Open();

                    string recipient, subject, message;
                    using (SqlCommand details = new SqlCommand(@"
SELECT u.FullName, COALESCE(NULLIF(u.PersonalEmail, N''), u.SchoolEmail), e.Title, e.Location, e.StartDateTime
FROM dbo.Users u CROSS JOIN dbo.Events e
WHERE u.UserId = @UserId AND e.EventId = @EventId;", connection))
                    {
                        details.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        details.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                        using (SqlDataReader reader = details.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Trace.TraceWarning("Notification skipped: user {0} or event {1} was not found.", userId, eventId);
                                return;
                            }

                            string title = reader.GetString(2);
                            recipient = reader.GetString(1);
                            subject = BuildSubject(heading, title);
                            message = "Hi " + reader.GetString(0) + ",\r\n\r\n" + intro + "\r\n\r\n"
                                + "Event: " + title + "\r\n"
                                + "Date: " + FormatMalaysiaTime(reader.GetDateTime(4)) + " (Malaysia time)\r\n"
                                + "Location: " + reader.GetString(3) + "\r\n\r\n"
                                + "Campus Event Management System";
                        }
                    }

                    int notificationId;
                    using (SqlCommand insert = new SqlCommand(@"
INSERT INTO dbo.Notifications (UserId, EventId, Subject, Message)
OUTPUT INSERTED.NotificationId
VALUES (@UserId, @EventId, @Subject, @Message);", connection))
                    {
                        insert.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        insert.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                        insert.Parameters.Add("@Subject", SqlDbType.NVarChar, SubjectMaxLength).Value = subject;
                        insert.Parameters.Add("@Message", SqlDbType.NVarChar, 1000).Value = message;
                        notificationId = (int)insert.ExecuteScalar();
                    }

                    // Do not keep the database connection open while waiting for the mail server
                    connection.Close();
                    bool sent = TrySendEmail(recipient, subject, message);
                    connection.Open();

                    using (SqlCommand update = new SqlCommand(@"
UPDATE dbo.Notifications
SET Status = @Status, SentAt = CASE WHEN @Status = N'Sent' THEN SYSUTCDATETIME() ELSE NULL END
WHERE NotificationId = @NotificationId;", connection))
                    {
                        update.Parameters.Add("@Status", SqlDbType.NVarChar, 10).Value = sent ? "Sent" : "Failed";
                        update.Parameters.Add("@NotificationId", SqlDbType.Int).Value = notificationId;
                        update.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError("Notification could not be recorded: {0}", exception);
            }
        }

        private static bool TrySendEmail(string recipient, string subject, string message)
        {
            string host = GetSetting("SmtpHost");
            string from = GetSetting("SmtpFrom");
            if (host == null || from == null)
            {
                Trace.TraceWarning("Email was not sent because Email.config is not set up.");
                return false;
            }

            int port;
            if (!int.TryParse(GetSetting("SmtpPort"), out port)) port = 587;

            try
            {
                using (MailMessage mail = new MailMessage(from, recipient, subject, message))
                using (SmtpClient client = new SmtpClient(host, port))
                {
                    mail.SubjectEncoding = Encoding.UTF8;
                    mail.BodyEncoding = Encoding.UTF8;
                    client.Timeout = SmtpTimeoutMilliseconds;
                    client.EnableSsl = !string.Equals(GetSetting("SmtpEnableSsl"), "false", StringComparison.OrdinalIgnoreCase);

                    string user = GetSetting("SmtpUser");
                    if (user != null)
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(user, GetSetting("SmtpPassword"));
                    }

                    client.Send(mail);
                }
                return true;
            }
            catch (Exception exception)
            {
                Trace.TraceError("Email could not be sent: {0}", exception);
                return false;
            }
        }

        private static string BuildSubject(string heading, string eventTitle)
        {
            string subject = heading + ": " + eventTitle.Replace("\r", " ").Replace("\n", " ");
            return subject.Length <= SubjectMaxLength ? subject : subject.Substring(0, SubjectMaxLength);
        }

        private static string FormatMalaysiaTime(DateTime utcValue)
        {
            return utcValue.AddHours(8).ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture);
        }

        private static string GetSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
