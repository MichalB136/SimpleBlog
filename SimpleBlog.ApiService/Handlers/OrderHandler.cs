using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SimpleBlog.Common;
using SimpleBlog.ApiService.Services;
using SimpleBlog.Common.Models;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.ApiService.Handlers;

public sealed class OrderHandler : IOrderHandler
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderRequest> _validator;
    private readonly IOperationLogger _operationLogger;
    private readonly ILogger<OrderHandler> _logger;
    private readonly EndpointConfiguration _endpointConfig;
    private readonly AuthorizationConfiguration _authConfig;

    public OrderHandler(
        IOrderService orderService,
        IValidator<CreateOrderRequest> validator,
        IOperationLogger operationLogger,
        ILogger<OrderHandler> logger,
        EndpointConfiguration endpointConfig,
        AuthorizationConfiguration authConfig)
    {
        _orderService = orderService;
        _validator = validator;
        _operationLogger = operationLogger;
        _logger = logger;
        _endpointConfig = endpointConfig;
        _authConfig = authConfig;
    }

    public async Task<IResult> GetSummary(HttpContext context)
    {
        if (_authConfig.RequireAdminForOrderView && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("Unauthorized attempt to view orders analytics summary");
            return Results.Forbid();
        }

        var from = DateTime.TryParse(context.Request.Query["from"].FirstOrDefault(), out var f) ? f : (DateTime?)null;
        var to = DateTime.TryParse(context.Request.Query["to"].FirstOrDefault(), out var t) ? t : (DateTime?)null;

        var summary = await _orderService.GetOrdersSummaryAsync(from, to);
        return Results.Ok(summary);
    }

    public async Task<IResult> GetSalesByDay(HttpContext context)
    {
        if (_authConfig.RequireAdminForOrderView && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("Unauthorized attempt to view orders sales-by-day");
            return Results.Forbid();
        }

        var from = DateTime.TryParse(context.Request.Query["from"].FirstOrDefault(), out var f) ? f : (DateTime?)null;
        var to = DateTime.TryParse(context.Request.Query["to"].FirstOrDefault(), out var t) ? t : (DateTime?)null;
        var limit = int.TryParse(context.Request.Query["limit"].FirstOrDefault(), out var l) ? l : 30;

        var data = await _orderService.GetSalesByDayAsync(from, to, limit);
        return Results.Ok(data);
    }

    public async Task<IResult> GetStatusCounts(HttpContext context)
    {
        if (_authConfig.RequireAdminForOrderView && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("Unauthorized attempt to view orders status counts");
            return Results.Forbid();
        }

        var from = DateTime.TryParse(context.Request.Query["from"].FirstOrDefault(), out var f) ? f : (DateTime?)null;
        var to = DateTime.TryParse(context.Request.Query["to"].FirstOrDefault(), out var t) ? t : (DateTime?)null;

        var data = await _orderService.GetOrderStatusCountsAsync(from, to);
        return Results.Ok(data);
    }

    public async Task<IResult> GetAll(HttpContext context)
    {
        if (_authConfig.RequireAdminForOrderView && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("Unauthorized attempt to view all orders");
            return Results.Forbid();
        }

        var page = int.TryParse(context.Request.Query["page"].FirstOrDefault(), out var p) ? p : 1;
        var pageSize = int.TryParse(context.Request.Query["pageSize"].FirstOrDefault(), out var ps) ? ps : 10;

        var result = await _orderService.GetAllAsync(page, pageSize);
        return Results.Ok(result);
    }

    public async Task<IResult> GetById(Guid id, HttpContext context)
    {
        if (_authConfig.RequireAdminForOrderView && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("Unauthorized attempt to view order: {OrderId}", id);
            return Results.Forbid();
        }

        var order = await _orderService.GetByIdAsync(id);
        return order is not null ? Results.Ok(order) : Results.NotFound();
    }

    public async Task<IResult> Create(CreateOrderRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _operationLogger.LogValidationFailure("CreateOrder", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var created = await _orderService.CreateAsync(request);
        _logger.LogInformation("Order created: {OrderId}, Total: {Total}", created.Id, created.TotalAmount);
        return Results.Created($"{_endpointConfig.Orders.Base}/{created.Id}", created);
    }
}
