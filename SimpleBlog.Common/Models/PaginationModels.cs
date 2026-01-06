namespace SimpleBlog.Common.Models;

/// <summary>
/// Pagination request parameters
/// </summary>
public record PaginationParams
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public int Skip => (Page - 1) * PageSize;

    public PaginationParams Validate()
    {
        if (Page < 1)
            throw new ArgumentException("Page must be greater than 0", nameof(Page));
        if (PageSize < 1 || PageSize > 100)
            throw new ArgumentException("PageSize must be between 1 and 100", nameof(PageSize));
        return this;
    }
}

/// <summary>
/// Paginated result wrapper
/// </summary>
public record PaginatedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Total { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }

    public int TotalPages => (Total + PageSize - 1) / PageSize;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
