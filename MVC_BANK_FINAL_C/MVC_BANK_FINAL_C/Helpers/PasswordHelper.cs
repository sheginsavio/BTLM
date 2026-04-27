using BCrypt.Net;

namespace MVC_BANK_FINAL_C.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
            }
            catch
            {
                // Handles legacy SHA256 hashes gracefully — returns false
                // so the user is prompted to reset their password
                return false;
            }
        }
    }
}
