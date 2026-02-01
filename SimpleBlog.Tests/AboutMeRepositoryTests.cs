using Microsoft.EntityFrameworkCore;
using SimpleBlog.Blog.Services;
using SimpleBlog.Common;

namespace SimpleBlog.Tests;

public sealed class AboutMeRepositoryTests
{
    private static BlogDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new BlogDbContext(options);
    }

    [Fact]
    public async Task GetAsync_WhenEntityExists_ReturnsAboutMe()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfAboutMeRepository(context, new NoOpOperationLogger());
        
        var aboutMeEntity = new AboutMeEntity
        {
            Id = Guid.NewGuid(),
            Content = "Test content about me",
            UpdatedAt = DateTimeOffset.UtcNow,
            UpdatedBy = "TestUser"
        };
        
        context.AboutMe.Add(aboutMeEntity);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(aboutMeEntity.Id, result.Id);
        Assert.Equal("Test content about me", result.Content);
        Assert.Equal("TestUser", result.UpdatedBy);
    }

    [Fact]
    public async Task GetAsync_WhenEntityDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfAboutMeRepository(context, new NoOpOperationLogger());

        // Act
        var result = await repository.GetAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityDoesNotExist_CreatesNewEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfAboutMeRepository(context, new NoOpOperationLogger());
        
        var request = new UpdateAboutMeRequest("New about me content", null);
        var updatedBy = "AdminUser";

        // Act
        var result = await repository.UpdateAsync(request, updatedBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New about me content", result.Content);
        Assert.Equal("AdminUser", result.UpdatedBy);
        Assert.True(result.UpdatedAt <= DateTimeOffset.UtcNow);
        
        // Verify it was saved to database
        var saved = await context.AboutMe.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("New about me content", saved.Content);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityExists_UpdatesExistingEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfAboutMeRepository(context, new NoOpOperationLogger());
        
        var originalId = Guid.NewGuid();
        var originalTimestamp = DateTimeOffset.UtcNow.AddHours(-1); // Make sure there's a clear time difference
        var originalEntity = new AboutMeEntity
        {
            Id = originalId,
            Content = "Original content",
            UpdatedAt = originalTimestamp,
            UpdatedBy = "OriginalUser"
        };
        
        context.AboutMe.Add(originalEntity);
        await context.SaveChangesAsync();
        
        var request = new UpdateAboutMeRequest("Updated content", null);
        var updatedBy = "UpdaterUser";

        // Act
        var result = await repository.UpdateAsync(request, updatedBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalId, result.Id); // ID should remain the same
        Assert.Equal("Updated content", result.Content);
        Assert.Equal("UpdaterUser", result.UpdatedBy);
        Assert.True(result.UpdatedAt >= originalTimestamp, 
            $"Expected UpdatedAt ({result.UpdatedAt}) to be >= original time ({originalTimestamp})");
        
        // Verify only one entity exists
        var count = await context.AboutMe.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task UpdateAsync_WithLongContent_SavesLongContent()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfAboutMeRepository(context, new NoOpOperationLogger());
        
        var longContent = new string('A', 5000);
        var request = new UpdateAboutMeRequest(longContent, null);
        var updatedBy = "TestUser";

        // Act
        var result = await repository.UpdateAsync(request, updatedBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5000, result.Content.Length);
        Assert.Equal(longContent, result.Content);
    }
}
