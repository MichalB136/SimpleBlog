using FluentValidation;
using SimpleBlog.ApiService;
using SimpleBlog.ApiService.Handlers;
using SimpleBlog.Common;
using SimpleBlog.Common.Logging;
using static SimpleBlog.ApiService.SeedDataConstants;

namespace SimpleBlog.ApiService.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();

        var orders = app.MapGroup(endpointConfig.Orders.Base);

        orders.MapGet(endpointConfig.Orders.GetAll, (IOrderHandler handler, HttpContext ctx) => handler.GetAll(ctx)).RequireAuthorization();
        orders.MapGet(endpointConfig.Orders.GetById, (IOrderHandler handler, Guid id, HttpContext ctx) => handler.GetById(id, ctx)).RequireAuthorization();
        // Analytics endpoints for orders
        orders.MapGet("/analytics/summary", (IOrderHandler handler, HttpContext ctx) => handler.GetSummary(ctx)).RequireAuthorization();
        orders.MapGet("/analytics/sales-by-day", (IOrderHandler handler, HttpContext ctx) => handler.GetSalesByDay(ctx)).RequireAuthorization();
        orders.MapGet("/analytics/status-counts", (IOrderHandler handler, HttpContext ctx) => handler.GetStatusCounts(ctx)).RequireAuthorization();
        orders.MapPost(endpointConfig.Orders.Create, (IOrderHandler handler, CreateOrderRequest req) => handler.Create(req));
    }
}
