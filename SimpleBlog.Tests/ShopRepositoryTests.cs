using Microsoft.EntityFrameworkCore;
using SimpleBlog.Shop.Services;
using SimpleBlog.Common;

namespace SimpleBlog.Tests;

public sealed class ShopRepositoryTests
{
    private static ShopDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ShopDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_Products_ReturnsAllOrderedByCreatedAt()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context, new NoOpOperationLogger());
        
        var product1 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Product 1",
            Description = "Desc 1",
            Price = 10.00m,
            Category = "Cat1",
            Stock = 5,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        
        var product2 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Product 2",
            Description = "Desc 2",
            Price = 20.00m,
            Category = "Cat2",
            Stock = 10,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Products.AddRange(product1, product2);
        context.SaveChanges();

        // Act
        var result = await repository.GetAllAsync(null, 1, 10);

        // Assert
        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Product 2", result.Items[0].Name); // Most recent first
    }

    [Fact]
    public async Task CreateAsync_Product_ValidData_CreatesProduct()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context, new NoOpOperationLogger());
        
        var request = new CreateProductRequest(
            "New Product",
            "Product Description",
            99.99m,
            "https://example.com/image.jpg",
            "Electronics",
            50
        );

        // Act
        var result = await repository.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("New Product", result.Name);
        Assert.Equal(99.99m, result.Price);
        Assert.Equal(50, result.Stock);
        Assert.Equal("Electronics", result.Category);
    }

    [Fact]
    public async Task UpdateAsync_Product_Exists_UpdatesSuccessfully()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context, new NoOpOperationLogger());
        
        var productId = Guid.NewGuid();
        var product = new ProductEntity
        {
            Id = productId,
            Name = "Old Name",
            Description = "Old Desc",
            Price = 10.00m,
            Category = "OldCat",
            Stock = 5,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Products.Add(product);
        context.SaveChanges();
        
        var updateRequest = new UpdateProductRequest(
            "New Name",
            "New Desc",
            20.00m,
            "https://example.com/new.jpg",
            "NewCat",
            15
        );

        // Act
        var result = await repository.UpdateAsync(productId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal(20.00m, result.Price);
        Assert.Equal(15, result.Stock);
    }

    [Fact]
    public async Task UpdateAsync_Product_DoesNotExist_ReturnsNull()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context, new NoOpOperationLogger());

        var updateRequest = new UpdateProductRequest(
            "Name",
            "Desc",
            10.00m,
            null,
            "Cat",
            1);

        var result = await repository.UpdateAsync(Guid.NewGuid(), updateRequest);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_Product_Exists_DeletesSuccessfully()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context, new NoOpOperationLogger());
        
        var productId = Guid.NewGuid();
        var product = new ProductEntity
        {
            Id = productId,
            Name = "To Delete",
            Description = "Desc",
            Price = 10.00m,
            Category = "Cat",
            Stock = 5,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Products.Add(product);
        context.SaveChanges();

        // Act
        var result = await repository.DeleteAsync(productId);

        // Assert
        Assert.True(result);
        Assert.Null(context.Products.Find(productId));
    }

    [Fact]
    public async Task DeleteAsync_Product_DoesNotExist_ReturnsFalse()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context, new NoOpOperationLogger());

        var result = await repository.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task CreateAsync_Order_ValidData_CreatesOrderWithItems()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context, new NoOpOperationLogger());
        
        // Add products first
        var product1 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Product 1",
            Description = "Test Product 1",
            Price = 10.00m,
            Stock = 10,
            Category = "Test",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var product2 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Product 2",
            Description = "Test Product 2",
            Price = 20.00m,
            Stock = 10,
            Category = "Test",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Products.Add(product1);
        context.Products.Add(product2);
        context.SaveChanges();
        
        var request = new CreateOrderRequest(
            "John Doe",
            "john@example.com",
            "123456789",
            "Main Street 1",
            "Warsaw",
            "00-001",
            new List<OrderItemRequest>
            {
                new(product1.Id, 2),
                new(product2.Id, 1)
            }
        );

        // Act
        var result = await repository.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("John Doe", result.CustomerName);
        Assert.Equal("john@example.com", result.CustomerEmail);
        Assert.Equal(40.00m, result.TotalAmount); // (10*2) + (20*1)
        Assert.Equal(2, result.Items.Count);
        
        // Verify in database
        var savedOrder = context.Orders
            .Include(o => o.Items)
            .FirstOrDefault(o => o.Id == result.Id);
        Assert.NotNull(savedOrder);
        Assert.Equal(2, savedOrder.Items.Count);
    }

    [Fact]
    public async Task GetAllAsync_Orders_ReturnsAllOrders()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context, new NoOpOperationLogger());
        
        var order1 = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerName = "Customer 1",
            CustomerEmail = "customer1@test.com",
            CustomerPhone = "111",
            ShippingAddress = "Addr 1",
            ShippingCity = "City 1",
            ShippingPostalCode = "00-001",
            TotalAmount = 100.00m,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            Items = new List<OrderItemEntity>()
        };
        
        var order2 = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerName = "Customer 2",
            CustomerEmail = "customer2@test.com",
            CustomerPhone = "222",
            ShippingAddress = "Addr 2",
            ShippingCity = "City 2",
            ShippingPostalCode = "00-002",
            TotalAmount = 200.00m,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = new List<OrderItemEntity>()
        };
        
        context.Orders.AddRange(order1, order2);
        context.SaveChanges();

        // Act
        var result = await repository.GetAllAsync(1, 10);

        // Assert
        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Customer 2", result.Items[0].CustomerName); // Most recent first
    }

    [Fact]
    public async Task GetByIdAsync_Order_ReturnsOrderWithItems()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context, new NoOpOperationLogger());

        var orderId = Guid.NewGuid();
        var order = new OrderEntity
        {
            Id = orderId,
            CustomerName = "Customer",
            CustomerEmail = "cust@test.com",
            CustomerPhone = "111",
            ShippingAddress = "Addr",
            ShippingCity = "City",
            ShippingPostalCode = "00-001",
            TotalAmount = 30m,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = new List<OrderItemEntity>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "P1",
                    Price = 10m,
                    Quantity = 2
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "P2",
                    Price = 10m,
                    Quantity = 1
                }
            }
        };

        context.Orders.Add(order);
        context.SaveChanges();

        var result = await repository.GetByIdAsync(orderId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(30m, result.TotalAmount);
    }

    [Theory]
    [InlineData(0)] // Zero quantity
    [InlineData(-1)] // Negative quantity
    public async Task CreateAsync_Order_InvalidQuantity_ThrowsOrHandlesGracefully(int quantity)
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context, new NoOpOperationLogger());
        
        var request = new CreateOrderRequest(
            "John Doe",
            "john@example.com",
            "123456789",
            "Main Street 1",
            "Warsaw",
            "00-001",
            new List<OrderItemRequest>
            {
                new(Guid.NewGuid(), quantity)
            }
        );

        // Act & Assert
        // This test documents current behavior - ideally should validate quantity
        var result = await repository.CreateAsync(request);
        Assert.NotNull(result); // Currently doesn't validate, but documents the behavior
    }

    [Fact]
    public async Task CreateAsync_Order_WithInsufficientStock_DocumentsCurrentBehavior()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context, new NoOpOperationLogger());

        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Limited",
            Description = "Low stock",
            Price = 5m,
            Stock = 1,
            Category = "Test",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Products.Add(product);
        context.SaveChanges();

        var request = new CreateOrderRequest(
            "Jane Doe",
            "jane@example.com",
            "999",
            "Addr",
            "City",
            "00-002",
            new List<OrderItemRequest>
            {
                new(product.Id, 5) // exceeds stock; behavior is currently not validated
            });

        var result = await repository.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(25m, result.TotalAmount); // documents that calculation ignores stock availability
        Assert.Single(result.Items);
        Assert.Equal(5, result.Items[0].Quantity);
    }

    [Fact]
    public async Task GetByIdAsync_Product_Exists_ReturnsProduct()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context, new NoOpOperationLogger());
        
        var productId = Guid.NewGuid();
        var product = new ProductEntity
        {
            Id = productId,
            Name = "Test Product",
            Description = "Description",
            Price = 50.00m,
            Category = "Test",
            Stock = 10,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Products.Add(product);
        context.SaveChanges();

        // Act
        var result = await repository.GetByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(50.00m, result.Price);
    }

    [Fact]
    public async Task GetByIdAsync_Product_DoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context, new NoOpOperationLogger());

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_Product_WithColors_CreatesProductWithColors()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context, new NoOpOperationLogger());

        var colors = new List<string> { "#000000", "#ffffff", "red" };
        var request = new CreateProductRequest(
            "Colored Product",
            "Has colors",
            25.00m,
            "https://example.com/image.jpg",
            "Apparel",
            10,
            colors
        );

        // Act
        var result = await repository.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Colors?.Count ?? 0);

        // Verify persisted in database
        var saved = context.Products.Include(p => p.ProductColors).FirstOrDefault(p => p.Id == result.Id);
        Assert.NotNull(saved);
        Assert.Equal(3, saved.ProductColors.Count);
        var savedColors = saved.ProductColors.Select(pc => pc.Color).ToList();
        Assert.Equal(colors.Count, savedColors.Count);
        foreach (var c in colors)
            Assert.Contains(c, savedColors);
    }

    [Fact]
    public async Task UpdateAsync_Product_UpdateColors_PersistsColors()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context, new NoOpOperationLogger());

        var productId = Guid.NewGuid();
        var product = new ProductEntity
        {
            Id = productId,
            Name = "No Colors",
            Description = "Initially no colors",
            Price = 15.00m,
            Category = "Test",
            Stock = 5,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Products.Add(product);
        context.SaveChanges();

        var newColors = new List<string> { "blue", "green" };
        var updateRequest = new UpdateProductRequest(
            null,
            null,
            null,
            null,
            null,
            null,
            newColors
        );

        // Act
        var updated = await repository.UpdateAsync(productId, updateRequest);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(2, updated.Colors?.Count ?? 0);

        var saved = context.Products.Include(p => p.ProductColors).FirstOrDefault(p => p.Id == productId);
        Assert.NotNull(saved);
        Assert.Equal(2, saved.ProductColors.Count);
        var savedUpdatedColors = saved.ProductColors.Select(pc => pc.Color).ToList();
        Assert.Equal(newColors.Count, savedUpdatedColors.Count);
        foreach (var c in newColors)
            Assert.Contains(c, savedUpdatedColors);
    }
}
