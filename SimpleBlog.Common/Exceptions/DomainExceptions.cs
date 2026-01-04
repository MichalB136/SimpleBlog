namespace SimpleBlog.Common.Exceptions;

/// <summary>
/// Thrown when a post is not found.
/// </summary>
public class PostNotFoundException(Guid postId) : Exception($"Post with ID '{postId}' was not found.");

/// <summary>
/// Thrown when a product is not found.
/// </summary>
public class ProductNotFoundException(Guid productId) : Exception($"Product with ID '{productId}' was not found.");

/// <summary>
/// Thrown when an order is not found.
/// </summary>
public class OrderNotFoundException(Guid orderId) : Exception($"Order with ID '{orderId}' was not found.");

/// <summary>
/// Thrown when a comment is not found.
/// </summary>
public class CommentNotFoundException(Guid commentId) : Exception($"Comment with ID '{commentId}' was not found.");

/// <summary>
/// Thrown when there is insufficient stock for a product.
/// </summary>
public class InsufficientStockException(string productName, int requested, int available)
    : Exception($"Insufficient stock for '{productName}': requested {requested}, available {available}.");

/// <summary>
/// Thrown when user authentication fails.
/// </summary>
public class AuthenticationFailedException(string username) 
    : Exception($"Authentication failed for user '{username}'.");

/// <summary>
/// Thrown when validation fails for a model.
/// </summary>
public class ValidationException(string fieldName, string message) 
    : Exception($"Validation error on '{fieldName}': {message}");
