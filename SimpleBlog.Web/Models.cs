namespace SimpleBlog.Web;

public sealed record Post(
    Guid Id, 
    string Title, 
    string Content, 
    string Author, 
    DateTimeOffset CreatedAt, 
    IReadOnlyList<Comment> Comments, 
    string? ImageUrl = null);

public sealed record Comment(
    Guid Id, 
    Guid PostId, 
    string Author, 
    string Content, 
    DateTimeOffset CreatedAt);

public sealed record CreatePostRequest(
    string Title, 
    string Content, 
    string Author, 
    string? ImageUrl = null);

public sealed record UpdatePostRequest(
    string Title, 
    string Content, 
    string Author, 
    string? ImageUrl = null);

public sealed record CreateCommentRequest(
    string Content, 
    string Author);

public sealed record UpdateAboutMeRequest(
    string Content);

public sealed record LoginRequest(
    string Username, 
    string Password);

public sealed record Product(
    Guid Id, 
    string Name, 
    string Description, 
    decimal Price, 
    string? ImageUrl, 
    string Category, 
    int Stock, 
    DateTimeOffset CreatedAt);

public sealed record CreateProductRequest(
    string Name, 
    string Description, 
    decimal Price, 
    string? ImageUrl, 
    string Category, 
    int Stock);

public sealed record UpdateProductRequest(
    string Name, 
    string Description, 
    decimal Price, 
    string? ImageUrl, 
    string Category, 
    int Stock);

public sealed record Order(
    Guid Id, 
    string CustomerName, 
    string CustomerEmail, 
    string CustomerPhone, 
    string ShippingAddress, 
    string ShippingCity, 
    string ShippingPostalCode, 
    decimal TotalAmount, 
    DateTimeOffset CreatedAt, 
    IReadOnlyList<OrderItem> Items);

public sealed record OrderItem(
    Guid Id, 
    string ProductName, 
    decimal Price, 
    int Quantity);

public sealed record CreateOrderRequest(
    string CustomerName, 
    string CustomerEmail, 
    string CustomerPhone, 
    string ShippingAddress, 
    string ShippingCity, 
    string ShippingPostalCode, 
    List<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(
    Guid ProductId, 
    string ProductName, 
    decimal Price, 
    int Quantity);
