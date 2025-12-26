using System;
using System.Collections.Generic;

namespace SimpleBlog.ApiService.Data;

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
