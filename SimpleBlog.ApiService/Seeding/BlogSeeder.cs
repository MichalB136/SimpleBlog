using Microsoft.EntityFrameworkCore;
using SimpleBlog.Blog.Services;

namespace SimpleBlog.ApiService.Seeding;

public static class BlogSeeder
{
    public static async Task SeedAsync(BlogDbContext db, ILogger logger)
    {
        if (await db.Posts.AnyAsync())
        {
            logger.LogInformation("Blog data already exists, skipping seed");
            return;
        }

        logger.LogInformation("Seeding blog data...");

        var posts = new[]
        {
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Witaj w SimpleBlog!",
                Content = "To jest pierwszy post na naszym blogu. SimpleBlog to przykładowa aplikacja demonstrująca możliwości .NET Aspire z PostgreSQL.",
                Author = "admin",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
                ImageUrl = null,
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
                        Author = "admin",
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
                Author = "admin",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-7),
                ImageUrl = null,
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
                Author = "admin",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                ImageUrl = null,
                Comments = new List<CommentEntity>()
            },
            new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = "Entity Framework Core Migrations",
                Content = "EF Core Migrations pozwala na automatyczne zarządzanie schematem bazy danych. Nasza aplikacja automatycznie aplikuje migracje przy starcie.",
                Author = "admin",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
                ImageUrl = null,
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
                        Author = "admin",
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
                Author = "admin",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                ImageUrl = null,
                Comments = new List<CommentEntity>()
            }
        };

        await db.Posts.AddRangeAsync(posts);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} blog posts with comments", posts.Length);
    }
}
