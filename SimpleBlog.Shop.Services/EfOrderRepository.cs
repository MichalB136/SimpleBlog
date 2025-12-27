using Microsoft.EntityFrameworkCore;
using SimpleBlog.Common;

namespace SimpleBlog.Shop.Services;

public sealed class EfOrderRepository(ShopDbContext context) : IOrderRepository
{
    public IEnumerable<Order> GetAll()
    {
        var entities = context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
        return entities.Select(MapToModel);
    }

    public Order? GetById(Guid id)
    {
        var entity = context.Orders
            .Include(o => o.Items)
            .FirstOrDefault(o => o.Id == id);
        return entity is not null ? MapToModel(entity) : null;
    }

    public Order Create(CreateOrderRequest request)
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
            TotalAmount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = new List<OrderItemEntity>()
        };

        decimal totalAmount = 0;
        foreach (var itemRequest in request.Items)
        {
            var product = context.Products.FirstOrDefault(p => p.Id == itemRequest.ProductId);
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
        context.SaveChanges();
        return MapToModel(entity);
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
            entity.Items.Select(i => new OrderItem(i.Id, i.ProductId, i.ProductName, i.Price, i.Quantity)).ToList()
        );
}
