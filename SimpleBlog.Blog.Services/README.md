# SimpleBlog.Blog.Services

## Overview

Domain services library for blog functionality including posts, comments, and about page content.

## Technologies

- **.NET 9.0** - Framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database via Npgsql
- **Specification Pattern** - Query abstraction

## Project Structure

```
SimpleBlog.Blog.Services/
├── Data/
│   └── Migrations/           # EF Core migrations
├── Specifications/           # Query specifications
│   ├── CommentSpecifications.cs
│   └── PostSpecifications.cs
├── BlogDbContext.cs          # Database context
├── Entities.cs               # Domain entities
├── EfAboutMeRepository.cs    # About page repository
└── EfPostRepository.cs       # Post repository
```

## Domain Entities

### Post

```csharp
public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string? ImageBase64 { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool IsPinned { get; set; }
    public ICollection<Comment> Comments { get; set; }
}
```

### Comment

```csharp
public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string Author { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public Post Post { get; set; }
}
```

### AboutMe

```csharp
public class AboutMe
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

## Repositories

### IPostRepository

```csharp
Task<List<Post>> GetAllPostsAsync(CancellationToken ct = default);
Task<Post?> GetPostByIdAsync(int id, CancellationToken ct = default);
Task<Post> AddPostAsync(Post post, CancellationToken ct = default);
Task UpdatePostAsync(Post post, CancellationToken ct = default);
Task DeletePostAsync(int id, CancellationToken ct = default);
Task<bool> PostExistsAsync(int id, CancellationToken ct = default);
```

### IAboutMeRepository

```csharp
Task<AboutMe?> GetAboutMeAsync(CancellationToken ct = default);
Task<AboutMe> UpdateAboutMeAsync(AboutMe aboutMe, CancellationToken ct = default);
```

## Specifications

### Purpose

Specifications encapsulate complex queries and make them reusable and testable.

### PostSpecifications

```csharp
// Get posts with comments included
var spec = PostSpecifications.WithComments();
var posts = await repository.ListAsync(spec);

// Get pinned posts only
var spec = PostSpecifications.PinnedPosts();
var pinnedPosts = await repository.ListAsync(spec);

// Get posts ordered by date (newest first)
var spec = PostSpecifications.OrderedByDate();
var orderedPosts = await repository.ListAsync(spec);
```

### CommentSpecifications

```csharp
// Get comments for specific post
var spec = CommentSpecifications.ForPost(postId);
var comments = await repository.ListAsync(spec);

// Get comments by author
var spec = CommentSpecifications.ByAuthor(username);
var userComments = await repository.ListAsync(spec);
```

## Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName `
    --project SimpleBlog.Blog.Services `
    --context BlogDbContext

# Apply migrations
dotnet ef database update `
    --project SimpleBlog.Blog.Services `
    --context BlogDbContext

# Remove last migration
dotnet ef migrations remove `
    --project SimpleBlog.Blog.Services `
    --context BlogDbContext
```

## Usage in API

```csharp
// Register services
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IPostRepository, EfPostRepository>();
builder.Services.AddScoped<IAboutMeRepository, EfAboutMeRepository>();

// Use in endpoint
app.MapGet("/api/posts", async (IPostRepository repo) =>
{
    var posts = await repo.GetAllPostsAsync();
    return Results.Ok(posts);
});
```

## Testing

```csharp
public class PostRepositoryTests
{
    private readonly BlogDbContext _context;
    
    public PostRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        _context = new BlogDbContext(options);
    }
    
    [Fact]
    public async Task GetAllPostsAsync_ReturnsAllPosts()
    {
        // Arrange
        var repository = new EfPostRepository(_context);
        
        // Act
        var posts = await repository.GetAllPostsAsync();
        
        // Assert
        Assert.NotEmpty(posts);
    }
}
```

## Configuration

### Connection String

```json
{
  "ConnectionStrings": {
    "BlogDb": "Host=localhost;Database=blogdb;Username=postgres;Password=postgres"
  }
}
```

### DbContext Options

```csharp
services.AddDbContext<BlogDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging(isDevelopment);
    options.LogTo(Console.WriteLine, LogLevel.Information);
});
```

## Dependencies

- `Microsoft.EntityFrameworkCore` - ORM framework
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL provider
- `SimpleBlog.Common` - Shared models and interfaces

## Best Practices

1. **Use Specifications** - Encapsulate complex queries
2. **Async All The Way** - All repository methods are async
3. **CancellationToken** - Support request cancellation
4. **Include Related Data** - Use `.Include()` for related entities
5. **Track Changes** - Be mindful of change tracking
6. **Dispose Contexts** - Use dependency injection scope

## Related Documentation

- [Database Guide](../docs/development/database-guide.md) - Migration and setup
- [Architecture Overview](../docs/technical/architecture-overview.md) - Domain design
- [Entity Framework Core](https://learn.microsoft.com/ef/core/) - Official docs

## Troubleshooting

### Migration Errors

```bash
# Check pending migrations
dotnet ef migrations list --context BlogDbContext

# Reset database (development only!)
dotnet ef database drop --context BlogDbContext
dotnet ef database update --context BlogDbContext
```

### Connection Issues

1. Verify PostgreSQL is running
2. Check connection string format
3. Ensure database exists
4. Verify user permissions

### Performance Issues

1. Use `.AsNoTracking()` for read-only queries
2. Include related data with `.Include()`
3. Use projections with `.Select()`
4. Add database indexes for frequently queried columns

## Future Enhancements

- [ ] Add post categories/tags
- [ ] Implement full-text search
- [ ] Add post versioning/history
- [ ] Support post attachments
- [ ] Add comment threading
- [ ] Implement post likes/reactions
