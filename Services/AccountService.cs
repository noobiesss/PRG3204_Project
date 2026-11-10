using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using PRG3204_Project_Group4.Helpers;

namespace PRG3204_Project_Group4.Services
{
    public sealed class UserAccount
    {
        public int UserId { get; set; }
        public string MatricOrStaffNo { get; set; }
        public string FullName { get; set; }
        public string SchoolEmail { get; set; }
        public string PersonalEmail { get; set; }
        public string Role { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Faculty { get; set; }
    }

    public sealed class AccountResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public UserAccount User { get; private set; }

        internal AccountResult(bool success, string message, UserAccount user)
        {
            Success = success;
            Message = message;
            User = user;
        }
    }

    public static class AccountService
    {
        private const string InvalidLogin = "Invalid email or password.";

        public static AccountResult Login(string email, string password)
        {
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"
SELECT UserId, FullName, PasswordHash, Role, IsActive
FROM dbo.Users
WHERE SchoolEmail = @SchoolEmail;", connection))
            {
                command.Parameters.Add("@SchoolEmail", SqlDbType.NVarChar, 100).Value = NormalizeEmail(email);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read() || !PasswordHelper.VerifyPassword(password, reader.GetString(2)))
                    {
                        return Failure(InvalidLogin);
                    }
                    if (!reader.GetBoolean(4))
                    {
                        return Failure("This account has been deactivated.");
                    }

                    return new AccountResult(true, "Login successful.", new UserAccount
                    {
                        UserId = reader.GetInt32(0),
                        FullName = reader.GetString(1),
                        Role = reader.GetString(3)
                    });
                }
            }
        }

        // Public registration can only create Student accounts
        public static AccountResult RegisterStudent(UserAccount account, string password)
        {
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"
INSERT INTO dbo.Users
    (MatricOrStaffNo, FullName, SchoolEmail, PersonalEmail, PasswordHash, Role, Gender, DateOfBirth, PhoneNumber, Faculty)
VALUES
    (@MatricOrStaffNo, @FullName, @SchoolEmail, @PersonalEmail, @PasswordHash, N'Student', @Gender, @DateOfBirth, @PhoneNumber, @Faculty);", connection))
            {
                command.Parameters.Add("@MatricOrStaffNo", SqlDbType.NVarChar, 20).Value = account.MatricOrStaffNo.Trim().ToUpperInvariant();
                command.Parameters.Add("@SchoolEmail", SqlDbType.NVarChar, 100).Value = NormalizeEmail(account.SchoolEmail);
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = PasswordHelper.HashPassword(password);
                AddProfileParameters(command, account);
                connection.Open();
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException exception)
                {
                    if (exception.Number == 2601 || exception.Number == 2627)
                    {
                        return Failure(exception.Message.Contains("IX_Users_SchoolEmail")
                            ? "This school email is already registered."
                            : "This matric number is already registered.");
                    }
                    throw;
                }
            }
            return new AccountResult(true, "Registration successful.", null);
        }

        public static UserAccount GetUser(int userId)
        {
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"
SELECT UserId, MatricOrStaffNo, FullName, SchoolEmail, PersonalEmail, Role, Gender, DateOfBirth, PhoneNumber, Faculty
FROM dbo.Users
WHERE UserId = @UserId AND IsActive = 1;", connection))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;

                    return new UserAccount
                    {
                        UserId = reader.GetInt32(0),
                        MatricOrStaffNo = reader.GetString(1),
                        FullName = reader.GetString(2),
                        SchoolEmail = reader.GetString(3),
                        PersonalEmail = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Role = reader.GetString(5),
                        Gender = reader.IsDBNull(6) ? null : reader.GetString(6),
                        DateOfBirth = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                        PhoneNumber = reader.IsDBNull(8) ? null : reader.GetString(8),
                        Faculty = reader.IsDBNull(9) ? null : reader.GetString(9)
                    };
                }
            }
        }

        public static AccountResult UpdateProfile(UserAccount account)
        {
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(@"
UPDATE dbo.Users
SET FullName = @FullName, PersonalEmail = @PersonalEmail, Gender = @Gender,
    DateOfBirth = @DateOfBirth, PhoneNumber = @PhoneNumber, Faculty = @Faculty
WHERE UserId = @UserId AND IsActive = 1;", connection))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = account.UserId;
                AddProfileParameters(command, account);
                connection.Open();
                return command.ExecuteNonQuery() == 1
                    ? new AccountResult(true, "Profile updated.", null)
                    : Failure("Your account could not be found.");
            }
        }

        public static AccountResult ChangePassword(int userId, string currentPassword, string newPassword)
        {
            using (SqlConnection connection = DatabaseHelper.CreateConnection())
            {
                connection.Open();

                string storedHash;
                using (SqlCommand select = new SqlCommand("SELECT PasswordHash FROM dbo.Users WHERE UserId = @UserId AND IsActive = 1;", connection))
                {
                    select.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    storedHash = select.ExecuteScalar() as string;
                }
                if (!PasswordHelper.VerifyPassword(currentPassword, storedHash))
                {
                    return Failure("Current password is incorrect.");
                }

                using (SqlCommand update = new SqlCommand("UPDATE dbo.Users SET PasswordHash = @PasswordHash WHERE UserId = @UserId AND IsActive = 1;", connection))
                {
                    update.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    update.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = PasswordHelper.HashPassword(newPassword);
                    return update.ExecuteNonQuery() == 1
                        ? new AccountResult(true, "Password changed.", null)
                        : Failure("Your account could not be found.");
                }
            }
        }

        // An empty value is allowed because date of birth is optional
        public static bool TryParseDateOfBirth(string value, out DateTime? dateOfBirth)
        {
            dateOfBirth = null;
            if (string.IsNullOrWhiteSpace(value)) return true;

            DateTime parsed;
            if (!DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return false;
            }

            DateTime todayInMalaysia = DateTime.UtcNow.AddHours(8).Date;
            if (parsed.Year < 1900 || parsed >= todayInMalaysia) return false;

            dateOfBirth = parsed;
            return true;
        }

        private static void AddProfileParameters(SqlCommand command, UserAccount account)
        {
            command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = account.FullName.Trim();
            command.Parameters.Add("@PersonalEmail", SqlDbType.NVarChar, 100).Value = OptionalText(account.PersonalEmail);
            command.Parameters.Add("@Gender", SqlDbType.NVarChar, 10).Value = OptionalText(account.Gender);
            command.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value = account.DateOfBirth.HasValue ? (object)account.DateOfBirth.Value : DBNull.Value;
            command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = OptionalText(account.PhoneNumber);
            command.Parameters.Add("@Faculty", SqlDbType.NVarChar, 100).Value = OptionalText(account.Faculty);
        }

        private static object OptionalText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private static AccountResult Failure(string message)
        {
            return new AccountResult(false, message, null);
        }
    }
}
