namespace SimpleBlog.Blog.Services;

public class PostEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Author { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPinned { get; set; }

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

public class AboutMeEntity
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = null!;
}

public class SiteSettingsEntity
{
    public Guid Id { get; set; }
    public string Theme { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = null!;
}
