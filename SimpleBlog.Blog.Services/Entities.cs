namespace SimpleBlog.Blog.Services;

public class PostEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Author { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public string ImageUrls { get; set; } = "[]"; // JSON array of image URLs
    public bool IsPinned { get; set; }

    public List<CommentEntity> Comments { get; set; } = new();
    public List<PostTagEntity> PostTags { get; set; } = new();
}

public class TagEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Color { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<PostTagEntity> PostTags { get; set; } = new();
}

public class PostTagEntity
{
    public Guid PostId { get; set; }
    public Guid TagId { get; set; }

    public PostEntity Post { get; set; } = null!;
    public TagEntity Tag { get; set; } = null!;
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
