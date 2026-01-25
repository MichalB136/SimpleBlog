using SimpleBlog.Common;
using SimpleBlog.Common.Extensions;
using SimpleBlog.Common.Specifications;
using SimpleBlog.Common.Logging;
using SimpleBlog.Shop.Services.Specifications;
using Microsoft.EntityFrameworkCore;

namespace SimpleBlog.Shop.Services;

public sealed class EfProductRepository(
    ShopDbContext context,
    IOperationLogger operationLogger) : IProductRepository
{
    public async Task<PaginatedResult<Product>> GetAllAsync(ProductFilterRequest? filter = null, int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetAllProducts",
            async () =>
            {
                var query = context.Products
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .Include(p => p.ProductColors)
                    .AsQueryable();

                // Apply filters
                if (filter is not null)
                {
                    // Filter by tags
                    if (filter.TagIds is not null && filter.TagIds.Count > 0)
                    {
                        query = query.Where(p => p.ProductTags.Any(pt => filter.TagIds.Contains(pt.TagId)));
                    }

                    // Filter by category
                    if (!string.IsNullOrWhiteSpace(filter.Category))
                    {
                        query = query.Where(p => p.Category.ToLower() == filter.Category.ToLower());
                    }

                    // Filter by search term (name or description)
                    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    {
                        var searchTerm = filter.SearchTerm.ToLower();
                        query = query.Where(p => 
                            p.Name.ToLower().Contains(searchTerm) || 
                            p.Description.ToLower().Contains(searchTerm));
                    }
                }

                var total = await query.CountAsync();
                var entities = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Product>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new { filter, page, pageSize });
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetProductById",
            async () =>
            {
                var entity = await context.Products
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .Include(p => p.ProductColors)
                    .FirstOrDefaultAsync(p => p.Id == id);
                return entity is not null ? MapToModel(entity) : null;
            },
            new { ProductId = id });
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Create",
            "Product",
            async () =>
            {
                var entity = new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    ImageUrl = request.ImageUrl,
                    Category = request.Category,
                    Stock = request.Stock,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                // Attach available colors if provided
                if (request.Colors is not null && request.Colors.Count > 0)
                {
                    foreach (var color in request.Colors)
                    {
                        entity.ProductColors.Add(new ProductColorEntity
                        {
                            ProductId = entity.Id,
                            Color = color
                        });
                    }
                }

                context.Products.Add(entity);
                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new { request.Name, request.Price, request.Category });
    }

    public async Task<Product?> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Update",
            "Product",
            async () =>
            {
                var entity = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (entity is null)
                    return null;

                if (request.Name is not null)
                    entity.Name = request.Name;
                if (request.Description is not null)
                    entity.Description = request.Description;
                if (request.Price.HasValue)
                    entity.Price = request.Price.Value;
                if (request.ImageUrl is not null)
                    entity.ImageUrl = request.ImageUrl;
                if (request.Category is not null)
                    entity.Category = request.Category;
                if (request.Stock.HasValue)
                    entity.Stock = request.Stock.Value;

                // Update colors if provided
                if (request.Colors is not null)
                {
                    // ensure collection is loaded
                    await context.Entry(entity).Collection(e => e.ProductColors).LoadAsync();
                    context.ProductColors.RemoveRange(entity.ProductColors);
                    foreach (var color in request.Colors)
                    {
                        entity.ProductColors.Add(new ProductColorEntity
                        {
                            ProductId = entity.Id,
                            Color = color
                        });
                    }
                }

                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new { ProductId = id, HasName = !string.IsNullOrEmpty(request.Name), HasPrice = request.Price.HasValue });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "Delete",
            "Product",
            async () =>
            {
                var entity = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (entity is null)
                    return false;

                context.Products.Remove(entity);
                await context.SaveChangesAsync();
                return true;
            },
            new { ProductId = id });
    }

    /// <summary>
    /// Gets all products that are in stock using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Product>> GetInStockAsync(int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetInStockProducts",
            async () =>
            {
                var spec = new ProductsInStockSpecification();
                operationLogger.LogSpecificationUsage(nameof(ProductsInStockSpecification), "Product", new { page, pageSize });
                
                var total = await context.Products.ApplySpecification(spec).CountAsync();
                
                var entities = await context.Products
                    .ApplySpecification(spec)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Product>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new { page, pageSize, SpecName = nameof(ProductsInStockSpecification) });
    }

    /// <summary>
    /// Gets products by category using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Product>> GetByCategoryAsync(string category, int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetProductsByCategory",
            async () =>
            {
                var spec = new ProductsByCategorySpecification(category);
                operationLogger.LogSpecificationUsage(nameof(ProductsByCategorySpecification), "Product", new { category, page, pageSize });
                
                var total = await context.Products.ApplySpecification(spec).CountAsync();
                
                var entities = await context.Products
                    .ApplySpecification(spec)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Product>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new { category, page, pageSize, SpecName = nameof(ProductsByCategorySpecification) });
    }

    /// <summary>
    /// Gets products by price range using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Product>> GetByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice, 
        int page = 1, 
        int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetProductsByPriceRange",
            async () =>
            {
                var spec = new ProductsByPriceRangeSpecification(minPrice, maxPrice);
                operationLogger.LogSpecificationUsage(nameof(ProductsByPriceRangeSpecification), "Product", new { minPrice, maxPrice, page, pageSize });
                
                var total = await context.Products.ApplySpecification(spec).CountAsync();
                
                var entities = await context.Products
                    .ApplySpecification(spec)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Product>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new { minPrice, maxPrice, page, pageSize, SpecName = nameof(ProductsByPriceRangeSpecification) });
    }

    /// <summary>
    /// Searches products by name or description using specification pattern.
    /// </summary>
    public async Task<PaginatedResult<Product>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "SearchProducts",
            async () =>
            {
                var spec = new ProductsSearchSpecification(searchTerm);
                operationLogger.LogSpecificationUsage(nameof(ProductsSearchSpecification), "Product", new { searchTerm, page, pageSize });
                
                var total = await context.Products.ApplySpecification(spec).CountAsync();
                
                var entities = await context.Products
                    .ApplySpecification(spec)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedResult<Product>
                {
                    Items = entities.Select(MapToModel).ToList(),
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            },
            new { searchTerm, page, pageSize, SpecName = nameof(ProductsSearchSpecification) });
    }

    private static Product MapToModel(ProductEntity entity)
    {
        var tags = entity.ProductTags
            .Select(pt => new Tag(
                pt.Tag.Id,
                pt.Tag.Name,
                pt.Tag.Slug,
                pt.Tag.Color,
                pt.Tag.CreatedAt))
            .ToList();

        var colors = entity.ProductColors?.Select(pc => pc.Color).ToList() ?? new List<string>();

        return new Product(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Price,
            entity.ImageUrl,
            entity.Category,
            entity.Stock,
            entity.CreatedAt,
            tags,
            colors
        );
    }

    public async Task<Product?> AssignTagsAsync(Guid productId, List<Guid> tagIds)
    {
        return await operationLogger.LogRepositoryOperationAsync(
            "AssignTags",
            "Product",
            async () =>
            {
                var entity = await context.Products
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstOrDefaultAsync(p => p.Id == productId);
                
                if (entity is null)
                    return null;

                // Remove existing tags
                context.ProductTags.RemoveRange(entity.ProductTags);

                // Add new tags
                foreach (var tagId in tagIds)
                {
                    entity.ProductTags.Add(new ProductTagEntity
                    {
                        ProductId = productId,
                        TagId = tagId
                    });
                }

                await context.SaveChangesAsync();
                return MapToModel(entity);
            },
            new { ProductId = productId, TagCount = tagIds.Count });
    }

    public async Task RecordViewAsync(Guid productId, string? userId = null, string? sessionId = null)
    {
        await operationLogger.LogRepositoryOperationAsync(
            "RecordView",
            "Product",
            async () =>
            {
                var view = new ProductViewEntity
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ViewedAt = DateTimeOffset.UtcNow,
                    UserId = userId,
                    SessionId = sessionId
                };
                context.ProductViews.Add(view);
                await context.SaveChangesAsync();
                return true;
            },
            new { ProductId = productId });
    }

    public async Task<IReadOnlyList<TopProduct>> GetTopSoldProductsAsync(DateTime? from = null, DateTime? to = null, int limit = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetTopSoldProducts",
            async () =>
            {
                var query = context.OrderItems.AsQueryable();
                if (from.HasValue)
                    query = query.Where(o => o.Order!.CreatedAt >= from.Value);
                if (to.HasValue)
                    query = query.Where(o => o.Order!.CreatedAt <= to.Value);

                var groupedRaw = await query
                    .GroupBy(i => new { i.ProductId, i.ProductName })
                    .Select(g => new { g.Key.ProductId, g.Key.ProductName, Count = g.Sum(x => x.Quantity) })
                    .OrderByDescending(x => x.Count)
                    .Take(limit)
                    .ToListAsync();

                var grouped = groupedRaw
                    .Select(g => new TopProduct(g.ProductId, g.ProductName, (long)g.Count))
                    .ToList();

                return grouped;
            },
            new { from, to, limit });
    }

    public async Task<IReadOnlyList<TopProduct>> GetTopViewedProductsAsync(DateTime? from = null, DateTime? to = null, int limit = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetTopViewedProducts",
            async () =>
            {
                var query = context.ProductViews.AsQueryable();
                if (from.HasValue)
                    query = query.Where(v => v.ViewedAt >= from.Value);
                if (to.HasValue)
                    query = query.Where(v => v.ViewedAt <= to.Value);

                var grouped = await query
                    .GroupBy(v => v.ProductId)
                    .Select(g => new { ProductId = g.Key, Views = g.LongCount() })
                    .OrderByDescending(x => x.Views)
                    .Take(limit)
                    .ToListAsync();

                // Join product names
                var productIds = grouped.Select(g => g.ProductId).ToList();
                var products = await context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

                var result = grouped.Select(g =>
                {
                    var prod = products.FirstOrDefault(p => p.Id == g.ProductId);
                    return new TopProduct(g.ProductId, prod?.Name ?? string.Empty, g.Views);
                }).ToList();

                return result;
            },
            new { from, to, limit });
    }
}
