using FluentValidation;
using SimpleBlog.ApiService;
using SimpleBlog.Common;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.ApiService.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();
        var authConfig = app.Services.GetRequiredService<AuthorizationConfiguration>();

        var products = app.MapGroup(endpointConfig.Products.Base);

        products.MapGet(endpointConfig.Products.GetAll, GetAll);
        products.MapGet(endpointConfig.Products.GetById, GetById);
        products.MapPost(endpointConfig.Products.Create, Create).RequireAuthorization();
        products.MapPut(endpointConfig.Products.Update, Update).RequireAuthorization();
        products.MapDelete(endpointConfig.Products.Delete, Delete).RequireAuthorization();
    }

    private static async Task<IResult> GetAll(
        IProductRepository repository,
        int page = 1,
        int pageSize = 10) =>
        Results.Ok(await repository.GetAllAsync(page, pageSize));

    private static async Task<IResult> GetById(Guid id, IProductRepository repository)
    {
        var product = await repository.GetByIdAsync(id);
        return product is not null ? Results.Ok(product) : Results.NotFound();
    }

    private static async Task<IResult> Create(
        CreateProductRequest request,
        IValidator<CreateProductRequest> validator,
        IProductRepository repository,
        IOperationLogger operationLogger,
        HttpContext context,
        ILogger<Program> logger,
        EndpointConfiguration endpointConfig,
        AuthorizationConfiguration authConfig)
    {
        if (authConfig.RequireAdminForProductCreate && !context.User.IsInRole(SeedDataConstants.AdminUsername))
        {
            logger.LogWarning("User {UserName} attempted to create product without Admin role", context.User.Identity?.Name);
            return Results.Forbid();
        }

        // Validate request using FluentValidation
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            operationLogger.LogValidationFailure("CreateProduct", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var created = await repository.CreateAsync(request);
        logger.LogInformation("Product created: {ProductId}", created.Id);
        return Results.Created($"{endpointConfig.Products.Base}/{created.Id}", created);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateProductRequest request,
        IValidator<UpdateProductRequest> validator,
        IProductRepository repository,
        IOperationLogger operationLogger,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig)
    {
        if (authConfig.RequireAdminForProductUpdate && !context.User.IsInRole(SeedDataConstants.AdminUsername))
        {
            logger.LogWarning("Unauthorized update attempt for product: {ProductId}", id);
            return Results.Forbid();
        }

        // Validate request using FluentValidation
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            operationLogger.LogValidationFailure("UpdateProduct", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var updated = await repository.UpdateAsync(id, request);
        if (updated is null)
        {
            logger.LogWarning("Update attempt for non-existent product: {ProductId}", id);
            return Results.NotFound();
        }
        
        logger.LogInformation("Product updated: {ProductId}", id);
        return Results.Ok(updated);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IProductRepository repository,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig)
    {
        if (authConfig.RequireAdminForProductDelete && !context.User.IsInRole(SeedDataConstants.AdminUsername))
        {
            logger.LogWarning("Unauthorized delete attempt for product: {ProductId}", id);
            return Results.Forbid();
        }
        
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
        {
            logger.LogWarning("Delete attempt for non-existent product: {ProductId}", id);
            return Results.NotFound();
        }
        
        logger.LogInformation("Product deleted: {ProductId}", id);
        return Results.NoContent();
    }
}
