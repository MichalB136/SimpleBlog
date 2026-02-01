using Microsoft.EntityFrameworkCore;
using SimpleBlog.Common;
using SimpleBlog.Common.Extensions;
using SimpleBlog.Common.Logging;
using SimpleBlog.Common.Specifications;
using SimpleBlog.Shop.Services.Specifications;

namespace SimpleBlog.Shop.Services;

public sealed class EfOrderRepository(
    ShopDbContext context,
    IOperationLogger operationLogger) : IOrderRepository
{
    public async Task<PaginatedResult<Order>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetAllOrders",
            async () =>
            {
                var total = await context.Orders.CountAsync();
                var entities = await context.Orders
                    .Include(o => o.Items)
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Order>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new { page, pageSize });
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetOrderById",
            async () =>
            {
                var entity = await context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == id);
                return entity is not null ? MapToModel(entity) : null;
            },
            new { OrderId = id });
    }

    public async Task<Order> CreateAsync(CreateOrderRequest request)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Create",
            "Order",
            async () =>
            {
                var entity = new OrderEntity
                {
                    Id = Guid.NewGuid(),
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,
                    ShippingAddress = request.ShippingAddress,
                    ShippingCity = request.ShippingCity,
                    ShippingPostalCode = request.ShippingPostalCode,
                    Status = "New",
                    TotalAmount = 0,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Items = new List<OrderItemEntity>()
                };

                decimal totalAmount = 0;
                foreach (var itemRequest in request.Items)
                {
                    var product = await context.Products.FirstOrDefaultAsync(p => p.Id == itemRequest.ProductId);
                    if (product is null)
                        continue;

                    var item = new OrderItemEntity
                    {
                        Id = Guid.NewGuid(),
                        OrderId = entity.Id,
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = itemRequest.Quantity
                    };

                    entity.Items.Add(item);
                    totalAmount += product.Price * itemRequest.Quantity;
                }

                entity.TotalAmount = totalAmount;
                context.Orders.Add(entity);
                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new
            {
                CustomerEmail = MaskEmail(request.CustomerEmail),
                ItemCount = request.Items.Count
            });
    }

    /// <summary>
    /// Gets all orders with items included using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Order>> GetAllWithItemsAsync(int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetAllOrdersWithItems",
            async () =>
            {
                var spec = new OrdersWithItemsSpecification();
                operationLogger.LogSpecificationUsage(nameof(OrdersWithItemsSpecification), "Order", new { page, pageSize });
                
                var total = await context.Orders.CountAsync();
                
                var entities = await context.Orders
                    .ApplySpecification(spec)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Order>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new { page, pageSize, SpecName = nameof(OrdersWithItemsSpecification) });
    }

    /// <summary>
    /// Gets orders by customer email using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Order>> GetByCustomerEmailAsync(string customerEmail, int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetOrdersByCustomerEmail",
            async () =>
            {
                var spec = new OrdersByCustomerEmailSpecification(customerEmail);
                operationLogger.LogSpecificationUsage(
                    nameof(OrdersByCustomerEmailSpecification),
                    "Order",
                    new { CustomerEmail = MaskEmail(customerEmail), page, pageSize });
                
                var total = await context.Orders.ApplySpecification(spec).CountAsync();
                
                var entities = await context.Orders
                    .ApplySpecification(spec)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Order>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new
            {
                CustomerEmail = MaskEmail(customerEmail),
                page,
                pageSize,
                SpecName = nameof(OrdersByCustomerEmailSpecification)
            });
    }

    /// <summary>
    /// Gets orders created after a specific date using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Order>> GetCreatedAfterAsync(DateTimeOffset date, int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetOrdersCreatedAfter",
            async () =>
            {
                var spec = new OrdersCreatedAfterSpecification(date);
                operationLogger.LogSpecificationUsage(nameof(OrdersCreatedAfterSpecification), "Order", new { date, page, pageSize });
                
                var total = await context.Orders.ApplySpecification(spec).CountAsync();
                
                var entities = await context.Orders
                    .ApplySpecification(spec)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Order>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new { date, page, pageSize, SpecName = nameof(OrdersCreatedAfterSpecification) });
    }

    /// <summary>
    /// Gets orders with minimum total amount using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Order>> GetByMinimumAmountAsync(decimal minAmount, int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetOrdersByMinimumAmount",
            async () =>
            {
                var spec = new OrdersByMinimumAmountSpecification(minAmount);
                operationLogger.LogSpecificationUsage(nameof(OrdersByMinimumAmountSpecification), "Order", new { minAmount, page, pageSize });
                
                var total = await context.Orders.ApplySpecification(spec).CountAsync();
                
                var entities = await context.Orders
                    .ApplySpecification(spec)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Order>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new { minAmount, page, pageSize, SpecName = nameof(OrdersByMinimumAmountSpecification) });
    }

        public async Task<OrderSummary> GetOrdersSummaryAsync(DateTime? from = null, DateTime? to = null)
        {
            return await operationLogger.LogQueryPerformanceAsync(
                "GetOrdersSummary",
                async () =>
                {
                    var query = context.Orders.AsQueryable();
                    if (from.HasValue)
                        query = query.Where(o => o.CreatedAt >= from.Value);
                    if (to.HasValue)
                        query = query.Where(o => o.CreatedAt <= to.Value);

                    var totalOrders = await query.LongCountAsync();
                    var totalRevenue = await query.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
                    var avg = totalOrders > 0 ? decimal.Round(totalRevenue / totalOrders, 2) : 0m;

                    return new OrderSummary(totalOrders, totalRevenue, avg);
                },
                new { from, to });
        }

        public async Task<IReadOnlyList<SalesByDay>> GetSalesByDayAsync(DateTime? from = null, DateTime? to = null, int limit = 30)
        {
            return await operationLogger.LogQueryPerformanceAsync(
                "GetSalesByDay",
                async () =>
                {
                    var query = context.Orders.AsQueryable();
                    if (from.HasValue)
                        query = query.Where(o => o.CreatedAt >= from.Value);
                    if (to.HasValue)
                        query = query.Where(o => o.CreatedAt <= to.Value);

                    var intermediate = await query
                        .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month, o.CreatedAt.Day })
                        .Select(g => new {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Day = g.Key.Day,
                            OrdersCount = g.LongCount(),
                            Revenue = g.Sum(e => e.TotalAmount)
                        })
                        .OrderByDescending(x => x.Year)
                        .ThenByDescending(x => x.Month)
                        .ThenByDescending(x => x.Day)
                        .Take(limit)
                        .ToListAsync();

                    var grouped = intermediate
                        .Select(x => new SalesByDay(new DateTime(x.Year, x.Month, x.Day), x.OrdersCount, x.Revenue))
                        .OrderBy(s => s.Date)
                        .ToList();

                    return grouped;
                },
                new { from, to, limit });
        }

        public async Task<IReadOnlyList<StatusCount>> GetOrderStatusCountsAsync(DateTime? from = null, DateTime? to = null)
        {
            return await operationLogger.LogQueryPerformanceAsync(
                "GetOrderStatusCounts",
                async () =>
                {
                    var query = context.Orders.AsQueryable();
                    if (from.HasValue)
                        query = query.Where(o => o.CreatedAt >= from.Value);
                    if (to.HasValue)
                        query = query.Where(o => o.CreatedAt <= to.Value);

                    var grouped = await query
                        .GroupBy(o => o.Status)
                        .Select(g => new StatusCount(g.Key ?? "", g.LongCount()))
                        .ToListAsync();

                    return grouped;
                },
                new { from, to });
        }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "unknown";
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            return "unknown";
        }

        var firstChar = email[..1];
        var domain = email[(atIndex + 1)..];
        return $"{firstChar}***@{domain}";
    }

    private static Order MapToModel(OrderEntity entity) =>
        new(
            entity.Id,
            entity.CustomerName,
            entity.CustomerEmail,
            entity.CustomerPhone,
            entity.ShippingAddress,
            entity.ShippingCity,
            entity.ShippingPostalCode,
            entity.TotalAmount,
            entity.CreatedAt,
            entity.Items.Select(i => new OrderItem(i.Id, i.ProductId, i.ProductName, i.Price, i.Quantity)).ToList(),
            entity.Status
        );
}
