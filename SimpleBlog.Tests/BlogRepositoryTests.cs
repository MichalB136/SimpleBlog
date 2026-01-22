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
    public async Task GetAllAsync_ReturnsAllPosts_OrderedByCreatedAtDescending()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
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
        var result = await repository.GetAllAsync(null, 1, 10);

        // Assert
        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Second Post", result.Items[0].Title); // Most recent first
        Assert.Equal("First Post", result.Items[1].Title);
    }

    [Fact]
    public async Task GetByIdAsync_PostExists_ReturnsPost()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
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
        var result = await repository.GetByIdAsync(postId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Post", result.Title);
        Assert.Equal("Test Content", result.Content);
        Assert.Equal("Test Author", result.Author);
    }

    [Fact]
    public async Task GetByIdAsync_PostDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesPost()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
        var request = new CreatePostRequest(
            "New Post",
            "New Content",
            "New Author"
        );

        // Act
        var result = await repository.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("New Post", result.Title);
        Assert.Equal("New Content", result.Content);
        Assert.Equal("New Author", result.Author);
        Assert.Empty(result.ImageUrls);
        
        // Verify it's in the database
        var savedPost = context.Posts.Find(result.Id);
        Assert.NotNull(savedPost);
        Assert.Equal("New Post", savedPost.Title);
    }

    [Fact]
    public async Task UpdateAsync_PostExists_UpdatesPost()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
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
            "Updated Author"
        );

        // Act
        var result = await repository.UpdateAsync(postId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Content", result.Content);
        Assert.Equal("Updated Author", result.Author);
        
        // Verify in database
        var updatedPost = context.Posts.Find(postId);
        Assert.NotNull(updatedPost);
        Assert.Equal("Updated Title", updatedPost.Title);
    }

    [Fact]
    public async Task UpdateAsync_PostDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
        var updateRequest = new UpdatePostRequest("Title", "Content", "Author");

        // Act
        var result = await repository.UpdateAsync(Guid.NewGuid(), updateRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_PreservesOtherFields()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());

        var postId = Guid.NewGuid();
        var post = new PostEntity
        {
            Id = postId,
            Title = "Title",
            Content = "Content",
            Author = "Author",
            ImageUrls = "[]",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Posts.Add(post);
        context.SaveChanges();

        var updateRequest = new UpdatePostRequest("Updated", "Updated", "Author");

        var result = await repository.UpdateAsync(postId, updateRequest);

        Assert.NotNull(result);
        Assert.Empty(result.ImageUrls); // Images managed separately
    }

    [Fact]
    public async Task DeleteAsync_PostExists_DeletesPost()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
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
        var result = await repository.DeleteAsync(postId);

        // Assert
        Assert.True(result);
        Assert.Null(context.Posts.Find(postId));
    }

    [Fact]
    public async Task DeleteAsync_PostDoesNotExist_ReturnsFalse()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());

        // Act
        var result = await repository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetCommentsAsync_PostExists_ReturnsComments()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
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
        var result = await repository.GetCommentsAsync(postId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(postId, c.PostId));
    }

    [Fact]
    public async Task AddCommentAsync_PostExists_AddsComment()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
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
        var result = await repository.AddCommentAsync(postId, commentRequest);

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
    public async Task AddCommentAsync_ReturnsCommentsOrderedByCreatedAtDescending()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());

        var postId = Guid.NewGuid();
        var baseTime = DateTimeOffset.UtcNow.AddMinutes(-30);

        var post = new PostEntity
        {
            Id = postId,
            Title = "Post",
            Content = "Content",
            Author = "Author",
            CreatedAt = baseTime,
            Comments = new List<CommentEntity>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Author = "Older",
                    Content = "Older comment",
                    CreatedAt = baseTime.AddMinutes(-5)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Author = "Newer",
                    Content = "Newer comment",
                    CreatedAt = baseTime.AddMinutes(-1)
                }
            }
        };

        context.Posts.Add(post);
        context.SaveChanges();

        var added = await repository.AddCommentAsync(postId, new CreateCommentRequest("Reader", "Latest"));
        Assert.NotNull(added);

        var comments = await repository.GetCommentsAsync(postId);

        Assert.NotNull(comments);
        Assert.True(comments.Count >= 3);
        Assert.Equal("Latest", comments[0].Content); // newest comment first
        Assert.True(comments[0].CreatedAt >= comments[1].CreatedAt);
        Assert.True(comments[1].CreatedAt >= comments[2].CreatedAt);
    }

    [Fact]
    public async Task AddCommentAsync_PostDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
        var commentRequest = new CreateCommentRequest("Comment", "Author");

        // Act
        var result = await repository.AddCommentAsync(Guid.NewGuid(), commentRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_NoTagsFilter_ReturnsAllPosts()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
        var post1 = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "Post 1",
            Content = "Content 1",
            Author = "Author 1",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        var post2 = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "Post 2",
            Content = "Content 2",
            Author = "Author 2",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        
        context.Posts.AddRange(post1, post2);
        context.SaveChanges();

        // Act - No filter
        var result = await repository.GetAllAsync(null, 1, 10);

        // Assert
        Assert.Equal(2, result.Total);
    }

    [Fact]
    public async Task GetByIdAsync_WithTags_IncludesTags()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
        var tag1 = new TagEntity { Id = Guid.NewGuid(), Name = "Tag1", Slug = "tag1" };
        var tag2 = new TagEntity { Id = Guid.NewGuid(), Name = "Tag2", Slug = "tag2" };
        
        var post = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "Post with tags",
            Content = "Content",
            Author = "Author",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Tags.AddRange(tag1, tag2);
        context.Posts.Add(post);
        context.PostTags.AddRange(
            new PostTagEntity { PostId = post.Id, TagId = tag1.Id },
            new PostTagEntity { PostId = post.Id, TagId = tag2.Id }
        );
        
        context.SaveChanges();

        // Act
        var result = await repository.GetByIdAsync(post.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Tags.Count);
        Assert.Contains(result.Tags, t => t.Name == "Tag1");
        Assert.Contains(result.Tags, t => t.Name == "Tag2");
    }

    [Fact]
    public async Task GetByIdAsync_WithComments_IncludesComments()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
        var post = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "Post with comments",
            Content = "Content",
            Author = "Author",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        var comment1 = new CommentEntity
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            Content = "Comment 1",
            Author = "Commenter 1",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        var comment2 = new CommentEntity
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            Content = "Comment 2",
            Author = "Commenter 2",
            CreatedAt = DateTimeOffset.UtcNow.AddSeconds(-10)
        };
        
        context.Posts.Add(post);
        context.Comments.AddRange(comment1, comment2);
        context.SaveChanges();

        // Act
        var result = await repository.GetByIdAsync(post.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Comments.Count);
    }

    [Fact]
    public async Task SetPinnedAsync_PinPost_SetsIsPinnedTrue()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
        var post = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "Post to pin",
            Content = "Content",
            Author = "Author",
            CreatedAt = DateTimeOffset.UtcNow,
            IsPinned = false
        };
        
        context.Posts.Add(post);
        context.SaveChanges();

        // Act
        await repository.SetPinnedAsync(post.Id, true);
        var result = await repository.GetByIdAsync(post.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsPinned);
    }

    [Fact]
    public async Task SetPinnedAsync_UnpinPost_SetsIsPinnedFalse()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
        var post = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "Post to unpin",
            Content = "Content",
            Author = "Author",
            CreatedAt = DateTimeOffset.UtcNow,
            IsPinned = true
        };
        
        context.Posts.Add(post);
        context.SaveChanges();

        // Act
        await repository.SetPinnedAsync(post.Id, false);
        var result = await repository.GetByIdAsync(post.Id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsPinned);
    }

    [Fact]
    public async Task AddCommentAsync_AddsCommentToPost()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfPostRepository(context, new NoOpOperationLogger());
        
        var post = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "Post with future comment",
            Content = "Content",
            Author = "Author",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Posts.Add(post);
        context.SaveChanges();
        
        var commentRequest = new CreateCommentRequest("Reader", "Great post!");

        // Act
        var result = await repository.AddCommentAsync(post.Id, commentRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Great post!", result.Content);
        Assert.Equal("Reader", result.Author);
        
        var updatedPost = await repository.GetByIdAsync(post.Id);
        Assert.NotNull(updatedPost);
        Assert.Single(updatedPost.Comments);
    }
}

