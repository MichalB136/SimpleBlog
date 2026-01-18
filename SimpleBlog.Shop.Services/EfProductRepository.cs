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
    public async Task<PaginatedResult<Product>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await operationLogger.LogQueryPerformanceAsync(
            "GetAllProducts",
            async () =>
            {
                var total = await context.Products.CountAsync();
                var entities = await context.Products
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
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
            new { page, pageSize });
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

        return new Product(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Price,
            entity.ImageUrl,
            entity.Category,
            entity.Stock,
            entity.CreatedAt,
            tags
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
}
