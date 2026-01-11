namespace SimpleBlog.Common.Models;

// User model
public record User(string Username, string Email, string Role);

// Authentication
public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Email, string Password);
public record RegisterResponse(bool Success, string? Message);

// Post DTOs
public record CreatePostRequest(
    string Title,
    string Content,
    string? Author,
    string? ImageUrl
);

public record UpdatePostRequest(
    string? Title,
    string? Content,
    string? Author,
    string? ImageUrl
);

public record CreateCommentRequest(
    string? Author,
    string? Content
);

// AboutMe DTOs
public record UpdateAboutMeRequest(
    string Content
);

// Product DTOs
public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    string Category,
    int Stock
);

public record UpdateProductRequest(
    string? Name,
    string? Description,
    decimal? Price,
    string? ImageUrl,
    string? Category,
    int? Stock
);

// Order DTOs
public record CreateOrderRequest(
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string ShippingAddress,
    string ShippingCity,
    string ShippingPostalCode,
    List<OrderItemRequest> Items
);

public record OrderItemRequest(
    Guid ProductId,
    int Quantity
);
