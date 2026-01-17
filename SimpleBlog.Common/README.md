# SimpleBlog.Common

## Overview

Shared library containing common models, interfaces, exceptions, validators, and utilities used across all SimpleBlog projects.

## Technologies

- **.NET 9.0** - Framework
- **FluentValidation** - Validation library
- **Ardalis.Specification** - Specification pattern

## Project Structure

```
SimpleBlog.Common/
├── Exceptions/               # Custom exceptions
│   ├── NotFoundException.cs
│   ├── ValidationException.cs
│   └── UnauthorizedException.cs
├── Extensions/               # Extension methods
│   ├── StringExtensions.cs
│   └── EnumerableExtensions.cs
├── Interfaces/               # Shared interfaces
│   ├── IRepository.cs
│   ├── IReadRepository.cs
│   └── ISpecification.cs
├── Logging/                  # Logging utilities
│   └── OperationLogger.cs
├── Models/                   # Shared DTOs
│   ├── PagedResult.cs
│   ├── ErrorResponse.cs
│   └── ApiResponse.cs
├── Specifications/           # Base specifications
│   └── BaseSpecification.cs
└── Validators/               # Shared validators
    └── ValidationExtensions.cs
```

## Key Components

### Custom Exceptions

```csharp
// Not found exception
public class NotFoundException : Exception
{
    public NotFoundException(string entity, object id)
        : base($"{entity} with id {id} not found")
    { }
}

// Validation exception
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }
    
    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}

// Unauthorized exception
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Unauthorized")
        : base(message)
    { }
}
```

### Repository Interfaces

```csharp
public interface IReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<T>> ListAsync(CancellationToken ct = default);
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default);
}

public interface IRepository<T> : IReadRepository<T> where T : class
{
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

### Specification Pattern

```csharp
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification(Expression<Func<T, bool>>? criteria)
    {
        Criteria = criteria;
    }
    
    public Expression<Func<T, bool>>? Criteria { get; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
}
```

### Extension Methods

```csharp
// String extensions
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value) 
        => string.IsNullOrEmpty(value);
    
    public static bool IsNullOrWhiteSpace(this string? value) 
        => string.IsNullOrWhiteSpace(value);
    
    public static string TruncateWithElipsis(this string value, int maxLength)
    {
        if (value.Length <= maxLength) return value;
        return value.Substring(0, maxLength - 3) + "...";
    }
}

// Enumerable extensions
public static class EnumerableExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
        => collection == null || !collection.Any();
    
    public static PagedResult<T> ToPagedResult<T>(
        this IEnumerable<T> source,
        int page,
        int pageSize)
    {
        var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = source.Count();
        
        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
```

### Response Models

```csharp
// Paged result
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

// API response wrapper
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}

// Error response
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
    public string? StackTrace { get; set; }
}
```

### Operation Logger

```csharp
public class OperationLogger : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operation;
    private readonly Stopwatch _stopwatch;
    
    public OperationLogger(ILogger logger, string operation)
    {
        _logger = logger;
        _operation = operation;
        _stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting {Operation}", operation);
    }
    
    public void Dispose()
    {
        _stopwatch.Stop();
        _logger.LogInformation(
            "Completed {Operation} in {ElapsedMs}ms",
            _operation,
            _stopwatch.ElapsedMilliseconds);
    }
}

// Usage
using (new OperationLogger(logger, "GetAllPosts"))
{
    return await repository.GetAllPostsAsync();
}
```

## Usage Examples

### Repository Pattern

```csharp
public class PostService
{
    private readonly IRepository<Post> _repository;
    
    public PostService(IRepository<Post> repository)
    {
        _repository = repository;
    }
    
    public async Task<Post> GetPostAsync(int id)
    {
        var post = await _repository.GetByIdAsync(id);
        if (post is null)
        {
            throw new NotFoundException(nameof(Post), id);
        }
        return post;
    }
}
```

### Specification Pattern

```csharp
public class ActivePostsSpec : BaseSpecification<Post>
{
    public ActivePostsSpec() 
        : base(p => p.PublishedAt <= DateTime.UtcNow)
    {
        AddInclude(p => p.Comments);
        AddOrderBy(p => p.PublishedAt);
    }
}

// Usage
var posts = await repository.ListAsync(new ActivePostsSpec());
```

### Validation

```csharp
public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title too long");
            
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MinimumLength(10).WithMessage("Content too short");
    }
}
```

## Dependencies

- `Ardalis.Specification` - Specification pattern
- `FluentValidation` - Validation framework
- `Microsoft.Extensions.Logging.Abstractions` - Logging

## Testing Support

```csharp
// No-op logger for tests
public class NoOpOperationLogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => null!;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    { }
}
```

## Best Practices

1. **Keep It Simple** - Only add truly shared code here
2. **No Dependencies** - Minimize external dependencies
3. **Immutable Models** - Use records for DTOs
4. **Explicit Nullability** - Use nullable reference types
5. **Extension Methods** - Prefer extension methods over static utilities
6. **Async All The Way** - All I/O operations should be async

## Related Documentation

- [Project Structure](../docs/development/project-structure.md) - Architecture
- [Coding Standards](../.github/copilot-instructions.md) - Code style

## Contributing

When adding to this library:
1. Ensure code is truly shared across multiple projects
2. Add XML documentation comments
3. Include unit tests
4. Follow existing patterns
5. Keep dependencies minimal

## Version History

- **1.0.0** - Initial release with base abstractions
- **1.1.0** - Added operation logger and paged results
- **1.2.0** - Extended validation support
