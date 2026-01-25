using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleBlog.Common;

namespace SimpleBlog.ApiService.Handlers;

public interface IProductHandler
{
    Task<IResult> GetAll(HttpContext context);
    Task<IResult> GetById(Guid id, HttpContext context);
    Task<IResult> Create(CreateProductRequest request);
    Task<IResult> Update(Guid id, UpdateProductRequest request, HttpContext context);
    Task<IResult> Delete(Guid id, HttpContext context);
    Task<IResult> AssignTags(Guid id, AssignTagsRequest request, HttpContext context);
    Task<IResult> RecordView(Guid id, HttpContext context);
    Task<IResult> GetTopSold(HttpContext context);
    Task<IResult> GetTopViewed(HttpContext context);
}
