using System;
using System.Security.Cryptography;

namespace PRG3204_Project_Group4.Helpers
{
    public static class PasswordHelper
    {
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        // Stored format: PBKDF2$iterations$salt$hash
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            byte[] hash = Derive(password, salt, Iterations, HashSize);
            return "PBKDF2$" + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            string[] parts = storedHash.Split('$');
            int iterations;
            if (parts.Length != 4 || parts[0] != "PBKDF2" || !int.TryParse(parts[1], out iterations) || iterations <= 0)
            {
                return false;
            }

            byte[] salt;
            byte[] expected;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }
            if (salt.Length == 0 || expected.Length == 0) return false;

            byte[] actual = Derive(password, salt, iterations, expected.Length);

            // Compare every byte so the time taken does not reveal the first difference
            int difference = 0;
            for (int i = 0; i < expected.Length; i++)
            {
                difference |= actual[i] ^ expected[i];
            }
            return difference == 0;
        }

        private static byte[] Derive(string password, byte[] salt, int iterations, int size)
        {
            using (Rfc2898DeriveBytes derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return derive.GetBytes(size);
            }
        }
    }
}
