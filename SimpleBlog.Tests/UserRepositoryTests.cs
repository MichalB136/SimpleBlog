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

        // Simple in-memory refresh token store for tests
        private readonly Dictionary<string, (string Username, DateTime ExpiresUtc, bool Revoked)> _refreshTokens = new();

        public Task SaveRefreshTokenAsync(string username, string refreshToken, DateTime expiresUtc)
        {
            _refreshTokens[refreshToken] = (username, expiresUtc, false);
            return Task.CompletedTask;
        }

        public Task<string?> GetUsernameByRefreshTokenAsync(string refreshToken)
        {
            if (_refreshTokens.TryGetValue(refreshToken, out var info))
            {
                if (!info.Revoked && info.ExpiresUtc > DateTime.UtcNow)
                    return Task.FromResult<string?>(info.Username);
            }
            return Task.FromResult<string?>(null);
        }

        public Task RevokeRefreshTokenAsync(string refreshToken)
        {
            if (_refreshTokens.TryGetValue(refreshToken, out var info))
            {
                _refreshTokens[refreshToken] = (info.Username, info.ExpiresUtc, true);
            }
            return Task.CompletedTask;
        }

        public Task<User?> GetUserByUsernameAsync(string username)
        {
            if (_users.TryGetValue(username, out var info))
            {
                return Task.FromResult<User?>(new User(username, string.Empty, info.Role));
            }
            return Task.FromResult<User?>(null);
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

    [Fact]
    public async Task RegisterAsync_ValidNewUser_Succeeds()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var (success, errorMessage) = await repository.RegisterAsync("newuser", "newuser@example.com", "NewPass123!");

        // Assert
        Assert.True(success);
        Assert.Null(errorMessage);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsFalse()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var (success, errorMessage) = await repository.RegisterAsync("admin", "newemail@example.com", "NewPass123!");

        // Assert
        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Contains("Username already exists", errorMessage);
    }

    [Fact]
    public async Task RegisterAsync_NewUserCanLoginAfterRegistration()
    {
        // Arrange
        var repository = new TestUserRepository();
        var newUsername = "freshuser";
        var newPassword = "FreshPass123!";

        // Act
        var registerResult = await repository.RegisterAsync(newUsername, "fresh@example.com", newPassword);
        var loginResult = await repository.ValidateUserAsync(newUsername, newPassword);

        // Assert
        Assert.True(registerResult.Success);
        Assert.NotNull(loginResult);
        Assert.Equal(newUsername, loginResult.Username);
        Assert.Equal("User", loginResult.Role);
    }

    [Fact]
    public async Task RegisterAsync_NewUserHasCorrectRole()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var (success, _) = await repository.RegisterAsync("newuser", "new@example.com", "NewPass123!");
        var user = await repository.ValidateUserAsync("newuser", "NewPass123!");

        // Assert
        Assert.True(success);
        Assert.NotNull(user);
        Assert.Equal("User", user.Role);
    }

    [Fact]
    public async Task RegisterAsync_MultipleNewUsers_EachRegistersSuccessfully()
    {
        // Arrange
        var repository = new TestUserRepository();

        // Act
        var result1 = await repository.RegisterAsync("user1", "user1@example.com", "Pass123!");
        var result2 = await repository.RegisterAsync("user2", "user2@example.com", "Pass123!");
        var result3 = await repository.RegisterAsync("user3", "user3@example.com", "Pass123!");
        var login1 = await repository.ValidateUserAsync("user1", "Pass123!");
        var login2 = await repository.ValidateUserAsync("user2", "Pass123!");
        var login3 = await repository.ValidateUserAsync("user3", "Pass123!");

        // Assert
        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.True(result3.Success);
        Assert.Null(result1.ErrorMessage);
        Assert.Null(result2.ErrorMessage);
        Assert.Null(result3.ErrorMessage);
        Assert.NotNull(login1);
        Assert.NotNull(login2);
        Assert.NotNull(login3);
    }
}
