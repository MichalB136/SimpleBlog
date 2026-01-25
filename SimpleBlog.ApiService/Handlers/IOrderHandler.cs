using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleBlog.Common.Models;

namespace SimpleBlog.ApiService.Handlers;

public interface IOrderHandler
{
    Task<IResult> GetAll(HttpContext context);
    Task<IResult> GetById(Guid id, HttpContext context);
    Task<IResult> Create(CreateOrderRequest request);
    Task<IResult> GetSummary(HttpContext context);
    Task<IResult> GetSalesByDay(HttpContext context);
    Task<IResult> GetStatusCounts(HttpContext context);
}
