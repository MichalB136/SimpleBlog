using Microsoft.EntityFrameworkCore;
using SimpleBlog.Blog.Services;
using SimpleBlog.Common;

namespace SimpleBlog.Tests;

public sealed class TagRepositoryTests
{
    private static BlogDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new BlogDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTags_OrderedByName()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());
        
        var tag1 = new TagEntity
        {
            Id = Guid.NewGuid(),
            Name = "Casual",
            Slug = "casual",
            Color = "#FF5733",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        var tag2 = new TagEntity
        {
            Id = Guid.NewGuid(),
            Name = "Vintage",
            Slug = "vintage",
            Color = "#33FF57",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        var tag3 = new TagEntity
        {
            Id = Guid.NewGuid(),
            Name = "Boho",
            Slug = "boho",
            Color = "#3357FF",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Tags.AddRange(tag1, tag2, tag3);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Boho", result[0].Name); // Alphabetical order
        Assert.Equal("Casual", result[1].Name);
        Assert.Equal("Vintage", result[2].Name);
    }

    [Fact]
    public async Task GetByIdAsync_TagExists_ReturnsTag()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());
        
        var tagId = Guid.NewGuid();
        var tag = new TagEntity
        {
            Id = tagId,
            Name = "Summer",
            Slug = "summer",
            Color = "#FFFF00",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(tagId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tagId, result.Id);
        Assert.Equal("Summer", result.Name);
        Assert.Equal("summer", result.Slug);
        Assert.Equal("#FFFF00", result.Color);
    }

    [Fact]
    public async Task GetByIdAsync_TagDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBySlugAsync_TagExists_ReturnsTag()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());
        
        var tag = new TagEntity
        {
            Id = Guid.NewGuid(),
            Name = "Winter",
            Slug = "winter",
            Color = "#0000FF",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetBySlugAsync("winter");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Winter", result.Name);
        Assert.Equal("winter", result.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_TagDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());

        // Act
        var result = await repository.GetBySlugAsync("non-existent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesAndReturnsTag()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());
        
        var request = new CreateTagRequest("Elegant", "#FFD700");

        // Act
        var result = await repository.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Elegant", result.Name);
        Assert.Equal("#FFD700", result.Color);
        
        // Verify in database
        var savedTag = await context.Tags.FindAsync(result.Id);
        Assert.NotNull(savedTag);
        Assert.Equal("Elegant", savedTag.Name);
    }

    [Fact]
    public async Task CreateAsync_WithColor_CreatesTagWithColor()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());
        
        var request = new CreateTagRequest("Custom", "#00FF00");

        // Act
        var result = await repository.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("#00FF00", result.Color);
    }

    [Fact]
    public async Task UpdateAsync_TagExists_UpdatesAndReturnsTag()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());
        
        var tagId = Guid.NewGuid();
        var tag = new TagEntity
        {
            Id = tagId,
            Name = "Old Name",
            Slug = "old-name",
            Color = "#000000",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        
        var updateRequest = new UpdateTagRequest("New Name", "#FFFFFF");

        // Act
        var result = await repository.UpdateAsync(tagId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tagId, result.Id);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("#FFFFFF", result.Color);
        
        // Verify in database
        var updatedTag = await context.Tags.FindAsync(tagId);
        Assert.NotNull(updatedTag);
        Assert.Equal("New Name", updatedTag.Name);
    }

    [Fact]
    public async Task UpdateAsync_TagDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());
        var nonExistentId = Guid.NewGuid();
        
        var updateRequest = new UpdateTagRequest("New Name", "#FF00FF");

        // Act
        var result = await repository.UpdateAsync(nonExistentId, updateRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_TagExists_DeletesAndReturnsTrue()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());
        
        var tagId = Guid.NewGuid();
        var tag = new TagEntity
        {
            Id = tagId,
            Name = "To Delete",
            Slug = "to-delete",
            Color = "#FF0000",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.DeleteAsync(tagId);

        // Assert
        Assert.True(result);
        
        // Verify deleted from database
        var deletedTag = await context.Tags.FindAsync(tagId);
        Assert.Null(deletedTag);
    }

    [Fact]
    public async Task DeleteAsync_TagDoesNotExist_ReturnsFalse()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.DeleteAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfTagRepository(context, new NoOpOperationLogger());

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
