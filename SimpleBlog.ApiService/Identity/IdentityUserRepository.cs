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
}
