using SimpleBlog.Common.Interfaces;
using SimpleBlog.Common.Models;

namespace SimpleBlog.ApiService.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public Task<PaginatedResult<Product>> GetAllAsync(ProductFilterRequest? filter = null, int page = 1, int pageSize = 10)
        => _repository.GetAllAsync(filter, page, pageSize);

    public Task<Product?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);
}
