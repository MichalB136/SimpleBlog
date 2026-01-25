using Microsoft.EntityFrameworkCore;
using SimpleBlog.Shop.Services;
using static SimpleBlog.ApiService.SeedDataConstants;

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
                Name = "Sukienka lniana 'Letnia Rosa'",
                Description = "Zwiewna sukienka midi z naturalnego lnu. Idealna na letnie dni. Pastelowy róż podkreśla kobiecość. Rozmiar uniwersalny (S-M). Wymiary: długość 110cm, obwód w talii 70-85cm.",
                Price = 289.99m,
                Category = CategoryDresses,
                Stock = 3,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-15)
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Koszulka oversize 'Vintage Dreams'",
                Description = "100% bawełna organiczna, oversizowy krój. Ręcznie malowany nadruk w stylu vintage. Unisex. Dostępne rozmiary: M, L, XL.",
                Price = 129.99m,
                Category = CategoryTShirts,
                Stock = 8,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-14)
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Spodnie lniane 'Boho Wide'",
                Description = "Szerokie spodnie z lnu w stylu boho. Wysoki stan, gumka w pasie. Naturalna beżowa kolorystyka. Rozmiary: S, M, L.",
                Price = 249.99m,
                Category = CategoryPants,
                Stock = 5,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-13)
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Tunika bawełniana 'Minimalist'",
                Description = "Minimalistyczna tunika z organicznej bawełny. Luźny krój, idealna do letnich stylizacji. Biała, rozmiar uniwersalny.",
                Price = 179.99m,
                Category = CategoryTunics,
                Stock = 6,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Torba na ramię 'Eco Shopper'",
                Description = "Ręcznie szyta torba z bawełny organicznej. Wzmocnione uchwyty, podszewka z lnu. Wymiary: 40x35cm. Idealna na zakupy lub jako torba plażowa.",
                Price = 89.99m,
                Category = CategoryAccessories,
                Stock = 12,
                ImageUrl = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-7)
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Sukienka maxi 'Bohemian Soul'",
                Description = "Długa sukienka w stylu boho. Bawełna + len. Haftowane detale ręcznie wykonane. Pasek w talii. Rozmiary: S, M, L.",
                Price = 399.99m,
                Category = CategoryDresses,
                Stock = 2,
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
                Status = "Completed",
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
                Status = "Processing",
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
                Status = "New",
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
