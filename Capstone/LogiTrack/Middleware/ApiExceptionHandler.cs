using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Middleware;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            ArgumentException => (400, "Invalid request", exception.Message),
            InvalidOperationException => (409, "Request conflict", exception.Message),
            DbUpdateException => (409, "Resource conflict", "The requested change conflicts with existing data."),
            _ => (0, "", "")
        };
        if (status == 0)
            return false;
        logger.LogWarning(exception, "Handled API exception with status {StatusCode}", status);
        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status, Title = title, Detail = detail, Instance = httpContext.Request.Path
        }, cancellationToken);
        return true;
    }
}