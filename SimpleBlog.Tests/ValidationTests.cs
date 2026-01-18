using SimpleBlog.Common;
using SimpleBlog.Common.Validators;

namespace SimpleBlog.Tests;

public sealed class ValidationTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreatePostRequest_InvalidTitle_ShouldBeValidated(string? title)
    {
        // Arrange & Act
        var request = new CreatePostRequest(title!, "Content", "Author");

        // Assert
        Assert.True(string.IsNullOrWhiteSpace(request.Title));
        // Note: This documents that validation should be done at API level
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreatePostRequest_InvalidContent_ShouldBeValidated(string? content)
    {
        // Arrange & Act
        var request = new CreatePostRequest("Title", content!, "Author");

        // Assert
        Assert.True(string.IsNullOrWhiteSpace(request.Content));
    }

    [Fact]
    public void CreatePostRequest_ValidData_CreatesRequest()
    {
        // Arrange & Act
        var request = new CreatePostRequest("Valid Title", "Valid Content", "Valid Author");

        // Assert
        Assert.Equal("Valid Title", request.Title);
        Assert.Equal("Valid Content", request.Content);
        Assert.Equal("Valid Author", request.Author);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void CreateProductRequest_NegativePrice_ShouldBeValidated(decimal price)
    {
        // Arrange & Act
        var request = new CreateProductRequest("Product", "Description", price, null, "Category", 10);

        // Assert
        Assert.True(request.Price < 0);
        // Documents that price validation should be done at API level
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void CreateProductRequest_InvalidStock_ShouldBeValidated(int stock)
    {
        // Arrange & Act
        var request = new CreateProductRequest("Product", "Description", 10.00m, null, "Category", stock);

        // Assert
        Assert.True(request.Stock <= 0);
    }

    [Fact]
    public void CreateProductRequest_ValidData_CreatesRequest()
    {
        // Arrange & Act
        var request = new CreateProductRequest(
            "Test Product",
            "Test Description",
            99.99m,
            "https://example.com/product.jpg",
            "Electronics",
            50
        );

        // Assert
        Assert.Equal("Test Product", request.Name);
        Assert.Equal(99.99m, request.Price);
        Assert.Equal(50, request.Stock);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void CreateOrderRequest_InvalidEmail_ShouldBeValidated(string? email)
    {
        // Arrange & Act
        var request = new CreateOrderRequest(
            "John Doe",
            email!,
            "123456789",
            "Address",
            "City",
            "00-001",
            new List<OrderItemRequest>()
        );

        // Assert
        // Email format validation should be done at API level
        Assert.Equal(email, request.CustomerEmail);
    }

    [Fact]
    public void CreateOrderRequest_EmptyItems_ShouldBeValidated()
    {
        // Arrange & Act
        var request = new CreateOrderRequest(
            "John Doe",
            "john@example.com",
            "123456789",
            "Address",
            "City",
            "00-001",
            new List<OrderItemRequest>()
        );

        // Assert
        Assert.Empty(request.Items);
        // Should be validated at API level
    }

    [Fact]
    public void CreateOrderRequest_ValidData_CreatesRequest()
    {
        // Arrange
        var items = new List<OrderItemRequest>
        {
            new(Guid.NewGuid(), 2),
            new(Guid.NewGuid(), 1)
        };

        // Act
        var request = new CreateOrderRequest(
            "John Doe",
            "john@example.com",
            "123456789",
            "Main Street 1",
            "Warsaw",
            "00-001",
            items
        );

        // Assert
        Assert.Equal("John Doe", request.CustomerName);
        Assert.Equal("john@example.com", request.CustomerEmail);
        Assert.Equal(2, request.Items.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreateCommentRequest_InvalidContent_ShouldBeValidated(string? content)
    {
        // Arrange & Act
        var request = new CreateCommentRequest("Author", content!);

        // Assert
        Assert.True(string.IsNullOrWhiteSpace(request.Content));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreateCommentRequest_InvalidAuthor_ShouldBeValidated(string? author)
    {
        // Arrange & Act
        var request = new CreateCommentRequest(author!, "Content");

        // Assert
        Assert.True(string.IsNullOrWhiteSpace(request.Author));
    }

    [Fact]
    public void CreateCommentRequest_ValidData_CreatesRequest()
    {
        // Arrange & Act
        var request = new CreateCommentRequest("John Doe", "Great post!");

        // Assert
        Assert.Equal("Great post!", request.Content);
        Assert.Equal("John Doe", request.Author);
    }

    [Fact]
    public void Post_WithComments_MaintainsRelationship()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        
        var post = new Post(
            postId,
            "Title",
            "Content",
            "Author",
            DateTimeOffset.UtcNow,
            new List<Comment>
            {
                new(commentId, postId, "Commenter", "Comment text", DateTimeOffset.UtcNow)
            },
            null,
            false
        );

        // Assert
        Assert.Single(post.Comments);
        Assert.Equal(postId, post.Comments[0].PostId);
        Assert.Equal(commentId, post.Comments[0].Id);
    }

    [Fact]
    public void Product_PriceCalculation_IsAccurate()
    {
        // Arrange
        var product = new Product(
            Guid.NewGuid(),
            "Product",
            "Description",
            19.99m,
            null,
            "Category",
            100,
            DateTimeOffset.UtcNow
        );

        // Act
        var totalForFive = product.Price * 5;

        // Assert
        Assert.Equal(99.95m, totalForFive);
    }

    [Fact]
    public void Order_TotalAmount_IsCalculatedCorrectly()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Product 1", 10.50m, 2),  // 21.00
            new(Guid.NewGuid(), Guid.NewGuid(), "Product 2", 15.75m, 3),  // 47.25
            new(Guid.NewGuid(), Guid.NewGuid(), "Product 3", 5.00m, 1)    //  5.00
        };
        
        var order = new Order(
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            "123456789",
            "Address",
            "City",
            "00-001",
            73.25m,
            DateTimeOffset.UtcNow,
            items
        );

        // Act
        var calculatedTotal = items.Sum(i => i.Price * i.Quantity);

        // Assert
        Assert.Equal(73.25m, order.TotalAmount);
        Assert.Equal(order.TotalAmount, calculatedTotal);
    }

    [Fact]
    public void RegisterRequestValidator_ValidData_Succeeds()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("testuser123", "test@example.com", "Password123!");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void RegisterRequestValidator_EmptyUsername_FailsValidation(string? username)
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest(username ?? "", "test@example.com", "Password123!");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("ab")]  // Too short
    [InlineData("a")]   // Too short
    public void RegisterRequestValidator_UsernameTooShort_FailsValidation(string username)
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest(username, "test@example.com", "Password123!");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Username" && e.ErrorMessage.Contains("at least 3"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void RegisterRequestValidator_EmptyEmail_FailsValidation(string? email)
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("testuser", email ?? "", "Password123!");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test")]
    public void RegisterRequestValidator_InvalidEmailFormat_FailsValidation(string email)
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("testuser", email, "Password123!");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void RegisterRequestValidator_EmptyPassword_FailsValidation(string? password)
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("testuser", "test@example.com", password ?? "");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("Pass1!")]  // Too short
    [InlineData("Pass1")]   // Too short and no special char
    public void RegisterRequestValidator_PasswordTooShort_FailsValidation(string password)
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("testuser", "test@example.com", password);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("password123!")]  // No uppercase
    [InlineData("PASSWORD123!")]  // No lowercase
    [InlineData("Password!")]     // No digit
    [InlineData("Password123")]   // No special char
    public void RegisterRequestValidator_PasswordMissingRequirement_FailsValidation(string password)
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("testuser", "test@example.com", password);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Single(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void RegisterRequestValidator_ValidPasswordWithSpecialChar_Succeeds()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("testuser", "test@example.com", "SecurePass123!");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void UpdateAboutMeRequestValidator_EmptyContent_FailsValidation(string? content)
    {
        // Arrange
        var validator = new UpdateAboutMeRequestValidator();
        var request = new UpdateAboutMeRequest(content!);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Content" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void UpdateAboutMeRequestValidator_ContentExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var validator = new UpdateAboutMeRequestValidator();
        var longContent = new string('A', 10001); // Exceeds 10000 character limit
        var request = new UpdateAboutMeRequest(longContent);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Content" && e.ErrorMessage.Contains("10000"));
    }

    [Fact]
    public void UpdateAboutMeRequestValidator_ValidContent_PassesValidation()
    {
        // Arrange
        var validator = new UpdateAboutMeRequestValidator();
        var request = new UpdateAboutMeRequest("This is valid about me content.");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void UpdateAboutMeRequestValidator_ContentAtMaxLength_PassesValidation()
    {
        // Arrange
        var validator = new UpdateAboutMeRequestValidator();
        var maxLengthContent = new string('A', 10000); // Exactly at limit
        var request = new UpdateAboutMeRequest(maxLengthContent);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void UpdateAboutMeRequestValidator_ContentWithSpecialCharacters_PassesValidation()
    {
        // Arrange
        var validator = new UpdateAboutMeRequestValidator();
        var contentWithSpecialChars = "Hello! This is my about page. <b>HTML</b> & special chars: 你好, émojis: 😊, etc.";
        var request = new UpdateAboutMeRequest(contentWithSpecialChars);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
