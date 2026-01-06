using FluentValidation.Results;

namespace SimpleBlog.Common.Logging;

/// <summary>
/// Service for logging repository operations with automatic performance tracking and structured logging.
/// </summary>
public interface IOperationLogger
{
    /// <summary>
    /// Logs a repository operation with automatic performance tracking.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operationType">Type of operation: Create, Read, Update, Delete, Query.</param>
    /// <param name="entityType">Type of entity being operated on: Post, Product, Order.</param>
    /// <param name="operation">The async operation to execute and log.</param>
    /// <param name="additionalContext">Optional additional context to include in logs.</param>
    /// <returns>The result of the operation.</returns>
    Task<T> LogRepositoryOperationAsync<T>(
        string operationType,
        string entityType,
        Func<Task<T>> operation,
        object? additionalContext = null);

    /// <summary>
    /// Logs a repository operation that doesn't return a value.
    /// </summary>
    Task LogRepositoryOperationAsync(
        string operationType,
        string entityType,
        Func<Task> operation,
        object? additionalContext = null);

    /// <summary>
    /// Logs validation failures with detailed error information.
    /// </summary>
    void LogValidationFailure(
        string operationType,
        object request,
        IEnumerable<ValidationFailure> errors);

    /// <summary>
    /// Logs specification usage.
    /// </summary>
    void LogSpecificationUsage(
        string specificationType,
        string entityType,
        object? parameters = null);

    /// <summary>
    /// Logs database query performance.
    /// </summary>
    Task<T> LogQueryPerformanceAsync<T>(
        string queryDescription,
        Func<Task<T>> query,
        object? parameters = null);
}
