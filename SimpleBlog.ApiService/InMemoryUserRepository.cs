namespace SimpleBlog.ApiService;

internal sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, (string Password, string Role)> _users = new()
    {
        [SeedDataConstants.AdminUsername.ToLower()] = ("admin123", SeedDataConstants.AdminUsername),
        ["user"] = ("user123", "User")
    };

    public Task<User?> ValidateUserAsync(string username, string password)
    {
        if (_users.TryGetValue(username.ToLower(), out var userInfo) && ConstantTimeEquals(userInfo.Password, password))
        {
            return Task.FromResult<User?>(new User(username, string.Empty, userInfo.Role));
        }
        return Task.FromResult<User?>(null);
    }

    public Task<(bool Success, string? ErrorMessage)> RegisterAsync(string username, string email, string password)
    {
        if (_users.ContainsKey(username.ToLower()))
        {
            return Task.FromResult<(bool Success, string? ErrorMessage)>((false, "Username already exists"));
        }

        _users[username.ToLower()] = (password, "User");
        return Task.FromResult<(bool Success, string? ErrorMessage)>((true, null));
    }

    /// <summary>
    /// Constant-time string comparison to prevent timing attacks on password validation.
    /// </summary>
    private static bool ConstantTimeEquals(string left, string right)
    {
        if (left is null || right is null)
        {
            return false;
        }

        if (left.Length != right.Length)
        {
            return false;
        }

        var leftBytes = System.Text.Encoding.UTF8.GetBytes(left);
        var rightBytes = System.Text.Encoding.UTF8.GetBytes(right);

        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
