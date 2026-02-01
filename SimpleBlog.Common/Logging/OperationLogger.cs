using System.Diagnostics;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SimpleBlog.Common.Logging;

/// <summary>
/// Implementation of IOperationLogger providing structured logging with performance tracking.
/// </summary>
public sealed class OperationLogger : IOperationLogger
{
    private readonly ILogger<OperationLogger> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public OperationLogger(
        ILogger<OperationLogger> logger,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T> LogRepositoryOperationAsync<T>(
        string operationType,
        string entityType,
        Func<Task<T>> operation,
        object? additionalContext = null)
    {
        var operationId = Guid.NewGuid();
        var userName = GetCurrentUserName();
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "[{OperationId}] Starting {OperationType} on {EntityType}. User: {User}, Context: {@Context}",
            operationId,
            operationType,
            entityType,
            userName,
            additionalContext);

        try
        {
            var result = await operation();
            stopwatch.Stop();

            _logger.LogInformation(
                "[{OperationId}] {OperationType} on {EntityType} completed successfully in {ElapsedMs}ms. User: {User}",
                operationId,
                operationType,
                entityType,
                stopwatch.ElapsedMilliseconds,
                userName);

            return result;
        }
        catch (DbUpdateException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[{OperationId}] Database error during {OperationType} on {EntityType} after {ElapsedMs}ms. User: {User}",
                operationId,
                operationType,
                entityType,
                stopwatch.ElapsedMilliseconds,
                userName);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[{OperationId}] Unexpected error during {OperationType} on {EntityType} after {ElapsedMs}ms. User: {User}",
                operationId,
                operationType,
                entityType,
                stopwatch.ElapsedMilliseconds,
                userName);
            throw;
        }
    }

    public async Task LogRepositoryOperationAsync(
        string operationType,
        string entityType,
        Func<Task> operation,
        object? additionalContext = null)
    {
        await LogRepositoryOperationAsync<object?>(
            operationType,
            entityType,
            async () =>
            {
                await operation();
                return null;
            },
            additionalContext);
    }

    public void LogValidationFailure(
        string operationType,
        object request,
        IEnumerable<ValidationFailure> errors)
    {
        var userName = GetCurrentUserName();

        _logger.LogWarning(
            "Validation failed for {OperationType}. User: {User}, Request: {RequestType}, ErrorCount: {ErrorCount}, Errors: {@ValidationErrors}",
            operationType,
            userName,
            request.GetType().Name,
            errors.Count(),
            errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
    }

    public void LogSpecificationUsage(
        string specificationType,
        string entityType,
        object? parameters = null)
    {
        _logger.LogDebug(
            "Applying specification {SpecificationType} to {EntityType}. Parameters: {@Parameters}",
            specificationType,
            entityType,
            parameters);
    }

    public async Task<T> LogQueryPerformanceAsync<T>(
        string queryDescription,
        Func<Task<T>> query,
        object? parameters = null)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug(
            "Executing query: {QueryDescription}. Parameters: {@Parameters}",
            queryDescription,
            parameters);

        try
        {
            var result = await query();
            stopwatch.Stop();

            _logger.LogInformation(
                "Query completed: {QueryDescription} in {ElapsedMs}ms",
                queryDescription,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Query failed: {QueryDescription} after {ElapsedMs}ms",
                queryDescription,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private string GetCurrentUserName()
    {
        var name = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
        return MaskUserName(name);
    }

    private static string MaskUserName(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "unknown";
        }

        if (username.Equals("System", StringComparison.OrdinalIgnoreCase))
        {
            return "System";
        }

        return username.Length <= 2
            ? $"{username[0]}*"
            : $"{username[..1]}***";
    }
}
