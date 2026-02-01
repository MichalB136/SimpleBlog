using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SimpleBlog.ApiService.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        // Skip health checks to reduce noise
        var path = context.Request.Path.HasValue ? context.Request.Path.Value! : string.Empty;
        if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var correlationId = GetOrCreateCorrelationId(context);
        var user = MaskUserName(context.User?.Identity?.Name);
        var method = context.Request.Method;
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId
        });

        _logger.LogInformation(
            "HTTP {Method} {Path} started by {User}",
            method,
            path,
            user);

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
            sw.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                method,
                path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "HTTP {Method} {Path} failed after {ElapsedMs}ms",
                method,
                path,
                sw.ElapsedMilliseconds);
            throw;
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        const string headerName = "X-Correlation-ID";

        if (context.Request.Headers.TryGetValue(headerName, out var existing))
        {
            var existingValue = existing.ToString();
            if (!string.IsNullOrWhiteSpace(existingValue))
            {
                context.Response.Headers[headerName] = existingValue;
                return existingValue;
            }
        }

        var id = context.TraceIdentifier;
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");

        context.Response.Headers[headerName] = id;
        return id;
    }

    private static string MaskUserName(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "Anonymous";
        }

        return username.Length <= 2
            ? $"{username[0]}*"
            : $"{username[..1]}***";
    }
}
