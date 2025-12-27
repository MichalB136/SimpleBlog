using Microsoft.EntityFrameworkCore;
using SimpleBlog.Blog.Services;
using SimpleBlog.Common;

namespace SimpleBlog.Tests;

public sealed class BlogRepositoryTests
{
    private static BlogDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new BlogDbContext(options);
    }

    [Fact]
    public void GetAll_ReturnsAllPosts_OrderedByCreatedAtDescending()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);
        
        var post1 = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "First Post",
            Content = "Content 1",
            Author = "Author 1",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
        };
        
        var post2 = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "Second Post",
            Content = "Content 2",
            Author = "Author 2",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        
        context.Posts.AddRange(post1, post2);
        context.SaveChanges();

        // Act
        var result = repository.GetAll().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Second Post", result[0].Title); // Most recent first
        Assert.Equal("First Post", result[1].Title);
    }

    [Fact]
    public void GetById_PostExists_ReturnsPost()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);
        
        var postId = Guid.NewGuid();
        var post = new PostEntity
        {
            Id = postId,
            Title = "Test Post",
            Content = "Test Content",
            Author = "Test Author",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Posts.Add(post);
        context.SaveChanges();

        // Act
        var result = repository.GetById(postId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Post", result.Title);
        Assert.Equal("Test Content", result.Content);
        Assert.Equal("Test Author", result.Author);
    }

    [Fact]
    public void GetById_PostDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);

        // Act
        var result = repository.GetById(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Create_ValidRequest_CreatesPost()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);
        
        var request = new CreatePostRequest(
            "New Post",
            "New Content",
            "New Author",
            "https://example.com/image.jpg"
        );

        // Act
        var result = repository.Create(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("New Post", result.Title);
        Assert.Equal("New Content", result.Content);
        Assert.Equal("New Author", result.Author);
        Assert.Equal("https://example.com/image.jpg", result.ImageUrl);
        
        // Verify it's in the database
        var savedPost = context.Posts.Find(result.Id);
        Assert.NotNull(savedPost);
        Assert.Equal("New Post", savedPost.Title);
    }

    [Fact]
    public void Update_PostExists_UpdatesPost()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);
        
        var postId = Guid.NewGuid();
        var post = new PostEntity
        {
            Id = postId,
            Title = "Original Title",
            Content = "Original Content",
            Author = "Original Author",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Posts.Add(post);
        context.SaveChanges();
        
        var updateRequest = new UpdatePostRequest(
            "Updated Title",
            "Updated Content",
            "Updated Author",
            "https://example.com/updated.jpg"
        );

        // Act
        var result = repository.Update(postId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Content", result.Content);
        Assert.Equal("Updated Author", result.Author);
        Assert.Equal("https://example.com/updated.jpg", result.ImageUrl);
        
        // Verify in database
        var updatedPost = context.Posts.Find(postId);
        Assert.NotNull(updatedPost);
        Assert.Equal("Updated Title", updatedPost.Title);
    }

    [Fact]
    public void Update_PostDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);
        
        var updateRequest = new UpdatePostRequest("Title", "Content", "Author", null);

        // Act
        var result = repository.Update(Guid.NewGuid(), updateRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Delete_PostExists_DeletesPost()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);
        
        var postId = Guid.NewGuid();
        var post = new PostEntity
        {
            Id = postId,
            Title = "To Delete",
            Content = "Content",
            Author = "Author",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Posts.Add(post);
        context.SaveChanges();

        // Act
        var result = repository.Delete(postId);

        // Assert
        Assert.True(result);
        Assert.Null(context.Posts.Find(postId));
    }

    [Fact]
    public void Delete_PostDoesNotExist_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);

        // Act
        var result = repository.Delete(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetComments_PostExists_ReturnsComments()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);
        
        var postId = Guid.NewGuid();
        var post = new PostEntity
        {
            Id = postId,
            Title = "Post with comments",
            Content = "Content",
            Author = "Author",
            CreatedAt = DateTimeOffset.UtcNow,
            Comments = new List<CommentEntity>
            {
                new() { Id = Guid.NewGuid(), Author = "Commenter 1", Content = "Comment 1", CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10) },
                new() { Id = Guid.NewGuid(), Author = "Commenter 2", Content = "Comment 2", CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5) }
            }
        };
        
        context.Posts.Add(post);
        context.SaveChanges();

        // Act
        var result = repository.GetComments(postId) ?? new List<Comment>();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(postId, c.PostId));
    }

    [Fact]
    public void AddComment_PostExists_AddsComment()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);
        
        var postId = Guid.NewGuid();
        var post = new PostEntity
        {
            Id = postId,
            Title = "Post",
            Content = "Content",
            Author = "Author",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Posts.Add(post);
        context.SaveChanges();
        
        var commentRequest = new CreateCommentRequest("Reader", "Great post!");

        // Act
        var result = repository.AddComment(postId, commentRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Great post!", result.Content);
        Assert.Equal("Reader", result.Author);
        Assert.Equal(postId, result.PostId);
        
        // Verify in database
        var savedComment = context.Comments.Find(result.Id);
        Assert.NotNull(savedComment);
    }

    [Fact]
    public void AddComment_PostDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context);
        
        var commentRequest = new CreateCommentRequest("Comment", "Author");

        // Act
        var result = repository.AddComment(Guid.NewGuid(), commentRequest);

        // Assert
        Assert.Null(result);
    }
}
