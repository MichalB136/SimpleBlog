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
    public void GetAll_Products_ReturnsAllOrderedByCreatedAt()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context);
        
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
        var result = repository.GetAll().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Product 2", result[0].Name); // Most recent first
    }

    [Fact]
    public void Create_Product_ValidData_CreatesProduct()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context);
        
        var request = new CreateProductRequest(
            "New Product",
            "Product Description",
            99.99m,
            "https://example.com/image.jpg",
            "Electronics",
            50
        );

        // Act
        var result = repository.Create(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("New Product", result.Name);
        Assert.Equal(99.99m, result.Price);
        Assert.Equal(50, result.Stock);
        Assert.Equal("Electronics", result.Category);
    }

    [Fact]
    public void Update_Product_Exists_UpdatesSuccessfully()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context);
        
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
        var result = repository.Update(productId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal(20.00m, result.Price);
        Assert.Equal(15, result.Stock);
    }

    [Fact]
    public void Delete_Product_Exists_DeletesSuccessfully()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context);
        
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
        var result = repository.Delete(productId);

        // Assert
        Assert.True(result);
        Assert.Null(context.Products.Find(productId));
    }

    [Fact]
    public void Create_Order_ValidData_CreatesOrderWithItems()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context);
        
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
        var result = repository.Create(request);

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
    public void GetAll_Orders_ReturnsAllOrders()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context);
        
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
        var result = repository.GetAll().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Customer 2", result[0].CustomerName); // Most recent first
    }

    [Theory]
    [InlineData(0)] // Zero quantity
    [InlineData(-1)] // Negative quantity
    public void Create_Order_InvalidQuantity_ThrowsOrHandlesGracefully(int quantity)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context);
        
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
        var result = repository.Create(request);
        Assert.NotNull(result); // Currently doesn't validate, but documents the behavior
    }

    [Fact]
    public void GetById_Product_Exists_ReturnsProduct()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context);
        
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
        var result = repository.GetById(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(50.00m, result.Price);
    }

    [Fact]
    public void GetById_Product_DoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new EfProductRepository(context);

        // Act
        var result = repository.GetById(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
