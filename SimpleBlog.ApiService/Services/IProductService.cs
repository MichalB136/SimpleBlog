using SimpleBlog.Common.Models;

namespace SimpleBlog.ApiService.Services;

public interface IProductService
{
    Task<PaginatedResult<Product>> GetAllAsync(ProductFilterRequest? filter = null, int page = 1, int pageSize = 10);
    Task<Product?> GetByIdAsync(Guid id);
}
