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
}
