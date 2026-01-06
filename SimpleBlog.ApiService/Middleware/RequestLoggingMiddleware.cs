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
        // Skip health checks to reduce noise
        var path = context.Request.Path.HasValue ? context.Request.Path.Value! : string.Empty;
        if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var correlationId = GetOrCreateCorrelationId(context);
        var user = context.User?.Identity?.Name ?? "Anonymous";
        var method = context.Request.Method;
        var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;

        _logger.LogInformation(
            "HTTP {Method} {Path}{Query} started by {User}. CorrelationId: {CorrelationId}",
            method,
            path,
            query,
            user,
            correlationId);

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
            sw.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path}{Query} responded {StatusCode} in {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                method,
                path,
                query,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "HTTP {Method} {Path}{Query} failed after {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                method,
                path,
                query,
                sw.ElapsedMilliseconds,
                correlationId);
            throw;
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        const string headerName = "X-Correlation-ID";

        if (context.Request.Headers.TryGetValue(headerName, out var existing) && !string.IsNullOrWhiteSpace(existing))
        {
            context.Response.Headers[headerName] = existing.ToString();
            return existing!;
        }

        var id = context.TraceIdentifier;
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");

        context.Response.Headers[headerName] = id;
        return id;
    }
}
