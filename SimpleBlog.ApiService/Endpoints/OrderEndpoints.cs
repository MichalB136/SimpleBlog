using FluentValidation;
using SimpleBlog.ApiService;
using SimpleBlog.Common;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.ApiService.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();

        var orders = app.MapGroup(endpointConfig.Orders.Base);

        orders.MapGet(endpointConfig.Orders.GetAll, GetAll).RequireAuthorization();
        orders.MapGet(endpointConfig.Orders.GetById, GetById).RequireAuthorization();
        orders.MapPost(endpointConfig.Orders.Create, Create);
    }

    private static async Task<IResult> GetAll(
        IOrderRepository repository,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig,
        int page = 1,
        int pageSize = 10)
    {
        if (authConfig.RequireAdminForOrderView && !context.User.IsInRole("Admin"))
        {
            logger.LogWarning("Unauthorized attempt to view all orders");
            return Results.Forbid();
        }
        
        return Results.Ok(await repository.GetAllAsync(page, pageSize));
    }

    private static async Task<IResult> GetById(
        Guid id,
        IOrderRepository repository,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig)
    {
        if (authConfig.RequireAdminForOrderView && !context.User.IsInRole("Admin"))
        {
            logger.LogWarning("Unauthorized attempt to view order: {OrderId}", id);
            return Results.Forbid();
        }
        
        var order = await repository.GetByIdAsync(id);
        return order is not null ? Results.Ok(order) : Results.NotFound();
    }

    private static async Task<IResult> Create(
        CreateOrderRequest request,
        IValidator<CreateOrderRequest> validator,
        IOrderRepository repository,
        IOperationLogger operationLogger,
        IEmailService emailService,
        ILogger<Program> logger,
        EndpointConfiguration endpointConfig)
    {
        // Validate request using FluentValidation
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            operationLogger.LogValidationFailure("CreateOrder", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var created = await repository.CreateAsync(request);
        logger.LogInformation("Order created: {OrderId}, Total: {Total}", created.Id, created.TotalAmount);
        
        // Send email notification
        await emailService.SendOrderConfirmationAsync(request.CustomerEmail, request.CustomerName, created);
        logger.LogInformation("Order confirmation email sent to: {Email}", request.CustomerEmail);
        
        return Results.Created($"{endpointConfig.Orders.Base}/{created.Id}", created);
    }
}
