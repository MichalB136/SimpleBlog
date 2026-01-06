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

        public Task<User?> ValidateUserAsync(string username, string password)
        {
            if (_users.TryGetValue(username, out var userInfo) && userInfo.Password == password)
            {
                return Task.FromResult<User?>(new User(username, string.Empty, userInfo.Role));
            }
            return Task.FromResult<User?>(null);
        }

        public Task<(bool Success, string? ErrorMessage)> RegisterAsync(string username, string email, string password)
        {
            if (_users.ContainsKey(username))
            {
                return Task.FromResult<(bool Success, string? ErrorMessage)>((false, "Username already exists"));
            }
            _users[username] = (password, "User");
            return Task.FromResult<(bool Success, string? ErrorMessage)>((true, null));
        }
    }

    [Fact]
    public async Task ValidateUser_CorrectCredentials_ReturnsUser()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = await repository.ValidateUserAsync("admin", "admin123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin", result.Username);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task ValidateUser_IncorrectPassword_ReturnsNull()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = await repository.ValidateUserAsync("admin", "wrongpassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateUser_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = await repository.ValidateUserAsync("nonexistent", "password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateUser_AdminRole_HasCorrectRole()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = await repository.ValidateUserAsync("admin", "admin123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task ValidateUser_UserRole_HasCorrectRole()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = await repository.ValidateUserAsync("user", "user123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("User", result.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateUser_EmptyUsername_ReturnsNull(string username)
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = await repository.ValidateUserAsync(username, "password");

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateUser_EmptyPassword_ReturnsNull(string password)
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result = await repository.ValidateUserAsync("admin", password);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateUser_CaseSensitiveUsername_ReturnsNull()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var resultUpperCase = await repository.ValidateUserAsync("ADMIN", "admin123");
        var resultMixedCase = await repository.ValidateUserAsync("Admin", "admin123");

        // Assert
        Assert.Null(resultUpperCase);
        Assert.Null(resultMixedCase);
        // Username is case-sensitive - this is by design
    }

    [Fact]
    public async Task ValidateUser_MultipleUsers_EachHasUniqueCredentials()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var admin = await repository.ValidateUserAsync("admin", "admin123");
        var user = await repository.ValidateUserAsync("user", "user123");
        var testUser = await repository.ValidateUserAsync("testuser", "testpass");

        // Assert
        Assert.NotNull(admin);
        Assert.NotNull(user);
        Assert.NotNull(testUser);
        Assert.NotEqual(admin.Role, user.Role);
    }
}
