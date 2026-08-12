using System.Net;

namespace UserManagementAPI.Middleware
{
    public class TokenValidationMiddleware(
        RequestDelegate next,
        ILogger<TokenValidationMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<TokenValidationMiddleware> _logger = logger;

        public async Task Invoke(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authHeader) ||
                !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Missing or malformed Authorization header");
                await Reject(context, "Missing or malformed Authorization header");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            if (!ValidateToken(token))
            {
                _logger.LogWarning("Invalid token: {Token}", token);
                await Reject(context, "Invalid token");
                return;
            }

            await _next(context);
        }

        private bool ValidateToken(string token)
        {
            // TODO: Replace with real validation logic
            // Example: check against a list, validate JWT, call auth service, etc.

            return token == "my-secret-token"; // placeholder
        }

        private async Task Reject(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync($"{{\"error\":\"{message}\"}}");
        }
    }

    public static class TokenValidationExtensions
    {
        public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TokenValidationMiddleware>();
        }
    }
}
