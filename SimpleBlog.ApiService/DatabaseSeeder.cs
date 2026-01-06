using Microsoft.EntityFrameworkCore;
using SimpleBlog.ApiService.Data;
using SimpleBlog.Blog.Services;
using SimpleBlog.Shop.Services;

namespace SimpleBlog.ApiService;

/// <summary>
/// Seeds dummy data into the database for development and testing purposes.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds all database contexts with dummy data if they are empty.
    /// </summary>
    public static async Task SeedAsync(
        ApplicationDbContext appDb,
        BlogDbContext blogDb,
        ShopDbContext shopDb,
        ILogger logger)
    {
        try
        {
            logger.LogInformation("Starting database seeding...");

            await SeedBlogDataAsync(blogDb, logger);
            await SeedShopDataAsync(shopDb, logger);

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database");
            throw;
        }
    }

    private static async Task SeedBlogDataAsync(BlogDbContext db, ILogger logger)
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

    private static async Task SeedShopDataAsync(ShopDbContext db, ILogger logger)
    {
        if (await db.Products.AnyAsync())
        {
            logger.LogInformation("Shop data already exists, skipping seed");
            return;
        }

        logger.LogInformation("Seeding shop data...");

        var products = new[]
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Kurs .NET Aspire",
                Description = "Kompletny kurs video wprowadzający do .NET Aspire. Nauczysz się budować aplikacje rozproszone.",
                Price = 199.99m,
                Category = "Edukacja",
                Stock = 100,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-15)
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Książka: PostgreSQL dla programistów",
                Description = "Praktyczny przewodnik po PostgreSQL. Od podstaw do zaawansowanych technik optymalizacji.",
                Price = 89.99m,
                Category = "Książki",
                Stock = 50,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-14)
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Konsultacja techniczna - 1h",
                Description = "Godzinna konsultacja techniczna dotycząca architektury aplikacji, baz danych lub .NET.",
                Price = 250.00m,
                Category = "Usługi",
                Stock = 10,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-13)
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Docker dla developerów",
                Description = "Kurs online nauczający konteneryzacji aplikacji z użyciem Docker i docker-compose.",
                Price = 149.99m,
                Category = "Edukacja",
                Stock = 100,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Code Review - pakiet 3 sesji",
                Description = "Trzy sesje code review Twojego projektu z ekspertem. Analiza kodu, best practices, sugestie ulepszeń.",
                Price = 599.99m,
                Category = "Usługi",
                Stock = 5,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-7)
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Entity Framework Core - ebook",
                Description = "E-book w formacie PDF omawiający EF Core od podstaw. Zawiera praktyczne przykłady i wzorce.",
                Price = 49.99m,
                Category = "Książki",
                Stock = 1000,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            }
        };

        await db.Products.AddRangeAsync(products);

        var orders = new[]
        {
            new OrderEntity
            {
                Id = Guid.NewGuid(),
                CustomerName = "Jan Kowalski",
                CustomerEmail = "jan.kowalski@example.com",
                CustomerPhone = "+48 123 456 789",
                ShippingAddress = "ul. Kwiatowa 15",
                ShippingCity = "Warszawa",
                ShippingPostalCode = "00-001",
                TotalAmount = 199.99m,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-12),
                Items = new List<OrderItemEntity>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = products[0].Id,
                        ProductName = products[0].Name,
                        Price = products[0].Price,
                        Quantity = 1
                    }
                }
            },
            new OrderEntity
            {
                Id = Guid.NewGuid(),
                CustomerName = "Anna Nowak",
                CustomerEmail = "anna.nowak@example.com",
                CustomerPhone = "+48 987 654 321",
                ShippingAddress = "ul. Słoneczna 7/12",
                ShippingCity = "Kraków",
                ShippingPostalCode = "30-001",
                TotalAmount = 139.98m,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-8),
                Items = new List<OrderItemEntity>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = products[1].Id,
                        ProductName = products[1].Name,
                        Price = products[1].Price,
                        Quantity = 1
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = products[5].Id,
                        ProductName = products[5].Name,
                        Price = products[5].Price,
                        Quantity = 1
                    }
                }
            },
            new OrderEntity
            {
                Id = Guid.NewGuid(),
                CustomerName = "Piotr Wiśniewski",
                CustomerEmail = "piotr.wisniewski@example.com",
                CustomerPhone = "+48 555 123 456",
                ShippingAddress = "ul. Leśna 33",
                ShippingCity = "Gdańsk",
                ShippingPostalCode = "80-001",
                TotalAmount = 850.00m,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
                Items = new List<OrderItemEntity>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = products[2].Id,
                        ProductName = products[2].Name,
                        Price = products[2].Price,
                        Quantity = 2
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = products[3].Id,
                        ProductName = products[3].Name,
                        Price = products[3].Price,
                        Quantity = 1
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = products[1].Id,
                        ProductName = products[1].Name,
                        Price = products[1].Price,
                        Quantity = 2
                    }
                }
            }
        };

        await db.Orders.AddRangeAsync(orders);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded {ProductCount} products and {OrderCount} orders", products.Length, orders.Length);
    }
}
