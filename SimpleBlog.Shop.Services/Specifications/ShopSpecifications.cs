using Microsoft.EntityFrameworkCore;
using SimpleBlog.Common.Specifications;

namespace SimpleBlog.Shop.Services.Specifications;

/// <summary>
/// Specification for filtering products that are in stock.
/// </summary>
public sealed class ProductsInStockSpecification : Specification<ProductEntity>
{
    public override IQueryable<ProductEntity> Apply(IQueryable<ProductEntity> query)
    {
        return query
            .Where(p => p.Stock > 0)
            .OrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for filtering products by category.
/// </summary>
public sealed class ProductsByCategorySpecification(string category) : Specification<ProductEntity>
{
    public override IQueryable<ProductEntity> Apply(IQueryable<ProductEntity> query)
    {
        return query
            .Where(p => p.Category == category)
            .OrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for filtering products by price range.
/// </summary>
public sealed class ProductsByPriceRangeSpecification(decimal minPrice, decimal maxPrice) : Specification<ProductEntity>
{
    public override IQueryable<ProductEntity> Apply(IQueryable<ProductEntity> query)
    {
        return query
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Price);
    }
}

/// <summary>
/// Specification for searching products by name or description.
/// </summary>
public sealed class ProductsSearchSpecification(string searchTerm) : Specification<ProductEntity>
{
    public override IQueryable<ProductEntity> Apply(IQueryable<ProductEntity> query)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return query
            .Where(p => p.Name.ToLower().Contains(lowerSearchTerm) || 
                       p.Description.ToLower().Contains(lowerSearchTerm))
            .OrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for loading orders with their items included.
/// </summary>
public sealed class OrdersWithItemsSpecification : Specification<OrderEntity>
{
    public override IQueryable<OrderEntity> Apply(IQueryable<OrderEntity> query)
    {
        return query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for filtering orders by customer email.
/// </summary>
public sealed class OrdersByCustomerEmailSpecification(string customerEmail) : Specification<OrderEntity>
{
    public override IQueryable<OrderEntity> Apply(IQueryable<OrderEntity> query)
    {
        return query
            .Where(o => o.CustomerEmail == customerEmail)
            .OrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for filtering orders created after a specific date.
/// </summary>
public sealed class OrdersCreatedAfterSpecification(DateTimeOffset date) : Specification<OrderEntity>
{
    public override IQueryable<OrderEntity> Apply(IQueryable<OrderEntity> query)
    {
        return query
            .Where(o => o.CreatedAt >= date)
            .OrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for filtering orders by minimum total amount.
/// </summary>
public sealed class OrdersByMinimumAmountSpecification(decimal minAmount) : Specification<OrderEntity>
{
    public override IQueryable<OrderEntity> Apply(IQueryable<OrderEntity> query)
    {
        return query
            .Where(o => o.TotalAmount >= minAmount)
            .OrderByDescending(o => o.TotalAmount);
    }
}
