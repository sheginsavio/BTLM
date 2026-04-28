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
                return false;
            }
        }
        public static bool VerifySecurityAnswer(string inputAnswer, string storedAnswer)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(inputAnswer, storedAnswer);
            }
            catch
            {
                return false;
            }
        }
    }
}
