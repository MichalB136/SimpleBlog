using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Interfaces;

public interface IPostRepository
{
    IEnumerable<Post> GetAll();
    Post? GetById(Guid id);
    Post Create(CreatePostRequest request);
    Post? Update(Guid id, UpdatePostRequest request);
    bool Delete(Guid id);
    IReadOnlyList<Comment>? GetComments(Guid postId);
    Comment? AddComment(Guid postId, CreateCommentRequest request);
}

public interface IProductRepository
{
    IEnumerable<Product> GetAll();
    Product? GetById(Guid id);
    Product Create(CreateProductRequest request);
    Product? Update(Guid id, UpdateProductRequest request);
    bool Delete(Guid id);
}

public interface IOrderRepository
{
    IEnumerable<Order> GetAll();
    Order? GetById(Guid id);
    Order Create(CreateOrderRequest request);
}

public interface IUserRepository
{
    User? ValidateUser(string username, string password);
}
