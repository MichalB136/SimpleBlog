using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Utilities;

/// <summary>
/// Helper class for common validation operations.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates a post request.
    /// </summary>
    /// <param name="request">The post request to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void ValidatePostRequest(CreatePostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Post title cannot be empty.", nameof(request.Title));

        if (request.Title.Length > 200)
            throw new ArgumentException("Post title cannot exceed 200 characters.", nameof(request.Title));

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Post content cannot be empty.", nameof(request.Content));

        if (request.Content.Length > 10000)
            throw new ArgumentException("Post content cannot exceed 10000 characters.", nameof(request.Content));
    }

    /// <summary>
    /// Validates a comment request.
    /// </summary>
    /// <param name="request">The comment request to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void ValidateCommentRequest(CreateCommentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Comment content cannot be empty.", nameof(request.Content));

        if (request.Content.Length > 1000)
            throw new ArgumentException("Comment content cannot exceed 1000 characters.", nameof(request.Content));

        if (request.Author != null && request.Author.Length > 100)
            throw new ArgumentException("Author name cannot exceed 100 characters.", nameof(request.Author));
    }

    /// <summary>
    /// Validates a product request.
    /// </summary>
    /// <param name="request">The product request to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void ValidateProductRequest(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Product name cannot be empty.", nameof(request.Name));

        if (request.Name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters.", nameof(request.Name));

        if (request.Price <= 0)
            throw new ArgumentException("Product price must be greater than zero.", nameof(request.Price));

        if (request.Stock < 0)
            throw new ArgumentException("Product stock cannot be negative.", nameof(request.Stock));

        if (string.IsNullOrWhiteSpace(request.Category))
            throw new ArgumentException("Product category cannot be empty.", nameof(request.Category));
    }

    /// <summary>
    /// Validates an order request.
    /// </summary>
    /// <param name="request">The order request to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void ValidateOrderRequest(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new ArgumentException("Customer name cannot be empty.", nameof(request.CustomerName));

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            throw new ArgumentException("Customer email cannot be empty.", nameof(request.CustomerEmail));

        if (!IsValidEmail(request.CustomerEmail))
            throw new ArgumentException("Customer email is invalid.", nameof(request.CustomerEmail));

        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            throw new ArgumentException("Shipping address cannot be empty.", nameof(request.ShippingAddress));

        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("Order must contain at least one item.", nameof(request.Items));

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException("Item quantity must be greater than zero.", nameof(item.Quantity));
        }
    }

    /// <summary>
    /// Validates login credentials.
    /// </summary>
    /// <param name="request">The login request to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void ValidateLoginRequest(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ArgumentException("Username cannot be empty.", nameof(request.Username));

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password cannot be empty.", nameof(request.Password));

        if (request.Password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long.", nameof(request.Password));
    }

    /// <summary>
    /// Validates email format using MailAddress class.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch (System.FormatException)
        {
            // Expected for invalid email formats
            return false;
        }
        catch (Exception ex)
        {
            // Log unexpected exceptions to aid debugging while preserving the boolean contract
            System.Console.Error.WriteLine(
                $"[{nameof(ValidationHelper)}] Unexpected exception in {nameof(IsValidEmail)}: {ex.GetType().FullName}");
            return false;
        }
    }
}
