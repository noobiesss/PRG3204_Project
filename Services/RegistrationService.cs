using System;
using System.Data;
using System.Data.SqlClient;
using PRG3204_Project_Group4.Helpers;

namespace PRG3204_Project_Group4.Services
{
    public sealed class RegistrationResult
    {
        public bool Success { get; private set; }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public int? RegistrationId { get; private set; }

        internal RegistrationResult(bool success, string code, string message, int? id)
        {
            Success = success;
            Code = code;
            Message = message;
            RegistrationId = id;
        }
    }

    public static class RegistrationService
    {
        // The page must derive userId from the authenticated Session, never from
        // a query string, hidden field, dropdown or other browser-supplied value.
        public static RegistrationResult Register(int userId, int eventId)
        {
            if (userId <= 0 || eventId <= 0)
                return Failure("InvalidRequest", "A valid student and event are required.");

            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int capacity;
                        // Serialize requests for this event before reading registrations.
                        // M3's seat-consuming operations must use the same event lock.
                        using (SqlCommand command = Command(connection, transaction, @"
SELECT Capacity, Status,
       CASE WHEN StartDateTime <= SYSUTCDATETIME() THEN 1 ELSE 0 END AS HasStarted
FROM dbo.Events WITH (UPDLOCK, HOLDLOCK)
WHERE EventId = @EventId;", userId, eventId))
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                reader.Close();
                                return Reject(transaction, "EventNotFound", "The event does not exist.");
                            }
                            capacity = reader.GetInt32(0);
                            bool open = string.Equals(reader.GetString(1), "Upcoming", StringComparison.OrdinalIgnoreCase)
                                && reader.GetInt32(2) == 0;
                            if (!open)
                            {
                                reader.Close();
                                return Reject(transaction, "EventClosed", "Registration is closed for this event.");
                            }
                        }

                        using (SqlCommand command = Command(connection, transaction, @"
SELECT COUNT(*) FROM dbo.Users
WHERE UserId = @UserId AND IsActive = 1 AND Role = N'Student';", userId, eventId))
                        {
                            if (Convert.ToInt32(command.ExecuteScalar()) != 1)
                                return Reject(transaction, "StudentRequired", "An active student account is required.");
                        }

                        int? existingId = null;
                        string existingStatus = null;
                        using (SqlCommand command = Command(connection, transaction, @"
SELECT RegistrationId, Status
FROM dbo.Registrations
WHERE EventId = @EventId AND UserId = @UserId;", userId, eventId))
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                existingId = reader.GetInt32(0);
                                existingStatus = reader.GetString(1);
                            }
                        }
                        if (string.Equals(existingStatus, "Registered", StringComparison.OrdinalIgnoreCase))
                            return Reject(transaction, "AlreadyRegistered", "You are already registered for this event.");
                        if (existingId.HasValue && !string.Equals(existingStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                            return Reject(transaction, "InvalidStatus", "The existing registration cannot be restored.");

                        using (SqlCommand command = Command(connection, transaction, @"
SELECT COUNT_BIG(*) FROM dbo.Registrations
WHERE EventId = @EventId AND Status = N'Registered';", userId, eventId))
                        {
                            if (Convert.ToInt64(command.ExecuteScalar()) >= capacity)
                                return Reject(transaction, "EventFull", "This event is full.");
                        }

                        int registrationId;
                        bool restoring = existingId.HasValue;
                        if (restoring)
                        {
                            registrationId = existingId.Value;
                            using (SqlCommand command = Command(connection, transaction, @"
UPDATE dbo.Registrations
SET Status = N'Registered', CancelledAt = NULL,
    UpdatedAt = SYSUTCDATETIME(), IsAttended = 0
WHERE RegistrationId = @RegistrationId AND UserId = @UserId
  AND EventId = @EventId AND Status = N'Cancelled';", userId, eventId))
                            {
                                command.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = registrationId;
                                if (command.ExecuteNonQuery() != 1)
                                    return Reject(transaction, "Conflict", "The registration changed. Please try again.");
                            }
                        }
                        else
                        {
                            using (SqlCommand command = Command(connection, transaction, @"
INSERT INTO dbo.Registrations (EventId, UserId, Status, IsAttended)
OUTPUT INSERTED.RegistrationId
VALUES (@EventId, @UserId, N'Registered', 0);", userId, eventId))
                                registrationId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        using (SqlCommand command = Command(connection, transaction, @"
INSERT INTO dbo.RegistrationLogs
    (RegistrationId, UserId, Action, OldStatus, NewStatus, OldEventId, NewEventId)
VALUES
    (@RegistrationId, @UserId, @Action, @OldStatus, N'Registered', @OldEventId, @EventId);", userId, eventId))
                        {
                            command.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = registrationId;
                            command.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = restoring ? "Reregistered" : "Registered";
                            command.Parameters.Add("@OldStatus", SqlDbType.NVarChar, 20).Value = restoring ? (object)"Cancelled" : DBNull.Value;
                            command.Parameters.Add("@OldEventId", SqlDbType.Int).Value = restoring ? (object)eventId : DBNull.Value;
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        return new RegistrationResult(true, restoring ? "Reregistered" : "Registered",
                            restoring ? "Your registration has been restored." : "Registration successful.", registrationId);
                    }
                    catch (SqlException exception)
                    {
                        RollbackSafely(transaction);
                        if (exception.Number == 1205 || exception.Number == 1222 || exception.Number == -2)
                            return Failure("Busy", "The system is busy. Check your registration history before trying again.");
                        if (exception.Number == 2601 || exception.Number == 2627)
                            return Failure("Conflict", "A matching record already exists. Check your registration history.");
                        throw;
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        // Callers must supply the authenticated student's ID from Session.
        public static DataTable GetHistory(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException("userId");
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            {
                connection.Open();
                using (SqlCommand check = new SqlCommand(@"
SELECT COUNT(*) FROM dbo.Users
WHERE UserId=@UserId AND IsActive=1 AND Role=N'Student';", connection))
                {
                    check.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    if (Convert.ToInt32(check.ExecuteScalar()) != 1)
                        throw new UnauthorizedAccessException("An active student account is required.");
                }
                using (SqlCommand command = new SqlCommand(@"
SELECT r.RegistrationId, r.EventId, e.Title, e.Location,
       e.StartDateTime, e.EndDateTime, e.Status AS EventStatus,
       r.RegisteredAt, r.UpdatedAt, r.CancelledAt,
       r.Status AS RegistrationStatus, r.IsAttended
FROM dbo.Registrations r
JOIN dbo.Events e ON e.EventId=r.EventId
JOIN dbo.Users u ON u.UserId=r.UserId
WHERE r.UserId=@UserId AND u.IsActive=1 AND u.Role=N'Student'
ORDER BY r.RegisteredAt DESC, r.RegistrationId DESC;", connection))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable history = new DataTable();
                        adapter.Fill(history);
                        return history;
                    }
                }
            }
        }

        private static SqlCommand Command(SqlConnection connection, SqlTransaction transaction,
            string sql, int userId, int eventId)
        {
            SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            command.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
            return command;
        }

        private static RegistrationResult Failure(string code, string message)
        {
            return new RegistrationResult(false, code, message, null);
        }

        private static RegistrationResult Reject(SqlTransaction transaction, string code, string message)
        {
            transaction.Rollback();
            return Failure(code, message);
        }

        private static void RollbackSafely(SqlTransaction transaction)
        {
            try { if (transaction.Connection != null) transaction.Rollback(); }
            catch (SqlException) { }
            catch (InvalidOperationException) { }
        }
    }
}
