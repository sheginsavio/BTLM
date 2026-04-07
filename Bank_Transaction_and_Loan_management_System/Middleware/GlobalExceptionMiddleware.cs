using System.Text.Json;

namespace Bank_Transaction_and_Loan_management_System.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempted");
                context.Response.StatusCode = 403;
                await WriteErrorResponse(context, "Access denied.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument provided");
                context.Response.StatusCode = 400;
                await WriteErrorResponse(context, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation performed");
                context.Response.StatusCode = 400;
                await WriteErrorResponse(context, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                context.Response.StatusCode = 500;
                await WriteErrorResponse(context, "An unexpected error occurred.");
            }
        }

        private static Task WriteErrorResponse(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            var response = new { message };
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
