using System;
using System.Collections.Generic;

namespace SimpleBlog.ApiService.Data;

// Entity Framework entities
public class PostEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Author { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public string? ImageUrl { get; set; }

    public List<CommentEntity> Comments { get; set; } = new();
}

public class CommentEntity
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string Author { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public PostEntity? Post { get; set; }
}

// Domain models
public record Post(
    Guid Id,
    string Title,
    string Content,
    string Author,
    DateTimeOffset CreatedAt,
    IReadOnlyList<Comment> Comments,
    string? ImageUrl
);

public record Comment(
    Guid Id,
    Guid PostId,
    string Author,
    string Content,
    DateTimeOffset CreatedAt
);

// Request DTOs
public record CreatePostRequest(
    string Title,
    string Content,
    string? Author,
    string? ImageUrl
);

public record UpdatePostRequest(
    string? Title,
    string? Content,
    string? Author,
    string? ImageUrl
);

public record CreateCommentRequest(
    string? Author,
    string? Content
);

// Repository interface
public interface IPostRepository
{
    IEnumerable<Post> GetAll();
    Post? GetById(Guid id);
    Post Create(CreatePostRequest request);
    Post? Update(Guid id, UpdatePostRequest request);
    bool Delete(Guid id);
    IReadOnlyList<Comment>? GetComments(Guid postId);
    Comment? AddComment(Guid postId, CreateCommentRequest request);
}
