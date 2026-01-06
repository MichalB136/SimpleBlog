using Microsoft.EntityFrameworkCore;
using SimpleBlog.Shop.Services;

namespace SimpleBlog.ApiService.Seeding;

public static class ShopSeeder
{
    public static async Task SeedAsync(ShopDbContext db, ILogger logger)
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
