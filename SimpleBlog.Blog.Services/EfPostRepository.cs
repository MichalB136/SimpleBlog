using SimpleBlog.Common;

namespace SimpleBlog.Blog.Services;

public sealed class EfPostRepository(BlogDbContext context) : IPostRepository
{
    public IEnumerable<Post> GetAll()
    {
        var entities = context.Posts.OrderByDescending(p => p.CreatedAt).ToList();
        return entities.Select(MapToModel);
    }

    public Post? GetById(Guid id)
    {
        var entity = context.Posts
            .AsEnumerable()
            .FirstOrDefault(p => p.Id == id);
        return entity is not null ? MapToModel(entity) : null;
    }

    public Post Create(CreatePostRequest request)
    {
        var entity = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            Author = request.Author ?? "Anon",
            CreatedAt = DateTimeOffset.UtcNow,
            ImageUrl = request.ImageUrl
        };

        context.Posts.Add(entity);
        context.SaveChanges();
        return MapToModel(entity);
    }

    public Post? Update(Guid id, UpdatePostRequest request)
    {
        var entity = context.Posts.FirstOrDefault(p => p.Id == id);
        if (entity is null)
            return null;

        if (request.Title is not null)
            entity.Title = request.Title;
        if (request.Content is not null)
            entity.Content = request.Content;
        if (request.Author is not null)
            entity.Author = request.Author;
        if (request.ImageUrl is not null)
            entity.ImageUrl = request.ImageUrl;

        context.SaveChanges();
        return MapToModel(entity);
    }

    public bool Delete(Guid id)
    {
        var entity = context.Posts.FirstOrDefault(p => p.Id == id);
        if (entity is null)
            return false;

        context.Posts.Remove(entity);
        context.SaveChanges();
        return true;
    }

    public IReadOnlyList<Comment>? GetComments(Guid postId)
    {
        var post = context.Posts
            .AsEnumerable()
            .FirstOrDefault(p => p.Id == postId);
        return post?.Comments?.Select(MapCommentToModel).ToList() ?? new List<Comment>();
    }

    public Comment? AddComment(Guid postId, CreateCommentRequest request)
    {
        var post = context.Posts.FirstOrDefault(p => p.Id == postId);
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
        context.SaveChanges();
        return MapCommentToModel(comment);
    }

    private static Post MapToModel(PostEntity entity) =>
        new(
            entity.Id,
            entity.Title,
            entity.Content,
            entity.Author,
            entity.CreatedAt,
            entity.Comments.Select(MapCommentToModel).ToList(),
            entity.ImageUrl
        );

    private static Comment MapCommentToModel(CommentEntity entity) =>
        new(
            entity.Id,
            entity.PostId,
            entity.Author,
            entity.Content,
            entity.CreatedAt
        );
}
