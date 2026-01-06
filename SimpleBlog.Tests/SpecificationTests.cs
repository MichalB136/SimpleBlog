using Xunit;
using SimpleBlog.Blog.Services;
using SimpleBlog.Blog.Services.Specifications;
using SimpleBlog.Shop.Services;
using SimpleBlog.Shop.Services.Specifications;
using SimpleBlog.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SimpleBlog.Tests;

public sealed class SpecificationTests
{
    [Fact]
    public async Task PostsWithCommentsSpecification_LoadsCommentsEagerly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        await using var context = new BlogDbContext(options);
        
        var post = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Post",
            Content = "Content",
            Author = "TestAuthor",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        var comment = new CommentEntity
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            Author = "Commenter",
            Content = "Comment content",
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Posts.Add(post);
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        var spec = new PostsWithCommentsSpecification();

        // Act
        var result = await context.Posts
            .ApplySpecification(spec)
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Comments);
        Assert.Single(result.Comments);
    }

    [Fact]
    public async Task PostsByAuthorSpecification_FiltersCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        await using var context = new BlogDbContext(options);
        
        var targetAuthor = "TargetAuthor";
        context.Posts.AddRange(
            new PostEntity { Id = Guid.NewGuid(), Title = "Post 1", Content = "C1", Author = targetAuthor, CreatedAt = DateTimeOffset.UtcNow },
            new PostEntity { Id = Guid.NewGuid(), Title = "Post 2", Content = "C2", Author = "OtherAuthor", CreatedAt = DateTimeOffset.UtcNow },
            new PostEntity { Id = Guid.NewGuid(), Title = "Post 3", Content = "C3", Author = targetAuthor, CreatedAt = DateTimeOffset.UtcNow }
        );
        await context.SaveChangesAsync();

        var spec = new PostsByAuthorSpecification(targetAuthor);

        // Act
        var result = await context.Posts
            .ApplySpecification(spec)
            .ToListAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(targetAuthor, p.Author));
    }

    [Fact]
    public async Task PostsSearchSpecification_SearchesTitleAndContent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        await using var context = new BlogDbContext(options);
        
        context.Posts.AddRange(
            new PostEntity { Id = Guid.NewGuid(), Title = "Keyword in title", Content = "Other content", Author = "A", CreatedAt = DateTimeOffset.UtcNow },
            new PostEntity { Id = Guid.NewGuid(), Title = "Other title", Content = "Keyword in content", Author = "A", CreatedAt = DateTimeOffset.UtcNow },
            new PostEntity { Id = Guid.NewGuid(), Title = "Nothing here", Content = "Nothing here", Author = "A", CreatedAt = DateTimeOffset.UtcNow }
        );
        await context.SaveChangesAsync();

        var spec = new PostsSearchSpecification("keyword");

        // Act
        var result = await context.Posts
            .ApplySpecification(spec)
            .ToListAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ProductsInStockSpecification_FiltersCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        await using var context = new ShopDbContext(options);
        
        context.Products.AddRange(
            new ProductEntity { Id = Guid.NewGuid(), Name = "P1", Description = "D", Price = 10, Category = "C", Stock = 5, CreatedAt = DateTimeOffset.UtcNow },
            new ProductEntity { Id = Guid.NewGuid(), Name = "P2", Description = "D", Price = 20, Category = "C", Stock = 0, CreatedAt = DateTimeOffset.UtcNow },
            new ProductEntity { Id = Guid.NewGuid(), Name = "P3", Description = "D", Price = 30, Category = "C", Stock = 10, CreatedAt = DateTimeOffset.UtcNow }
        );
        await context.SaveChangesAsync();

        var spec = new ProductsInStockSpecification();

        // Act
        var result = await context.Products
            .ApplySpecification(spec)
            .ToListAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.True(p.Stock > 0));
    }

    [Fact]
    public async Task ProductsByCategorySpecification_FiltersCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        await using var context = new ShopDbContext(options);
        
        var targetCategory = "Electronics";
        context.Products.AddRange(
            new ProductEntity { Id = Guid.NewGuid(), Name = "P1", Description = "D", Price = 10, Category = targetCategory, Stock = 5, CreatedAt = DateTimeOffset.UtcNow },
            new ProductEntity { Id = Guid.NewGuid(), Name = "P2", Description = "D", Price = 20, Category = "Books", Stock = 5, CreatedAt = DateTimeOffset.UtcNow },
            new ProductEntity { Id = Guid.NewGuid(), Name = "P3", Description = "D", Price = 30, Category = targetCategory, Stock = 5, CreatedAt = DateTimeOffset.UtcNow }
        );
        await context.SaveChangesAsync();

        var spec = new ProductsByCategorySpecification(targetCategory);

        // Act
        var result = await context.Products
            .ApplySpecification(spec)
            .ToListAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(targetCategory, p.Category));
    }

    [Fact]
    public async Task ProductsByPriceRangeSpecification_FiltersCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        await using var context = new ShopDbContext(options);
        
        context.Products.AddRange(
            new ProductEntity { Id = Guid.NewGuid(), Name = "P1", Description = "D", Price = 10, Category = "C", Stock = 5, CreatedAt = DateTimeOffset.UtcNow },
            new ProductEntity { Id = Guid.NewGuid(), Name = "P2", Description = "D", Price = 50, Category = "C", Stock = 5, CreatedAt = DateTimeOffset.UtcNow },
            new ProductEntity { Id = Guid.NewGuid(), Name = "P3", Description = "D", Price = 150, Category = "C", Stock = 5, CreatedAt = DateTimeOffset.UtcNow }
        );
        await context.SaveChangesAsync();

        var spec = new ProductsByPriceRangeSpecification(20, 100);

        // Act
        var result = await context.Products
            .ApplySpecification(spec)
            .ToListAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(50, result[0].Price);
    }

    [Fact]
    public async Task OrdersByCustomerEmailSpecification_FiltersCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        await using var context = new ShopDbContext(options);
        
        var targetEmail = "customer@test.com";
        context.Orders.AddRange(
            new OrderEntity 
            { 
                Id = Guid.NewGuid(), 
                CustomerName = "C1", 
                CustomerEmail = targetEmail, 
                CustomerPhone = "123",
                ShippingAddress = "Addr", 
                ShippingCity = "City", 
                ShippingPostalCode = "00-000",
                TotalAmount = 100, 
                CreatedAt = DateTimeOffset.UtcNow,
                Items = new List<OrderItemEntity>()
            },
            new OrderEntity 
            { 
                Id = Guid.NewGuid(), 
                CustomerName = "C2", 
                CustomerEmail = "other@test.com", 
                CustomerPhone = "123",
                ShippingAddress = "Addr", 
                ShippingCity = "City", 
                ShippingPostalCode = "00-000",
                TotalAmount = 200, 
                CreatedAt = DateTimeOffset.UtcNow,
                Items = new List<OrderItemEntity>()
            }
        );
        await context.SaveChangesAsync();

        var spec = new OrdersByCustomerEmailSpecification(targetEmail);

        // Act
        var result = await context.Orders
            .ApplySpecification(spec)
            .ToListAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(targetEmail, result[0].CustomerEmail);
    }

    [Fact]
    public async Task PostsCreatedAfterSpecification_FiltersCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        await using var context = new BlogDbContext(options);
        
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-7);
        
        context.Posts.AddRange(
            new PostEntity { Id = Guid.NewGuid(), Title = "Old Post", Content = "C", Author = "A", CreatedAt = cutoffDate.AddDays(-2) },
            new PostEntity { Id = Guid.NewGuid(), Title = "Recent Post", Content = "C", Author = "A", CreatedAt = cutoffDate.AddDays(2) }
        );
        await context.SaveChangesAsync();

        var spec = new PostsCreatedAfterSpecification(cutoffDate);

        // Act
        var result = await context.Posts
            .ApplySpecification(spec)
            .ToListAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Recent Post", result[0].Title);
    }
}
