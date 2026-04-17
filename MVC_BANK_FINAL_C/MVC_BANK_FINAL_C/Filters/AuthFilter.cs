using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVC_BANK_FINAL_C.Filters
{
    public class AuthFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Allow actions/controllers decorated with [AllowAnonymous]
            bool hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
                .OfType<AllowAnonymousAttribute>()
                .Any();

            if (hasAllowAnonymous) return;

            // Allow the ErrorController (404 page) without requiring a session
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            if (controllerName == "Error") return;

            // Check session for a logged-in user
            var userId = context.HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
