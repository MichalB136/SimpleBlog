using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.ApiService.Data;
using SimpleBlog.Blog.Services;
using SimpleBlog.Shop.Services;

namespace SimpleBlog.ApiService.Configuration;

/// <summary>
/// Extension methods for database configuration
/// </summary>
public static class DatabaseExtensions
{
    public static void ConfigureDatabase(this WebApplicationBuilder builder, string connectionString)
    {
        // Log connection string for debugging (mask password)
        var maskedConnectionString = connectionString.Contains("Password=")
            ? System.Text.RegularExpressions.Regex.Replace(connectionString, @"Password=[^;]*", "Password=***")
            : connectionString;
        var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("DatabaseConfiguration");
        logger.LogInformation("Using connection string: {ConnectionString}", maskedConnectionString);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // Register DbContext aliases for repositories
        builder.Services.AddScoped<BlogDbContext>(sp => 
        {
            var options = new DbContextOptionsBuilder<BlogDbContext>()
                .UseNpgsql(connectionString);
            
            return new BlogDbContext(options.Options);
        });

        builder.Services.AddScoped<ShopDbContext>(sp => 
        {
            var options = new DbContextOptionsBuilder<ShopDbContext>()
                .UseNpgsql(connectionString);
            
            return new ShopDbContext(options.Options);
        });

        // Register repositories from service layers
        builder.Services.AddScoped<IPostRepository, EfPostRepository>();
        builder.Services.AddScoped<IAboutMeRepository, EfAboutMeRepository>();
        builder.Services.AddScoped<IProductRepository, EfProductRepository>();
        builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
        builder.Services.AddScoped<ISiteSettingsRepository, EfSiteSettingsRepository>();
        builder.Services.AddScoped<ITagRepository, EfTagRepository>();
    }

    public static async Task MigrateDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            logger.LogInformation("Applying database migrations...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully");
            
            // Ensure other DbContexts use the same database
            var blogDb = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
            await blogDb.Database.MigrateAsync();
            
            var shopDb = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            await shopDb.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying database migrations");
            throw;
        }
    }

    public static async Task SeedDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var blogDb = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
            var shopDb = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            
            await Seeding.IdentitySeeder.SeedAsync(app.Services, app.Configuration);
            await Seeding.BlogSeeder.SeedAsync(blogDb, logger);
            await Seeding.ShopSeeder.SeedAsync(shopDb, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}
