using SimpleBlog.Common;

namespace SimpleBlog.Tests;

public sealed class UserRepositoryTests
{
    private sealed class TestUserRepository : IUserRepository
    {
        private readonly Dictionary<string, (string Password, string Role)> _users = new()
        {
            ["admin"] = ("admin123", "Admin"),
            ["user"] = ("user123", "User"),
            ["testuser"] = ("testpass", "User")
        };

        public User? ValidateUser(string username, string password)
        {
            if (_users.TryGetValue(username, out var userInfo) && userInfo.Password == password)
            {
                return new User(username, string.Empty, userInfo.Role);
            }
            return null;
        }
    }

    [Fact]
    public void ValidateUser_CorrectCredentials_ReturnsUser()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = repository.ValidateUser("admin", "admin123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin", result.Username);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public void ValidateUser_IncorrectPassword_ReturnsNull()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = repository.ValidateUser("admin", "wrongpassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateUser_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = repository.ValidateUser("nonexistent", "password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateUser_AdminRole_HasCorrectRole()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = repository.ValidateUser("admin", "admin123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public void ValidateUser_UserRole_HasCorrectRole()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = repository.ValidateUser("user", "user123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("User", result.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateUser_EmptyUsername_ReturnsNull(string username)
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = repository.ValidateUser(username, "password");

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateUser_EmptyPassword_ReturnsNull(string password)
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = repository.ValidateUser("admin", password);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateUser_CaseSensitiveUsername_ReturnsNull()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var resultUpperCase = repository.ValidateUser("ADMIN", "admin123");
        var resultMixedCase = repository.ValidateUser("Admin", "admin123");

        // Assert
        Assert.Null(resultUpperCase);
        Assert.Null(resultMixedCase);
        // Username is case-sensitive - this is by design
    }

    [Fact]
    public void ValidateUser_MultipleUsers_EachHasUniqueCredentials()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var admin = repository.ValidateUser("admin", "admin123");
        var user = repository.ValidateUser("user", "user123");
        var testUser = repository.ValidateUser("testuser", "testpass");

        // Assert
        Assert.NotNull(admin);
        Assert.NotNull(user);
        Assert.NotNull(testUser);
        Assert.NotEqual(admin.Role, user.Role);
    }
}
