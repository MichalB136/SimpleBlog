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

    public IdentityUserRepository(
        UserManager<ApplicationUser> userManager,
        ILogger<IdentityUserRepository> logger)
    {
        _userManager = userManager;
        _logger = logger;
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
        var existingUser = await _userManager.FindByNameAsync(username);
        if (existingUser is not null)
        {
            return (false, "Username already exists");
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
}
