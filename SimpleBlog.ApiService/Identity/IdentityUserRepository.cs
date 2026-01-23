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
            _logger.LogWarning("Failed password check for user {Username}", username);
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

        var createResult = await _userManager.CreateAsync(newUser, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user {Username}: {Errors}", username, errors);
            return (false, errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign user role to {Username}", username);
            return (false, "Failed to assign default role");
        }

        _logger.LogInformation("User {Username} registered successfully", username);
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
}
