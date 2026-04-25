using System.Security.Cryptography;
using System.Text;

namespace MVC_BANK_FINAL_C.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash  = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }

        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }
    }
}
