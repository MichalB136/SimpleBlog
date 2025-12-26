using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SimpleBlog.ApiService.Data;

public class EfPostRepository : IPostRepository
{
    private readonly ApplicationDbContext _db;

    public EfPostRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    private static Post MapToRecord(PostEntity e)
    {
        var comments = e.Comments?.OrderBy(c => c.CreatedAt).Select(c => new Comment(c.Id, e.Id, c.Author, c.Content, c.CreatedAt)).ToArray() ?? Array.Empty<Comment>();
        return new Post(e.Id, e.Title, e.Content, e.Author, e.CreatedAt, comments, e.ImageUrl);
    }

    public IEnumerable<Post> GetAll()
    {
        return _db.Posts.Include(p => p.Comments).OrderByDescending(p => p.CreatedAt).AsNoTracking().Select(MapToRecord).ToList();
    }

    public Post? GetById(Guid id)
    {
        var e = _db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == id);
        return e is null ? null : MapToRecord(e);
    }

    public Post Create(CreatePostRequest request)
    {
        var author = string.IsNullOrWhiteSpace(request.Author) ? "Anon" : request.Author.Trim();
        var entity = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Author = author,
            CreatedAt = DateTimeOffset.UtcNow,
            ImageUrl = request.ImageUrl
        };

        _db.Posts.Add(entity);
        _db.SaveChanges();
        return MapToRecord(entity);
    }

    public Post? Update(Guid id, UpdatePostRequest request)
    {
        var existing = _db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == id);
        if (existing == null) return null;

        existing.Title = string.IsNullOrWhiteSpace(request.Title) ? existing.Title : request.Title.Trim();
        existing.Content = string.IsNullOrWhiteSpace(request.Content) ? existing.Content : request.Content.Trim();
        existing.Author = string.IsNullOrWhiteSpace(request.Author) ? existing.Author : request.Author.Trim();
        existing.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? existing.ImageUrl : request.ImageUrl;

        _db.SaveChanges();
        return MapToRecord(existing);
    }

    public bool Delete(Guid id)
    {
        var existing = _db.Posts.Find(id);
        if (existing == null) return false;
        _db.Posts.Remove(existing);
        _db.SaveChanges();
        return true;
    }

    public IReadOnlyList<Comment>? GetComments(Guid postId)
    {
        var post = _db.Posts.Include(p => p.Comments).FirstOrDefault(p => p.Id == postId);
        return post == null ? null : post.Comments.OrderBy(c => c.CreatedAt).Select(c => new Comment(c.Id, postId, c.Author, c.Content, c.CreatedAt)).ToList();
    }

    public Comment? AddComment(Guid postId, CreateCommentRequest request)
    {
        var post = _db.Posts.FirstOrDefault(p => p.Id == postId);
        if (post == null) return null;

        var author = string.IsNullOrWhiteSpace(request.Author) ? "Anon" : request.Author.Trim();
        var comment = new CommentEntity
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            Author = author,
            Content = (request.Content ?? string.Empty).Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Comments.Add(comment);
        _db.SaveChanges();

        return new Comment(comment.Id, postId, comment.Author, comment.Content, comment.CreatedAt);
    }
}
