namespace SimpleBlog.Common.Models;

// User model
public record User(string Username, string Email, string Role);

// Authentication
public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Email, string Password);
public record RegisterResponse(bool Success, string? Message);
// Refresh token requests
public record RefreshRequest(string RefreshToken);
public record RevokeRequest(string RefreshToken);

// Password reset and email confirmation
public record RequestPasswordResetRequest(string Email);
public record PasswordResetRequest(string UserId, string Token, string NewPassword);
public record ConfirmEmailRequest(string UserId, string Token);
public record SendEmailConfirmationRequest(string Email);
public record OperationResponse(bool Success, string? Message);

// Post DTOs
public record CreatePostRequest(
    string Title,
    string Content,
    string? Author
);

public record UpdatePostRequest(
    string? Title,
    string? Content,
    string? Author
);

public record CreateCommentRequest(
    string? Author,
    string? Content
);

// AboutMe DTOs
public record UpdateAboutMeRequest(
    string Content,
    string? ImageUrl
);

// Tag DTOs
public record CreateTagRequest(
    string Name,
    string? Color
);

public record UpdateTagRequest(
    string? Name,
    string? Color
);

public record AssignTagsRequest(
    List<Guid> TagIds
);

// Product DTOs
public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    string Category,
    int Stock
    ,
    List<string>? Colors = null
);

public record UpdateProductRequest(
    string? Name,
    string? Description,
    decimal? Price,
    string? ImageUrl,
    string? Category,
    int? Stock
    ,
    List<string>? Colors = null
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

// Filter DTOs
public record PostFilterRequest(
    List<Guid>? TagIds,
    string? SearchTerm
);

public record ProductFilterRequest(
    List<Guid>? TagIds,
    string? Category,
    string? SearchTerm
);
