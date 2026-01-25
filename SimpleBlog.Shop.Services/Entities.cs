namespace SimpleBlog.Shop.Services;

public class ProductEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = null!;
    public int Stock { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<ProductTagEntity> ProductTags { get; set; } = new();
    // Available colors for this product (e.g. "#ffffff" or "Red")
    public List<ProductColorEntity> ProductColors { get; set; } = new();
}

public class TagEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Color { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<ProductTagEntity> ProductTags { get; set; } = new();
}

public class ProductTagEntity
{
    public Guid ProductId { get; set; }
    public Guid TagId { get; set; }

    public ProductEntity Product { get; set; } = null!;
    public TagEntity Tag { get; set; } = null!;
}

public class ProductColorEntity
{
    // Composite key: ProductId + Color
    public Guid ProductId { get; set; }
    public string Color { get; set; } = null!; // hex or name

    public ProductEntity Product { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
}

public class OrderEntity
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public string ShippingAddress { get; set; } = null!;
    public string ShippingCity { get; set; } = null!;
    public string ShippingPostalCode { get; set; } = null!;
    // Order status (e.g. New, Processing, Shipped)
    public string Status { get; set; } = "New";
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<OrderItemEntity> Items { get; set; } = new();
}

public class OrderItemEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public OrderEntity? Order { get; set; }
}

public class ProductViewEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public DateTimeOffset ViewedAt { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
}
