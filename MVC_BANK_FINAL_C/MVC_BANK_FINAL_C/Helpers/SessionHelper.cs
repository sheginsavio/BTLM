namespace MVC_BANK_FINAL_C.Helpers
{
    public static class SessionHelper
    {
        public static int GetUserId(IHttpContextAccessor accessor)
        {
            var val = accessor.HttpContext?.Session.GetString("UserId");
            return int.TryParse(val, out int id) ? id : 0;
        }

        public static string GetUsername(IHttpContextAccessor accessor)
            => accessor.HttpContext?.Session.GetString("Username") ?? string.Empty;

        public static string GetUserRole(IHttpContextAccessor accessor)
            => accessor.HttpContext?.Session.GetString("UserRole") ?? string.Empty;

        public static int? GetCustomerId(IHttpContextAccessor accessor)
        {
            var val = accessor.HttpContext?.Session.GetString("CustomerId");
            return int.TryParse(val, out int id) ? id : null;
        }

        public static bool IsLoggedIn(IHttpContextAccessor accessor)
            => !string.IsNullOrEmpty(accessor.HttpContext?.Session.GetString("UserId"));

        public static bool IsInRole(IHttpContextAccessor accessor, string role)
            => GetUserRole(accessor).Equals(role, StringComparison.OrdinalIgnoreCase);
    }
}
