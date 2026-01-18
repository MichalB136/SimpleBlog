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
    Task<Post?> SetPinnedAsync(Guid id, bool isPinned);
    
    // Image management
    Task<Post?> AddImageAsync(Guid postId, string imageUrl);
    Task<Post?> RemoveImageAsync(Guid postId, string imageUrl);
    
    // Tag management
    Task<Post?> AssignTagsAsync(Guid postId, List<Guid> tagIds);
}

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(Guid id);
    Task<Tag?> GetBySlugAsync(string slug);
    Task<Tag> CreateAsync(CreateTagRequest request);
    Task<Tag?> UpdateAsync(Guid id, UpdateTagRequest request);
    Task<bool> DeleteAsync(Guid id);
}

public interface IAboutMeRepository
{
    Task<AboutMe?> GetAsync();
    Task<AboutMe> UpdateAsync(UpdateAboutMeRequest request, string updatedBy);
}

public interface IProductRepository
{
    Task<PaginatedResult<Product>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(CreateProductRequest request);
    Task<Product?> UpdateAsync(Guid id, UpdateProductRequest request);
    Task<bool> DeleteAsync(Guid id);
    
    // Tag management
    Task<Product?> AssignTagsAsync(Guid productId, List<Guid> tagIds);
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
    Task<(bool Success, string? ErrorMessage)> RegisterAsync(string username, string email, string password);
}

public interface ISiteSettingsRepository
{
    Task<SiteSettings?> GetAsync(CancellationToken ct = default);
    Task<SiteSettings> UpdateAsync(string theme, string updatedBy, CancellationToken ct = default);
    Task<SiteSettings> UpdateLogoAsync(string? logoUrl, string updatedBy, CancellationToken ct = default);
}
