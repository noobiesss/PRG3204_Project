using System;
using System.Data;
using System.Data.SqlClient;
using PRG3204_Project_Group4.Helpers;

namespace PRG3204_Project_Group4.Services
{
    // M3: cancel or change (move to another event) a student's registration, plus logs.
    // Seat rule: every operation that can take a seat locks the Events row
    // WITH (UPDLOCK, HOLDLOCK) inside a Serializable transaction, the same as M2.
    public static class RegistrationChangeService
    {
        private const int ReasonMaxLength = 500;

        private sealed class EventInfo
        {
            public int Capacity;
            public string Status;
            public bool HasStarted;
        }

        // ---------- CANCEL ----------
        public static RegistrationResult Cancel(int userId, int registrationId, string reason)
        {
            int eventId;
            RegistrationResult result = CancelCore(userId, registrationId, reason, out eventId);
            // Notification is sent after the change is saved and never blocks it
            if (result.Success) NotificationService.RegistrationCancelled(userId, eventId);
            return result;
        }

        private static RegistrationResult CancelCore(int userId, int registrationId, string reason, out int eventId)
        {
            eventId = 0;
            if (userId <= 0 || registrationId <= 0)
                return Failure("InvalidRequest", "A valid registration is required.");

            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        if (!IsActiveStudent(connection, transaction, userId))
                            return Reject(transaction, "StudentRequired", "An active student account is required.");

                        string status;
                        if (!ReadRegistration(connection, transaction, userId, registrationId, out eventId, out status))
                            return Reject(transaction, "NotFound", "The registration could not be found.");
                        if (!string.Equals(status, "Registered", StringComparison.OrdinalIgnoreCase))
                            return Reject(transaction, "InvalidStatus", "This registration is already cancelled.");

                        EventInfo info = LockEvent(connection, transaction, eventId);
                        if (info == null)
                            return Reject(transaction, "EventNotFound", "The event does not exist.");
                        if (info.HasStarted)
                            return Reject(transaction, "EventStarted", "The event has already started, so the registration can no longer be cancelled.");

                        using (SqlCommand command = new SqlCommand(@"
UPDATE dbo.Registrations
SET Status = N'Cancelled', CancelledAt = SYSUTCDATETIME(),
    UpdatedAt = SYSUTCDATETIME(), IsAttended = 0
WHERE RegistrationId = @RegistrationId AND UserId = @UserId AND Status = N'Registered';", connection, transaction))
                        {
                            command.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = registrationId;
                            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                            if (command.ExecuteNonQuery() != 1)
                                return Reject(transaction, "Conflict", "The registration changed. Please try again.");
                        }

                        InsertLog(connection, transaction, registrationId, userId, "Cancelled",
                            "Registered", "Cancelled", eventId, eventId, reason);

                        transaction.Commit();
                        return new RegistrationResult(true, "Cancelled", "Your registration has been cancelled.", registrationId);
                    }
                    catch (SqlException exception)
                    {
                        RollbackSafely(transaction);
                        return HandleSqlException(exception);
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        // ---------- UPDATE (move registration to another event) ----------
        public static RegistrationResult Update(int userId, int registrationId, int newEventId, string reason)
        {
            RegistrationResult result = UpdateCore(userId, registrationId, newEventId, reason);
            if (result.Success) NotificationService.RegistrationUpdated(userId, newEventId);
            return result;
        }

        private static RegistrationResult UpdateCore(int userId, int registrationId, int newEventId, string reason)
        {
            if (userId <= 0 || registrationId <= 0 || newEventId <= 0)
                return Failure("InvalidRequest", "A valid registration and event are required.");

            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        if (!IsActiveStudent(connection, transaction, userId))
                            return Reject(transaction, "StudentRequired", "An active student account is required.");

                        int oldEventId;
                        string status;
                        if (!ReadRegistration(connection, transaction, userId, registrationId, out oldEventId, out status))
                            return Reject(transaction, "NotFound", "The registration could not be found.");
                        if (!string.Equals(status, "Registered", StringComparison.OrdinalIgnoreCase))
                            return Reject(transaction, "InvalidStatus", "Only an active registration can be changed.");
                        if (oldEventId == newEventId)
                            return Reject(transaction, "SameEvent", "Please choose a different event.");

                        // Lock both event rows in ascending EventId order to avoid deadlocks
                        int firstId = Math.Min(oldEventId, newEventId);
                        int secondId = Math.Max(oldEventId, newEventId);
                        EventInfo first = LockEvent(connection, transaction, firstId);
                        EventInfo second = LockEvent(connection, transaction, secondId);
                        EventInfo oldEvent = firstId == oldEventId ? first : second;
                        EventInfo newEvent = firstId == newEventId ? first : second;

                        if (newEvent == null)
                            return Reject(transaction, "EventNotFound", "The new event does not exist.");
                        if (oldEvent != null && oldEvent.HasStarted)
                            return Reject(transaction, "EventStarted", "Your current event has already started, so it can no longer be changed.");
                        if (!string.Equals(newEvent.Status, "Upcoming", StringComparison.OrdinalIgnoreCase) || newEvent.HasStarted)
                            return Reject(transaction, "EventClosed", "Registration is closed for the new event.");

                        // The unique index allows one record per (EventId, UserId)
                        using (SqlCommand command = new SqlCommand(@"
SELECT COUNT(*) FROM dbo.Registrations WHERE EventId = @EventId AND UserId = @UserId;", connection, transaction))
                        {
                            command.Parameters.Add("@EventId", SqlDbType.Int).Value = newEventId;
                            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                            if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                                return Reject(transaction, "AlreadyRegistered",
                                    "You already have a registration record for that event. If it was cancelled, register again from the event list.");
                        }

                        using (SqlCommand command = new SqlCommand(@"
SELECT COUNT_BIG(*) FROM dbo.Registrations WHERE EventId = @EventId AND Status = N'Registered';", connection, transaction))
                        {
                            command.Parameters.Add("@EventId", SqlDbType.Int).Value = newEventId;
                            if (Convert.ToInt64(command.ExecuteScalar()) >= newEvent.Capacity)
                                return Reject(transaction, "EventFull", "The new event is full.");
                        }

                        using (SqlCommand command = new SqlCommand(@"
UPDATE dbo.Registrations
SET EventId = @NewEventId, UpdatedAt = SYSUTCDATETIME(), IsAttended = 0
WHERE RegistrationId = @RegistrationId AND UserId = @UserId AND Status = N'Registered';", connection, transaction))
                        {
                            command.Parameters.Add("@NewEventId", SqlDbType.Int).Value = newEventId;
                            command.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = registrationId;
                            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                            if (command.ExecuteNonQuery() != 1)
                                return Reject(transaction, "Conflict", "The registration changed. Please try again.");
                        }

                        InsertLog(connection, transaction, registrationId, userId, "Updated",
                            "Registered", "Registered", oldEventId, newEventId, reason);

                        transaction.Commit();
                        return new RegistrationResult(true, "Updated", "Your registration has been moved to the new event.", registrationId);
                    }
                    catch (SqlException exception)
                    {
                        RollbackSafely(transaction);
                        return HandleSqlException(exception);
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        // ---------- QUERIES FOR THE PAGES ----------

        // Active registrations whose event has not started (times converted to Malaysia time)
        public static DataTable GetChangeableRegistrations(int userId)
        {
            return Fill(@"
SELECT r.RegistrationId, e.EventId, e.Title, e.Location,
       DATEADD(HOUR, 8, e.StartDateTime) AS StartMyt,
       DATEADD(HOUR, 8, e.EndDateTime) AS EndMyt
FROM dbo.Registrations r
JOIN dbo.Events e ON e.EventId = r.EventId
WHERE r.UserId = @UserId AND r.Status = N'Registered'
  AND e.StartDateTime > SYSUTCDATETIME()
ORDER BY e.StartDateTime;", userId);
        }

        // Upcoming events with free seats where the student has no registration record yet
        public static DataTable GetOpenEvents(int userId)
        {
            return Fill(@"
SELECT e.EventId, e.Title, DATEADD(HOUR, 8, e.StartDateTime) AS StartMyt,
       e.Capacity - (SELECT COUNT(*) FROM dbo.Registrations x
                     WHERE x.EventId = e.EventId AND x.Status = N'Registered') AS SeatsLeft
FROM dbo.Events e
WHERE e.Status = N'Upcoming' AND e.StartDateTime > SYSUTCDATETIME()
  AND NOT EXISTS (SELECT 1 FROM dbo.Registrations y WHERE y.EventId = e.EventId AND y.UserId = @UserId)
  AND e.Capacity > (SELECT COUNT(*) FROM dbo.Registrations z
                    WHERE z.EventId = e.EventId AND z.Status = N'Registered')
ORDER BY e.StartDateTime;", userId);
        }

        // Registration logs of the logged-in student, newest first
        public static DataTable GetLogs(int userId)
        {
            return Fill(@"
SELECT l.LogId, l.RegistrationId, l.Action, l.OldStatus, l.NewStatus,
       oe.Title AS OldEventTitle, ne.Title AS NewEventTitle,
       l.ChangeReason, DATEADD(HOUR, 8, l.ChangedAt) AS ChangedMyt
FROM dbo.RegistrationLogs l
LEFT JOIN dbo.Events oe ON oe.EventId = l.OldEventId
LEFT JOIN dbo.Events ne ON ne.EventId = l.NewEventId
WHERE l.UserId = @UserId
ORDER BY l.ChangedAt DESC, l.LogId DESC;", userId);
        }

        // ---------- HELPERS ----------
        private static DataTable Fill(string sql, int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException("userId");
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private static bool IsActiveStudent(SqlConnection connection, SqlTransaction transaction, int userId)
        {
            using (SqlCommand command = new SqlCommand(@"
SELECT COUNT(*) FROM dbo.Users WHERE UserId = @UserId AND IsActive = 1 AND Role = N'Student';", connection, transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        // The registration must belong to the logged-in user (userId comes from Session)
        private static bool ReadRegistration(SqlConnection connection, SqlTransaction transaction,
            int userId, int registrationId, out int eventId, out string status)
        {
            eventId = 0;
            status = null;
            using (SqlCommand command = new SqlCommand(@"
SELECT EventId, Status FROM dbo.Registrations
WHERE RegistrationId = @RegistrationId AND UserId = @UserId;", connection, transaction))
            {
                command.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = registrationId;
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return false;
                    eventId = reader.GetInt32(0);
                    status = reader.GetString(1);
                    return true;
                }
            }
        }

        private static EventInfo LockEvent(SqlConnection connection, SqlTransaction transaction, int eventId)
        {
            using (SqlCommand command = new SqlCommand(@"
SELECT Capacity, Status,
       CASE WHEN StartDateTime <= SYSUTCDATETIME() THEN 1 ELSE 0 END AS HasStarted
FROM dbo.Events WITH (UPDLOCK, HOLDLOCK)
WHERE EventId = @EventId;", connection, transaction))
            {
                command.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new EventInfo
                    {
                        Capacity = reader.GetInt32(0),
                        Status = reader.GetString(1),
                        HasStarted = reader.GetInt32(2) == 1
                    };
                }
            }
        }

        private static void InsertLog(SqlConnection connection, SqlTransaction transaction, int registrationId,
            int userId, string action, string oldStatus, string newStatus, int oldEventId, int newEventId, string reason)
        {
            using (SqlCommand command = new SqlCommand(@"
INSERT INTO dbo.RegistrationLogs
    (RegistrationId, UserId, Action, OldStatus, NewStatus, OldEventId, NewEventId, ChangeReason)
VALUES
    (@RegistrationId, @UserId, @Action, @OldStatus, @NewStatus, @OldEventId, @NewEventId, @ChangeReason);", connection, transaction))
            {
                command.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = registrationId;
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = action;
                command.Parameters.Add("@OldStatus", SqlDbType.NVarChar, 20).Value = oldStatus;
                command.Parameters.Add("@NewStatus", SqlDbType.NVarChar, 20).Value = newStatus;
                command.Parameters.Add("@OldEventId", SqlDbType.Int).Value = oldEventId;
                command.Parameters.Add("@NewEventId", SqlDbType.Int).Value = newEventId;
                command.Parameters.Add("@ChangeReason", SqlDbType.NVarChar, ReasonMaxLength).Value = CleanReason(reason);
                command.ExecuteNonQuery();
            }
        }

        private static object CleanReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return DBNull.Value;
            reason = reason.Trim();
            return reason.Length <= ReasonMaxLength ? reason : reason.Substring(0, ReasonMaxLength);
        }

        private static RegistrationResult HandleSqlException(SqlException exception)
        {
            if (exception.Number == 1205 || exception.Number == 1222 || exception.Number == -2)
                return Failure("Busy", "The system is busy. Check your registration history before trying again.");
            if (exception.Number == 2601 || exception.Number == 2627)
                return Failure("Conflict", "A matching record already exists. Check your registration history.");
            throw exception;
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