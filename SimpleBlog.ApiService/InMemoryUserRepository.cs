namespace SimpleBlog.ApiService;

/// <summary>
/// In-memory implementation of IUserRepository for development/demo purposes only.
/// </summary>
/// <remarks>
/// WARNING: This implementation stores passwords in plain text and is intended
/// for development and demonstration purposes only. Do NOT use this in production.
/// Instead, use a repository that securely hashes and stores passwords (e.g., IdentityUserRepository).
/// </remarks>
internal sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, (string Password, string Role, string Email)> _users = new()
    {
        [SeedDataConstants.AdminUsername.ToLower()] = ("admin123", SeedDataConstants.AdminUsername, "admin@example.com"),
        ["user"] = ("user123", "User", "user@example.com")
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

        _users[username.ToLower()] = (password, "User", email);
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
