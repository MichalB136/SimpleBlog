using Microsoft.EntityFrameworkCore;
using SimpleBlog.Shop.Services;
using SimpleBlog.Common;

namespace SimpleBlog.Tests;

public sealed class ShopAnalyticsTests
{
    private static ShopDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ShopDbContext(options);
    }

    [Fact]
    public async Task GetOrdersSummaryAsync_ComputesTotalsAndAverage()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context, new NoOpOperationLogger());

        var o1 = new OrderEntity {
            Id = Guid.NewGuid(),
            CustomerName = "Test Customer",
            CustomerEmail = "test1@example.com",
            CustomerPhone = "000-000-000",
            ShippingAddress = "Addr 1",
            ShippingCity = "City",
            ShippingPostalCode = "00-001",
            TotalAmount = 100m,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
            Status = "Completed"
        };
        var o2 = new OrderEntity {
            Id = Guid.NewGuid(),
            CustomerName = "Test Customer",
            CustomerEmail = "test2@example.com",
            CustomerPhone = "000-000-001",
            ShippingAddress = "Addr 2",
            ShippingCity = "City",
            ShippingPostalCode = "00-002",
            TotalAmount = 200m,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            Status = "Completed"
        };
        var o3 = new OrderEntity {
            Id = Guid.NewGuid(),
            CustomerName = "Test Customer",
            CustomerEmail = "test3@example.com",
            CustomerPhone = "000-000-002",
            ShippingAddress = "Addr 3",
            ShippingCity = "City",
            ShippingPostalCode = "00-003",
            TotalAmount = 50m,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = "New"
        };

        context.Orders.AddRange(o1, o2, o3);
        context.SaveChanges();

        var summary = await repository.GetOrdersSummaryAsync();

        Assert.Equal(3, summary.TotalOrders);
        Assert.Equal(350m, summary.TotalRevenue);
        Assert.Equal(decimal.Round(350m / 3m, 2), summary.AverageOrderValue);
    }

    [Fact]
    public async Task GetSalesByDayAsync_GroupsByDate()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context, new NoOpOperationLogger());

        var day1 = DateTimeOffset.UtcNow.Date.AddDays(-2);
        var day2 = DateTimeOffset.UtcNow.Date.AddDays(-1);

        var o1 = new OrderEntity {
            Id = Guid.NewGuid(),
            CustomerName = "Test",
            CustomerEmail = "a@example.com",
            CustomerPhone = "111",
            ShippingAddress = "Addr",
            ShippingCity = "City",
            ShippingPostalCode = "00-010",
            TotalAmount = 10m,
            CreatedAt = day1.AddHours(1),
            Status = "Completed"
        };
        var o2 = new OrderEntity {
            Id = Guid.NewGuid(),
            CustomerName = "Test",
            CustomerEmail = "b@example.com",
            CustomerPhone = "222",
            ShippingAddress = "Addr",
            ShippingCity = "City",
            ShippingPostalCode = "00-011",
            TotalAmount = 20m,
            CreatedAt = day1.AddHours(5),
            Status = "Completed"
        };
        var o3 = new OrderEntity {
            Id = Guid.NewGuid(),
            CustomerName = "Test",
            CustomerEmail = "c@example.com",
            CustomerPhone = "333",
            ShippingAddress = "Addr",
            ShippingCity = "City",
            ShippingPostalCode = "00-012",
            TotalAmount = 30m,
            CreatedAt = day2.AddHours(2),
            Status = "Completed"
        };

        context.Orders.AddRange(o1, o2, o3);
        context.SaveChanges();

        var data = await repository.GetSalesByDayAsync(limit: 10);

        // Expect two days
        Assert.Equal(2, data.Count);

        var first = data.First(); // earliest by date
        Assert.Equal(day1.Date, first.Date.Date);
        Assert.Equal(2, first.OrdersCount);
        Assert.Equal(30m, first.Revenue);

        var second = data.Last();
        Assert.Equal(day2.Date, second.Date.Date);
        Assert.Equal(1, second.OrdersCount);
        Assert.Equal(30m, second.Revenue);
    }

    [Fact]
    public async Task GetOrderStatusCountsAsync_ReturnsCountsPerStatus()
    {
        await using var context = CreateInMemoryContext();
        var repository = new EfOrderRepository(context, new NoOpOperationLogger());

        var o1 = new OrderEntity {
            Id = Guid.NewGuid(),
            CustomerName = "User A",
            CustomerEmail = "ua@example.com",
            CustomerPhone = "101",
            ShippingAddress = "Addr",
            ShippingCity = "City",
            ShippingPostalCode = "00-020",
            TotalAmount = 10m,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = "New"
        };
        var o2 = new OrderEntity {
            Id = Guid.NewGuid(),
            CustomerName = "User B",
            CustomerEmail = "ub@example.com",
            CustomerPhone = "102",
            ShippingAddress = "Addr",
            ShippingCity = "City",
            ShippingPostalCode = "00-021",
            TotalAmount = 20m,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = "Processing"
        };
        var o3 = new OrderEntity {
            Id = Guid.NewGuid(),
            CustomerName = "User C",
            CustomerEmail = "uc@example.com",
            CustomerPhone = "103",
            ShippingAddress = "Addr",
            ShippingCity = "City",
            ShippingPostalCode = "00-022",
            TotalAmount = 30m,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = "New"
        };

        context.Orders.AddRange(o1, o2, o3);
        context.SaveChanges();

        var counts = await repository.GetOrderStatusCountsAsync();

        Assert.Equal(2, counts.Count);
        var newCount = counts.FirstOrDefault(c => c.Status == "New");
        var procCount = counts.FirstOrDefault(c => c.Status == "Processing");

        Assert.NotNull(newCount);
        Assert.Equal(2, newCount.Count);
        Assert.NotNull(procCount);
        Assert.Equal(1, procCount.Count);
    }
}
