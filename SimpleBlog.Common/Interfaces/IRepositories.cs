using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Interfaces;

public interface IPostRepository
{
    Task<PaginatedResult<Post>> GetAllAsync(PostFilterRequest? filter = null, int page = 1, int pageSize = 10);
    Task<Post?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Post>> GetByTagAsync(Guid tagId);
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
    Task<PaginatedResult<Product>> GetAllAsync(ProductFilterRequest? filter = null, int page = 1, int pageSize = 10);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(CreateProductRequest request);
    Task<Product?> UpdateAsync(Guid id, UpdateProductRequest request);
    Task<bool> DeleteAsync(Guid id);
    
    // Tag management
    Task<Product?> AssignTagsAsync(Guid productId, List<Guid> tagIds);

    // Analytics
    Task RecordViewAsync(Guid productId, string? userId = null, string? sessionId = null);
    Task<IReadOnlyList<TopProduct>> GetTopSoldProductsAsync(DateTime? from = null, DateTime? to = null, int limit = 10);
    Task<IReadOnlyList<TopProduct>> GetTopViewedProductsAsync(DateTime? from = null, DateTime? to = null, int limit = 10);
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
    Task SaveRefreshTokenAsync(string username, string refreshToken, DateTime expiresUtc);
    Task<string?> GetUsernameByRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task<User?> GetUserByUsernameAsync(string username);
}

public interface ISiteSettingsRepository
{
    Task<SiteSettings?> GetAsync(CancellationToken ct = default);
    Task<SiteSettings> UpdateAsync(string theme, string updatedBy, CancellationToken ct = default);
    Task<SiteSettings> UpdateLogoAsync(string? logoUrl, string updatedBy, CancellationToken ct = default);
}
