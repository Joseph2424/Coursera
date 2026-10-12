using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Middleware;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
        httpContext.Response.ContentType = "application/problem+json; charset=utf-8";
        await JsonSerializer.SerializeAsync(httpContext.Response.Body, new ProblemDetails
        {
            Status = status, Title = title, Detail = detail, Instance = httpContext.Request.Path
        }, JsonOptions, cancellationToken);
        return true;
    }
}