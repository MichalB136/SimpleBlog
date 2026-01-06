using FluentValidation.Results;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.Tests;

public sealed class NoOpOperationLogger : IOperationLogger
{
    public Task<T> LogRepositoryOperationAsync<T>(string operationType, string entityType, Func<Task<T>> operation, object? additionalContext = null)
        => operation();

    public Task LogRepositoryOperationAsync(string operationType, string entityType, Func<Task> operation, object? additionalContext = null)
        => operation();

    public void LogValidationFailure(string operationType, object request, IEnumerable<ValidationFailure> errors)
    {
        // no-op for tests
    }

    public void LogSpecificationUsage(string specificationType, string entityType, object? parameters = null)
    {
        // no-op for tests
    }

    public Task<T> LogQueryPerformanceAsync<T>(string queryDescription, Func<Task<T>> query, object? parameters = null)
        => query();
}
