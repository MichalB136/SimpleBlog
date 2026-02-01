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

    // Simple in-memory refresh token store: token -> (username, expiresUtc, revoked)
    private readonly Dictionary<string, (string Username, DateTime ExpiresUtc, bool Revoked)> _refreshTokens = new();

    public Task SaveRefreshTokenAsync(string username, string refreshToken, DateTime expiresUtc)
    {
        _refreshTokens[refreshToken] = (username.ToLower(), expiresUtc, false);
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
        if (_users.TryGetValue(username.ToLower(), out var info))
        {
            return Task.FromResult<User?>(new User(username, info.Email, info.Role));
        }
        return Task.FromResult<User?>(null);
    }

    // In-memory store for password reset tokens: token -> (email, expiresUtc)
    private readonly Dictionary<string, (string Email, DateTime ExpiresUtc)> _passwordResetTokens = new();

    // In-memory store for email confirmation tokens: token -> (email, expiresUtc)
    private readonly Dictionary<string, (string Email, DateTime ExpiresUtc)> _emailConfirmationTokens = new();

    public Task<(string? Token, string? Error)> GeneratePasswordResetTokenAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult<(string?, string?)>((null, "Email is required"));
        }

        // Check if user exists with this email
        var userExists = _users.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (!userExists)
        {
            // Return null without error for security (prevent email enumeration)
            return Task.FromResult<(string?, string?)>((null, null));
        }

        // Generate token
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        _passwordResetTokens[token] = (email, DateTime.UtcNow.AddHours(3));
        
        return Task.FromResult<(string?, string?)>((token, null));
    }

    public Task<(bool Success, string? Error)> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
        {
            return Task.FromResult<(bool, string?)>((false, "Invalid reset token"));
        }

        if (!_passwordResetTokens.TryGetValue(token, out var tokenInfo))
        {
            return Task.FromResult<(bool, string?)>((false, "Invalid or expired reset token"));
        }

        if (tokenInfo.ExpiresUtc < DateTime.UtcNow)
        {
            _passwordResetTokens.Remove(token);
            return Task.FromResult<(bool, string?)>((false, "Invalid or expired reset token"));
        }

        // Find user by email and update password
        var userKey = _users.Keys.FirstOrDefault(k => 
            _users[k].Email.Equals(tokenInfo.Email, StringComparison.OrdinalIgnoreCase));
        
        if (userKey == null)
        {
            return Task.FromResult<(bool, string?)>((false, "Invalid or expired reset token"));
        }

        var userInfo = _users[userKey];
        _users[userKey] = (newPassword, userInfo.Role, userInfo.Email);
        _passwordResetTokens.Remove(token);

        return Task.FromResult<(bool, string?)>((true, null));
    }

    public Task<(string? Token, string? Error)> GenerateEmailConfirmationTokenAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult<(string?, string?)>((null, "Email is required"));
        }

        // Check if user exists with this email
        var userExists = _users.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (!userExists)
        {
            // Return null without error for security (prevent email enumeration)
            return Task.FromResult<(string?, string?)>((null, null));
        }

        // Generate token
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        _emailConfirmationTokens[token] = (email, DateTime.UtcNow.AddHours(3));
        
        return Task.FromResult<(string?, string?)>((token, null));
    }

    public Task<(bool Success, string? Error)> ConfirmEmailAsync(string userId, string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult<(bool, string?)>((false, "Invalid confirmation token"));
        }

        if (!_emailConfirmationTokens.TryGetValue(token, out var tokenInfo))
        {
            return Task.FromResult<(bool, string?)>((false, "Invalid or expired confirmation token"));
        }

        if (tokenInfo.ExpiresUtc < DateTime.UtcNow)
        {
            _emailConfirmationTokens.Remove(token);
            return Task.FromResult<(bool, string?)>((false, "Invalid or expired confirmation token"));
        }

        _emailConfirmationTokens.Remove(token);
        // In-memory repo: just mark as success (no persistent email confirmed flag)
        return Task.FromResult<(bool, string?)>((true, null));
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
