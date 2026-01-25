using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleBlog.Common;

namespace SimpleBlog.ApiService.Handlers;

public interface IPostHandler
{
    Task<IResult> GetAll(HttpContext context, int page = 1, int pageSize = 10);
    Task<IResult> GetById(Guid id);
    Task<IResult> Create(HttpContext context, CancellationToken ct);
    Task<IResult> Update(Guid id, UpdatePostRequest request, HttpContext context);
    Task<IResult> Delete(Guid id, HttpContext context);
    Task<IResult> GetComments(Guid id);
    Task<IResult> AddComment(Guid id, CreateCommentRequest request);
    Task<IResult> PinPost(Guid id, HttpContext context);
    Task<IResult> UnpinPost(Guid id, HttpContext context);
    Task<IResult> AddImageToPost(Guid id, IFormFile file, HttpContext context, CancellationToken ct);
    Task<IResult> RemoveImageFromPost(Guid id, string imageUrl, HttpContext context, CancellationToken ct);
    Task<IResult> AssignTags(Guid id, AssignTagsRequest request, HttpContext context);
}
