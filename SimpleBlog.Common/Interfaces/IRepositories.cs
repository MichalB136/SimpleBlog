using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Interfaces;

public interface IPostRepository
{
    Task<PaginatedResult<Post>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<Post?> GetByIdAsync(Guid id);
    Task<Post> CreateAsync(CreatePostRequest request);
    Task<Post?> UpdateAsync(Guid id, UpdatePostRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<IReadOnlyList<Comment>?> GetCommentsAsync(Guid postId);
    Task<Comment?> AddCommentAsync(Guid postId, CreateCommentRequest request);
}

public interface IProductRepository
{
    Task<PaginatedResult<Product>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(CreateProductRequest request);
    Task<Product?> UpdateAsync(Guid id, UpdateProductRequest request);
    Task<bool> DeleteAsync(Guid id);
}

public interface IOrderRepository
{
    Task<PaginatedResult<Order>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> CreateAsync(CreateOrderRequest request);
}

public interface IUserRepository
{
    Task<User?> ValidateUserAsync(string username, string password);
}
