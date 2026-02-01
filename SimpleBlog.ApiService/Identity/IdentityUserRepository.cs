using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SimpleBlog.ApiService.Data;
using SimpleBlog.Common.Interfaces;
using SimpleBlog.Common.Models;

namespace SimpleBlog.ApiService.Identity;

internal sealed class IdentityUserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<IdentityUserRepository> _logger;
    private readonly ApplicationDbContext _db;

    public IdentityUserRepository(
        UserManager<ApplicationUser> userManager,
        ILogger<IdentityUserRepository> logger,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _logger = logger;
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user is null)
        {
            return null;
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            _logger.LogWarning("Failed password check for user {Username}", MaskUserName(username));
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";
        return new User(user.UserName ?? username, string.Empty, role);
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(string username, string email, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);

        var existingUser = await _userManager.FindByNameAsync(username);
        if (existingUser is not null)
        {
            return (false, "Username already exists");
        }

        var existingByEmail = await _userManager.FindByEmailAsync(email);
        if (existingByEmail is not null)
        {
            return (false, "Email already exists");
        }

        var newUser = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var maskedUsername = MaskUserName(username);
        var createResult = await _userManager.CreateAsync(newUser, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user {Username}: {Errors}", maskedUsername, errors);
            return (false, errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign user role to {Username}", maskedUsername);
            return (false, "Failed to assign default role");
        }

        _logger.LogInformation("User {Username} registered successfully", maskedUsername);
        return (true, null);
    }

    public async Task SaveRefreshTokenAsync(string username, string refreshToken, DateTime expiresUtc)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user is null) return;

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            ExpiresUtc = expiresUtc,
            CreatedUtc = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();
    }

    public Task<string?> GetUsernameByRefreshTokenAsync(string refreshToken)
    {
        var token = _db.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (token is null) return Task.FromResult<string?>(null);
        if (token.RevokedUtc is not null) return Task.FromResult<string?>(null);
        if (token.ExpiresUtc <= DateTime.UtcNow) return Task.FromResult<string?>(null);

        var user = _userManager.FindByIdAsync(token.UserId.ToString());
        if (user is null) return Task.FromResult<string?>(null);

        return user.ContinueWith(t => t.Result?.UserName, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = _db.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (token is null) return;
        token.RevokedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user is null) return null;
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";
        return new User(user.UserName ?? username, user.Email ?? string.Empty, role);
    }

    public async Task<(string? Token, string? Error)> GeneratePasswordResetTokenAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var maskedEmail = MaskEmail(email);
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            // Return neutral response for security (prevent email enumeration)
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", maskedEmail);
            return (null, null);
        }

        try
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation("Password reset token generated for user {UserId}", user.Id);
            return (token, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating password reset token for user {UserId}", user.Id);
            return (null, "An error occurred while generating the reset token");
        }
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(newPassword);

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("Password reset attempted for non-existent user: {UserId}", userId);
                return (false, "Invalid reset token");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to reset password for user {UserId}: {Errors}", userId, errors);
                return (false, "Invalid or expired reset token");
            }

            _logger.LogInformation("Password successfully reset for user {UserId}", userId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
            return (false, "An error occurred while resetting your password");
        }
    }

    public async Task<(string? Token, string? Error)> GenerateEmailConfirmationTokenAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        var maskedEmail = MaskEmail(email);
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning("Email confirmation token requested for non-existent email: {Email}", maskedEmail);
            return (null, null);
        }

        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            _logger.LogInformation("Email confirmation token generated for user {UserId}", user.Id);
            return (token, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating email confirmation token for user {UserId}", user.Id);
            return (null, "An error occurred while generating the confirmation token");
        }
    }

    public async Task<(bool Success, string? Error)> ConfirmEmailAsync(string userId, string token)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(token);

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("Email confirmation attempted for non-existent user: {UserId}", userId);
                return (false, "Invalid confirmation token");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to confirm email for user {UserId}: {Errors}", userId, errors);
                return (false, "Invalid or expired confirmation token");
            }

            _logger.LogInformation("Email successfully confirmed for user {UserId}", userId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user {UserId}", userId);
            return (false, "An error occurred while confirming your email");
        }
    }

    private static string MaskUserName(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "unknown";
        }

        return username.Length <= 2
            ? $"{username[0]}*"
            : $"{username[..1]}***";
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "unknown";
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            return "unknown";
        }

        var firstChar = email[..1];
        var domain = email[(atIndex + 1)..];
        return $"{firstChar}***@{domain}";
    }
}
