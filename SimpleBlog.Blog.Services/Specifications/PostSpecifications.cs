using Microsoft.EntityFrameworkCore;
using SimpleBlog.Common.Specifications;

namespace SimpleBlog.Blog.Services.Specifications;

/// <summary>
/// Specification for loading posts with their comments included.
/// </summary>
public sealed class PostsWithCommentsSpecification : Specification<PostEntity>
{
    public override IQueryable<PostEntity> Apply(IQueryable<PostEntity> query)
    {
        return query
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt);
    }
}

/// <summary>
/// Specification for filtering posts by author.
/// </summary>
public sealed class PostsByAuthorSpecification(string author) : Specification<PostEntity>
{
    public override IQueryable<PostEntity> Apply(IQueryable<PostEntity> query)
    {
        return query
            .Where(p => p.Author == author)
            .OrderByDescending(p => p.CreatedAt);
    }
}

/// <summary>
/// Specification for filtering posts created after a specific date.
/// </summary>
public sealed class PostsCreatedAfterSpecification(DateTimeOffset date) : Specification<PostEntity>
{
    public override IQueryable<PostEntity> Apply(IQueryable<PostEntity> query)
    {
        return query
            .Where(p => p.CreatedAt >= date)
            .OrderByDescending(p => p.CreatedAt);
    }
}

/// <summary>
/// Specification for searching posts by title or content.
/// </summary>
public sealed class PostsSearchSpecification(string searchTerm) : Specification<PostEntity>
{
    public override IQueryable<PostEntity> Apply(IQueryable<PostEntity> query)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return query
            .Where(p => p.Title.ToLower().Contains(lowerSearchTerm) || 
                       p.Content.ToLower().Contains(lowerSearchTerm))
            .OrderByDescending(p => p.CreatedAt);
    }
}

/// <summary>
/// Specification for ordering posts by creation date (descending by default).
/// </summary>
public sealed class PostsOrderedByDateSpecification(bool ascending = false) : Specification<PostEntity>
{
    public override IQueryable<PostEntity> Apply(IQueryable<PostEntity> query)
    {
        return ascending 
            ? query.OrderBy(p => p.CreatedAt)
            : query.OrderByDescending(p => p.CreatedAt);
    }
}
