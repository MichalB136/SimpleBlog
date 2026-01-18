using Microsoft.EntityFrameworkCore;
using SimpleBlog.Blog.Services;
using SimpleBlog.ApiService;

namespace SimpleBlog.ApiService.Seeding;

public static class BlogSeeder
{
    public static async Task SeedAsync(BlogDbContext db, ILogger logger)
    {
        logger.LogInformation("Seeding blog data...");

        // Seed posts if none exist
        if (!await db.Posts.AnyAsync())
        {
            var posts = new[]
        {
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Witaj w SimpleBlog!",
                Content = "To jest pierwszy post na naszym blogu. SimpleBlog to przykładowa aplikacja demonstrująca możliwości .NET Aspire z PostgreSQL.",
                    Author = SeedDataConstants.AdminUsername,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
                ImageUrls = "[]",
                Comments = new List<CommentEntity>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Author = "user",
                        Content = "Świetny start! Czekam na więcej postów.",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-9)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Author = SeedDataConstants.AdminUsername,
                        Content = "Dziękuję! Planujemy regularne publikacje.",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-8)
                    }
                }
            },
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Wprowadzenie do .NET Aspire",
                Content = ".NET Aspire to framework do budowania aplikacji rozproszonych. Oferuje gotowe komponenty do orkiestracji, telemetrii i zarządzania zasobami.",
                    Author = SeedDataConstants.AdminUsername,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-7),
                ImageUrls = "[]",
                Comments = new List<CommentEntity>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Author = "user",
                        Content = "Czy Aspire wspiera PostgreSQL?",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-6)
                    }
                }
            },
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "PostgreSQL w praktyce",
                Content = "PostgreSQL to potężny otwartoźródłowy system baz danych. W SimpleBlog używamy go do przechowywania postów, komentarzy i danych sklepu.",
                    Author = SeedDataConstants.AdminUsername,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                ImageUrls = "[]",
                Comments = new List<CommentEntity>()
            },
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Entity Framework Core Migrations",
                Content = "EF Core Migrations pozwala na automatyczne zarządzanie schematem bazy danych. Nasza aplikacja automatycznie aplikuje migracje przy starcie.",
                Author = "admin",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
                ImageUrls = "[]",
                Comments = new List<CommentEntity>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Author = "user",
                        Content = "Jak tworzyć nową migrację?",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Author = SeedDataConstants.AdminUsername,
                        Content = "Użyj: dotnet ef migrations add NazwaMigracji",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
                    }
                }
            },
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Docker i docker-compose",
                Content = "Docker pozwala na łatwe uruchamianie PostgreSQL lokalnie. SimpleBlog używa docker-compose do zarządzania infrastrukturą.",
                    Author = SeedDataConstants.AdminUsername,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                ImageUrls = "[]",
                Comments = new List<CommentEntity>()
            }
        };

            await db.Posts.AddRangeAsync(posts);
            await db.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} blog posts with comments", posts.Length);
        }

        // Seed AboutMe if missing
        if (!await db.AboutMe.AnyAsync())
        {
            var about = new AboutMeEntity
            {
                Id = Guid.NewGuid(),
                Content = "Witaj na moim blogu! Jestem pasjonatem programowania i chętnie dzielę się wiedzą. Ten blog powstał w oparciu o .NET 9, Aspire oraz React.",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = SeedDataConstants.SystemUsername
            };

            await db.AboutMe.AddAsync(about);
            await db.SaveChangesAsync();

            logger.LogInformation("Seeded AboutMe content");
        }

        // Seed SiteSettings if missing
        if (!await db.SiteSettings.AnyAsync())
        {
            var settings = new SiteSettingsEntity
            {
                Id = Guid.NewGuid(),
                Theme = "light",
                LogoUrl = null,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = SeedDataConstants.SystemUsername
            };

            await db.SiteSettings.AddAsync(settings);
            await db.SaveChangesAsync();

            logger.LogInformation("Seeded SiteSettings with default theme");
        }
    }
}
