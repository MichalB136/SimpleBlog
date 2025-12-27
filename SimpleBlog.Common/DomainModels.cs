namespace SimpleBlog.Common;

// Domain models
public record Post(
    Guid Id,
    string Title,
    string Content,
    string Author,
    DateTimeOffset CreatedAt,
    IReadOnlyList<Comment> Comments,
    string? ImageUrl
);

public record Comment(
    Guid Id,
    Guid PostId,
    string Author,
    string Content,
    DateTimeOffset CreatedAt
);

public record Product(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    string Category,
    int Stock,
    DateTimeOffset CreatedAt
);

public record Order(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string ShippingAddress,
    string ShippingCity,
    string ShippingPostalCode,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItem> Items
);

public record OrderItem(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity
);
