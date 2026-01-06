using Microsoft.Extensions.Logging;
using SimpleBlog.ApiService.Data;
using SimpleBlog.Blog.Services;
using SimpleBlog.Shop.Services;

namespace SimpleBlog.ApiService;

/// <summary>
/// Backward-compatible wrapper delegating seeding to module seeders.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext _unusedAppDb,
        BlogDbContext blogDb,
        ShopDbContext shopDb,
        ILogger logger)
    {
        logger.LogInformation("Starting database seeding (wrapper)...");
        await Seeding.BlogSeeder.SeedAsync(blogDb, logger);
        await Seeding.ShopSeeder.SeedAsync(shopDb, logger);
        logger.LogInformation("Database seeding completed (wrapper)");
    }
}
