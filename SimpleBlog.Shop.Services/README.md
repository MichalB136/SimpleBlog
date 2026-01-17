# SimpleBlog.Shop.Services

## Overview

Domain services library for e-commerce functionality including products, orders, and order items.

## Technologies

- **.NET 9.0** - Framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database via Npgsql
- **Specification Pattern** - Query abstraction

## Project Structure

```
SimpleBlog.Shop.Services/
├── Data/
│   └── Migrations/           # EF Core migrations
├── Specifications/           # Query specifications
│   ├── OrderSpecifications.cs
│   └── ProductSpecifications.cs
├── Entities.cs               # Domain entities
├── ShopDbContext.cs          # Database context
├── EfOrderRepository.cs      # Order repository
└── EfProductRepository.cs    # Product repository
```

## Domain Entities

### Product

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageBase64 { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Order

```csharp
public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public ICollection<OrderItem> Items { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
```

### OrderItem

```csharp
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public Order Order { get; set; }
}
```

## Repositories

### IProductRepository

```csharp
Task<List<Product>> GetAllProductsAsync(CancellationToken ct = default);
Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default);
Task<Product> AddProductAsync(Product product, CancellationToken ct = default);
Task UpdateProductAsync(Product product, CancellationToken ct = default);
Task DeleteProductAsync(int id, CancellationToken ct = default);
Task<bool> ProductExistsAsync(int id, CancellationToken ct = default);
Task<bool> HasSufficientStockAsync(int id, int quantity, CancellationToken ct = default);
```

### IOrderRepository

```csharp
Task<List<Order>> GetOrdersByUserIdAsync(string userId, CancellationToken ct = default);
Task<Order?> GetOrderByIdAsync(int id, CancellationToken ct = default);
Task<Order> AddOrderAsync(Order order, CancellationToken ct = default);
Task UpdateOrderAsync(Order order, CancellationToken ct = default);
Task UpdateOrderStatusAsync(int id, OrderStatus status, CancellationToken ct = default);
```

## Specifications

### ProductSpecifications

```csharp
// Get in-stock products only
var spec = ProductSpecifications.InStock();
var products = await repository.ListAsync(spec);

// Get products under certain price
var spec = ProductSpecifications.UnderPrice(50m);
var affordableProducts = await repository.ListAsync(spec);

// Get products ordered by price
var spec = ProductSpecifications.OrderedByPrice();
var sortedProducts = await repository.ListAsync(spec);
```

### OrderSpecifications

```csharp
// Get orders for specific user
var spec = OrderSpecifications.ForUser(userId);
var userOrders = await repository.ListAsync(spec);

// Get orders with specific status
var spec = OrderSpecifications.WithStatus(OrderStatus.Pending);
var pendingOrders = await repository.ListAsync(spec);

// Get orders with items included
var spec = OrderSpecifications.WithItems();
var ordersWithItems = await repository.ListAsync(spec);
```

## Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName `
    --project SimpleBlog.Shop.Services `
    --context ShopDbContext

# Apply migrations
dotnet ef database update `
    --project SimpleBlog.Shop.Services `
    --context ShopDbContext

# Remove last migration
dotnet ef migrations remove `
    --project SimpleBlog.Shop.Services `
    --context ShopDbContext
```

## Usage in API

```csharp
// Register services
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IProductRepository, EfProductRepository>();
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();

// Use in endpoint
app.MapPost("/api/orders", async (
    CreateOrderRequest request,
    IOrderRepository orderRepo,
    IProductRepository productRepo) =>
{
    // Validate stock
    foreach (var item in request.Items)
    {
        if (!await productRepo.HasSufficientStockAsync(item.ProductId, item.Quantity))
        {
            return Results.BadRequest($"Insufficient stock for product {item.ProductId}");
        }
    }
    
    // Create order
    var order = new Order
    {
        UserId = userId,
        Items = request.Items,
        Total = CalculateTotal(request.Items)
    };
    
    await orderRepo.AddOrderAsync(order);
    return Results.Created($"/api/orders/{order.Id}", order);
});
```

## Business Logic

### Stock Management

```csharp
public async Task<bool> ProcessOrderAsync(Order order)
{
    foreach (var item in order.Items)
    {
        var product = await _productRepo.GetProductByIdAsync(item.ProductId);
        
        if (product.Stock < item.Quantity)
        {
            return false; // Insufficient stock
        }
        
        // Reduce stock
        product.Stock -= item.Quantity;
        await _productRepo.UpdateProductAsync(product);
    }
    
    return true;
}
```

### Order Total Calculation

```csharp
public decimal CalculateOrderTotal(Order order)
{
    return order.Items.Sum(item => item.Price * item.Quantity);
}
```

## Testing

```csharp
public class OrderRepositoryTests
{
    private readonly ShopDbContext _context;
    
    public OrderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        _context = new ShopDbContext(options);
    }
    
    [Fact]
    public async Task AddOrderAsync_CreatesOrder()
    {
        // Arrange
        var repository = new EfOrderRepository(_context);
        var order = new Order
        {
            UserId = "user1",
            CustomerName = "John Doe",
            Total = 100m
        };
        
        // Act
        var result = await repository.AddOrderAsync(order);
        
        // Assert
        Assert.NotEqual(0, result.Id);
    }
}
```

## Configuration

### Connection String

```json
{
  "ConnectionStrings": {
    "ShopDb": "Host=localhost;Database=blogdb;Username=postgres;Password=postgres"
  }
}
```

### DbContext Options

```csharp
services.AddDbContext<ShopDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging(isDevelopment);
    options.EnableDetailedErrors(isDevelopment);
});
```

## Dependencies

- `Microsoft.EntityFrameworkCore` - ORM framework
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL provider
- `SimpleBlog.Common` - Shared models and interfaces

## Best Practices

1. **Transaction Management** - Use transactions for order processing
2. **Stock Validation** - Always check stock before creating orders
3. **Decimal for Money** - Use `decimal` for prices, never `float` or `double`
4. **Audit Trail** - Track order status changes
5. **Idempotency** - Ensure order creation is idempotent
6. **Soft Deletes** - Consider soft deletes for products

## Related Documentation

- [Database Guide](../docs/development/database-guide.md) - Migration and setup
- [Architecture Overview](../docs/technical/architecture-overview.md) - Domain design
- [API Specification](../docs/technical/api-specification.md) - API endpoints

## Troubleshooting

### Stock Inconsistencies

Use database transactions:

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Update stock
    // Create order
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Decimal Precision Issues

Configure decimal precision in `OnModelCreating`:

```csharp
modelBuilder.Entity<Product>()
    .Property(p => p.Price)
    .HasPrecision(18, 2);
```

## Future Enhancements

- [ ] Add product categories
- [ ] Implement product variants (size, color)
- [ ] Add product reviews/ratings
- [ ] Support discount codes/coupons
- [ ] Add order shipping tracking
- [ ] Implement inventory alerts
- [ ] Support multiple warehouses
- [ ] Add payment gateway integration
