namespace SimpleBlog.Common.Models;

// Domain models
public record Tag(
    Guid Id,
    string Name,
    string Slug,
    string? Color,
    DateTimeOffset CreatedAt
);

public record Post(
    Guid Id,
    string Title,
    string Content,
    string Author,
    DateTimeOffset CreatedAt,
    IReadOnlyList<Comment> Comments,
    IReadOnlyList<string> ImageUrls,
    bool IsPinned,
    IReadOnlyList<Tag> Tags
);

public record Comment(
    Guid Id,
    Guid PostId,
    string Author,
    string Content,
    DateTimeOffset CreatedAt
);

public record AboutMe(
    Guid Id,
    string Content,
    DateTimeOffset UpdatedAt,
    string UpdatedBy
);

public record Product(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    string Category,
    int Stock,
    DateTimeOffset CreatedAt,
    IReadOnlyList<Tag> Tags
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
