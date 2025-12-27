using SimpleBlog.Common;

namespace SimpleBlog.Shop.Services;

public sealed class EfProductRepository(ShopDbContext context) : IProductRepository
{
    public IEnumerable<Product> GetAll()
    {
        var entities = context.Products.OrderByDescending(p => p.CreatedAt).ToList();
        return entities.Select(MapToModel);
    }

    public Product? GetById(Guid id)
    {
        var entity = context.Products.FirstOrDefault(p => p.Id == id);
        return entity is not null ? MapToModel(entity) : null;
    }

    public Product Create(CreateProductRequest request)
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
        context.SaveChanges();
        return MapToModel(entity);
    }

    public Product? Update(Guid id, UpdateProductRequest request)
    {
        var entity = context.Products.FirstOrDefault(p => p.Id == id);
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

        context.SaveChanges();
        return MapToModel(entity);
    }

    public bool Delete(Guid id)
    {
        var entity = context.Products.FirstOrDefault(p => p.Id == id);
        if (entity is null)
            return false;

        context.Products.Remove(entity);
        context.SaveChanges();
        return true;
    }

    private static Product MapToModel(ProductEntity entity) =>
        new(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Price,
            entity.ImageUrl,
            entity.Category,
            entity.Stock,
            entity.CreatedAt
        );
}
