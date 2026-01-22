using SimpleBlog.Common;
using SimpleBlog.Common.Extensions;
using SimpleBlog.Common.Specifications;
using SimpleBlog.Common.Logging;
using SimpleBlog.Blog.Services.Specifications;
using Microsoft.EntityFrameworkCore;

namespace SimpleBlog.Blog.Services;

public sealed class EfPostRepository(
    BlogDbContext context,
    IOperationLogger operationLogger) : IPostRepository
{
    public async Task<PaginatedResult<Post>> GetAllAsync(PostFilterRequest? filter = null, int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetAllPosts",
            async () =>
            {
                var query = context.Posts
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .AsQueryable();

                // Apply filters
                if (filter is not null)
                {
                    // Filter by tags - if any of the requested tags are in the post's tags
                    if (filter.TagIds is not null && filter.TagIds.Count > 0)
                    {
                        query = query.Where(p => 
                            p.PostTags.Any(pt => filter.TagIds.Contains(pt.TagId)));
                    }

                    // Filter by search term (title or content)
                    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    {
                        var searchTerm = filter.SearchTerm.ToLower();
                        query = query.Where(p => 
                            p.Title.ToLower().Contains(searchTerm) || 
                            p.Content.ToLower().Contains(searchTerm));
                    }
                }

                var total = await query.CountAsync();
                var entities = await query
                    .OrderByDescending(p => p.IsPinned)
                    .ThenByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Post>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new { filter, page, pageSize });
    }

    public async Task<Post?> GetByIdAsync(Guid id)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetPostById",
            async () =>
            {
                var entity = await context.Posts
                    .Include(p => p.Comments)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstOrDefaultAsync(p => p.Id == id);
                return entity is not null ? MapToModel(entity) : null;
            },
            new { PostId = id });
    }

    public async Task<IReadOnlyList<Post>> GetByTagAsync(Guid tagId)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetPostsByTag",
            async () =>
            {
                var entities = await context.Posts
                    .Include(p => p.Comments)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.PostTags.Any(pt => pt.TagId == tagId))
                    .OrderByDescending(p => p.IsPinned)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToListAsync();
                
                return entities.Select(MapToModel).ToList();
            },
            new { TagId = tagId });
    }

    public async Task<Post> CreateAsync(CreatePostRequest request)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Create",
            "Post",
            async () =>
            {
                var entity = new PostEntity
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Content = request.Content,
                    Author = request.Author ?? "Anon",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ImageUrls = "[]" // Start with empty array
                };

                context.Posts.Add(entity);
                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new { request.Title, request.Author });
    }

    public async Task<Post?> UpdateAsync(Guid id, UpdatePostRequest request)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Update",
            "Post",
            async () =>
            {
                var entity = await context.Posts.FirstOrDefaultAsync(p => p.Id == id);
                if (entity is null)
                    return null;

                if (request.Title is not null)
                    entity.Title = request.Title;
                if (request.Content is not null)
                    entity.Content = request.Content;
                if (request.Author is not null)
                    entity.Author = request.Author;

                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new { PostId = id, HasTitle = !string.IsNullOrEmpty(request.Title), HasContent = !string.IsNullOrEmpty(request.Content) });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Delete",
            "Post",
            async () =>
            {
                var entity = await context.Posts.FirstOrDefaultAsync(p => p.Id == id);
                if (entity is null)
                    return false;

                context.Posts.Remove(entity);
                await context.SaveChangesAsync();
                return true;
            },
            new { PostId = id });
    }

    public async Task<IReadOnlyList<Comment>?> GetCommentsAsync(Guid postId)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetPostComments",
            async () =>
            {
                var post = await context.Posts
                    .Include(p => p.Comments)
                    .FirstOrDefaultAsync(p => p.Id == postId);

                var orderedComments = post?.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(MapCommentToModel)
                    .ToList();

                return orderedComments ?? new List<Comment>();
            },
            new { PostId = postId });
    }

    public async Task<Comment?> AddCommentAsync(Guid postId, CreateCommentRequest request)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "AddComment",
            "Comment",
            async () =>
            {
                var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
                if (post is null)
                    return null;

                var comment = new CommentEntity
                {
                    Id = Guid.NewGuid(),
                    PostId = postId,
                    Author = request.Author ?? "Anon",
                    Content = request.Content!,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                context.Comments.Add(comment);
                await context.SaveChangesAsync();
                return MapCommentToModel(comment);
            },
            new { PostId = postId, request.Author });
    }

    public async Task<Post?> SetPinnedAsync(Guid id, bool isPinned)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "SetPinned",
            "Post",
            async () =>
            {
                var entity = await context.Posts.FirstOrDefaultAsync(p => p.Id == id);
                if (entity is null)
                    return null;

                entity.IsPinned = isPinned;
                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new { PostId = id, IsPinned = isPinned });
    }

    /// <summary>
    /// Gets all posts with comments included using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Post>> GetAllWithCommentsAsync(int page = 1, int pageSize = 10)
    {
        var spec = new PostsWithCommentsSpecification();
        var total = await context.Posts.CountAsync();
        
        var entities = await context.Posts
            .ApplySpecification(spec)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Post>
        {
            Items = entities.Select(MapToModel).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets posts by author using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Post>> GetByAuthorAsync(string author, int page = 1, int pageSize = 10)
    {
        var spec = new PostsByAuthorSpecification(author);
        var total = await context.Posts.ApplySpecification(spec).CountAsync();
        
        var entities = await context.Posts
            .ApplySpecification(spec)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Post>
        {
            Items = entities.Select(MapToModel).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Searches posts by title or content using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Post>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        var spec = new PostsSearchSpecification(searchTerm);
        var total = await context.Posts.ApplySpecification(spec).CountAsync();
        
        var entities = await context.Posts
            .ApplySpecification(spec)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Post>
        {
            Items = entities.Select(MapToModel).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets posts created after a specific date using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Post>> GetCreatedAfterAsync(DateTimeOffset date, int page = 1, int pageSize = 10)
    {
        var spec = new PostsCreatedAfterSpecification(date);
        var total = await context.Posts.ApplySpecification(spec).CountAsync();
        
        var entities = await context.Posts
            .ApplySpecification(spec)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Post>
        {
            Items = entities.Select(MapToModel).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private static Post MapToModel(PostEntity entity)
    {
        var orderedComments = entity.Comments
            .OrderByDescending(c => c.CreatedAt)
            .Select(MapCommentToModel)
            .ToList();

        // Deserialize ImageUrls JSON array (handle empty string as empty array)
        var imageUrls = string.IsNullOrWhiteSpace(entity.ImageUrls)
            ? []
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.ImageUrls) ?? [];

        // Map tags
        var tags = entity.PostTags
            .Select(pt => new Tag(
                pt.Tag.Id,
                pt.Tag.Name,
                pt.Tag.Slug,
                pt.Tag.Color,
                pt.Tag.CreatedAt))
            .ToList();

        return new Post(
            entity.Id,
            entity.Title,
            entity.Content,
            entity.Author,
            entity.CreatedAt,
            orderedComments,
            imageUrls,
            entity.IsPinned,
            tags
        );
    }

    public async Task<Post?> AddImageAsync(Guid postId, string imageUrl)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "AddImage",
            "Post",
            async () =>
            {
                var entity = await context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
                if (entity is null)
                    return null;

                var imageUrls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.ImageUrls) ?? [];
                
                if (!imageUrls.Contains(imageUrl))
                {
                    imageUrls.Add(imageUrl);
                    entity.ImageUrls = System.Text.Json.JsonSerializer.Serialize(imageUrls);
                    await context.SaveChangesAsync();
                }

                return MapToModel(entity);
            },
            new { PostId = postId, ImageUrl = imageUrl });
    }

    public async Task<Post?> RemoveImageAsync(Guid postId, string imageUrl)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "RemoveImage",
            "Post",
            async () =>
            {
                var entity = await context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
                if (entity is null)
                    return null;

                var imageUrls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.ImageUrls) ?? [];
                
                if (imageUrls.Remove(imageUrl))
                {
                    entity.ImageUrls = System.Text.Json.JsonSerializer.Serialize(imageUrls);
                    await context.SaveChangesAsync();
                }

                return MapToModel(entity);
            },
            new { PostId = postId, ImageUrl = imageUrl });
    }

    public async Task<Post?> AssignTagsAsync(Guid postId, List<Guid> tagIds)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "AssignTags",
            "Post",
            async () =>
            {
                var entity = await context.Posts
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Include(p => p.Comments)
                    .FirstOrDefaultAsync(p => p.Id == postId);
                
                if (entity is null)
                    return null;

                // Remove existing tags
                context.PostTags.RemoveRange(entity.PostTags);

                // Add new tags
                foreach (var tagId in tagIds)
                {
                    entity.PostTags.Add(new PostTagEntity
                    {
                        PostId = postId,
                        TagId = tagId
                    });
                }

                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new { PostId = postId, TagCount = tagIds.Count });
    }

    private static Comment MapCommentToModel(CommentEntity entity) =>
        new(
            entity.Id,
            entity.PostId,
            entity.Author,
            entity.Content,
            entity.CreatedAt
        );
}
