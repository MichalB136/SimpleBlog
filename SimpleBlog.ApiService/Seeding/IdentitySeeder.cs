using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleBlog.ApiService.Data;

namespace SimpleBlog.ApiService.Seeding;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        const string adminRole = "Admin";
        const string userRole = "User";

        await EnsureRoleAsync(roleManager, adminRole, logger);
        await EnsureRoleAsync(roleManager, userRole, logger);

        var adminUsername = configuration["AdminSeed:Username"] ?? "admin";
        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@example.com";
        var adminPassword = configuration["AdminSeed:Password"] ?? "ChangeMe123!";

        await EnsureAdminAsync(userManager, adminUsername, adminEmail, adminPassword, adminRole, logger);

        // Seed mock users (development only)
        var mockUsers = configuration.GetSection("MockUsers").Get<List<MockUserConfig>>();
        if (mockUsers != null)
        {
            foreach (var mockUser in mockUsers)
            {
                await EnsureMockUserAsync(userManager, mockUser.Username, mockUser.Email, mockUser.Password, userRole, logger);
            }
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole<Guid>> roleManager, string roleName, ILogger logger)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to create role {Role}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task EnsureAdminAsync(
        UserManager<ApplicationUser> userManager,
        string username,
        string email,
        string password,
        string adminRole,
        ILogger logger)
    {
        var admin = await userManager.FindByNameAsync(username);
        if (admin is not null)
        {
            if (!await userManager.IsInRoleAsync(admin, adminRole))
            {
                await userManager.AddToRoleAsync(admin, adminRole);
            }
            return;
        }

        var newAdmin = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(newAdmin, password);
        if (!createResult.Succeeded)
        {
            logger.LogWarning("Failed to create admin user {Username}: {Errors}", username, string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(newAdmin, adminRole);
        if (!roleResult.Succeeded)
        {
            logger.LogWarning("Failed to assign admin role to {Username}: {Errors}", username, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }

    private static async Task EnsureMockUserAsync(
        UserManager<ApplicationUser> userManager,
        string username,
        string email,
        string password,
        string userRole,
        ILogger logger)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user is not null)
        {
            if (!await userManager.IsInRoleAsync(user, userRole))
            {
                await userManager.AddToRoleAsync(user, userRole);
            }
            return;
        }

        var newUser = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(newUser, password);
        if (!createResult.Succeeded)
        {
            logger.LogWarning("Failed to create mock user {Username}: {Errors}", username, string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(newUser, userRole);
        if (!roleResult.Succeeded)
        {
            logger.LogWarning("Failed to assign user role to {Username}: {Errors}", username, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }
}

/// <summary>
/// Configuration model for seeding mock user accounts.
/// </summary>
/// <remarks>
/// This type is intended for <c>Development</c> / local testing only.
/// Do <b>not</b> bind real or production credentials to this configuration,
/// and do not use it in production environments. For any non-trivial scenario,
/// prefer using ASP.NET Core user secrets or environment variables for seed passwords
/// rather than committing them to <c>appsettings*.json</c>.
/// </remarks>
internal sealed class MockUserConfig
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Plain-text password used to create the mock user in development.
    /// </summary>
    /// <remarks>
    /// This value is intended only for local development and test environments.
    /// Do not store sensitive or real user passwords here, and do not commit
    /// production secrets to configuration files. Use user secrets or environment
    /// variables if a seeded password is required.
    /// </remarks>
    public string Password { get; set; } = string.Empty;
}

