using FluentValidation;
using SimpleBlog.ApiService;
using SimpleBlog.Common;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.ApiService.Endpoints;

public static class ProductEndpoints
{
    private sealed record UpdateProductDependencies(
        Guid Id,
        UpdateProductRequest Request,
        IValidator<UpdateProductRequest> Validator,
        IProductRepository Repository,
        IOperationLogger OperationLogger,
        HttpContext HttpContext,
        ILogger<Program> Logger,
        AuthorizationConfiguration AuthConfig,
        EndpointConfiguration EndpointConfig);
    public static void MapProductEndpoints(this WebApplication app)
    {
        var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();

        var products = app.MapGroup(endpointConfig.Products.Base);

        products.MapGet(endpointConfig.Products.GetAll, GetAll);
        products.MapGet(endpointConfig.Products.GetById, GetById);
        products.MapPost(endpointConfig.Products.Create, Create).RequireAuthorization();
        products.MapPut(endpointConfig.Products.Update, Update).RequireAuthorization();
        products.MapDelete(endpointConfig.Products.Delete, Delete).RequireAuthorization();
        products.MapPut("/{id:guid}/tags", AssignTags).RequireAuthorization();
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
        AuthorizationConfiguration authConfig,
        EndpointConfiguration endpointConfig)
    {
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
        AuthorizationConfiguration authConfig,
        EndpointConfiguration endpointConfig)
    {
        var deps = new UpdateProductDependencies(id, request, validator, repository, operationLogger, context, logger, authConfig, endpointConfig);
        return await PerformProductUpdateAsync(deps);
    }

    private static async Task<IResult> PerformProductUpdateAsync(UpdateProductDependencies deps)
    {
        if (deps.AuthConfig.RequireAdminForProductUpdate && !deps.HttpContext.User.IsInRole(SeedDataConstants.AdminRole))
        {
            deps.Logger.LogWarning("Unauthorized update attempt for product: {ProductId}", deps.Id);
            return Results.Forbid();
        }

        // Validate request using FluentValidation
        var validationResult = await deps.Validator.ValidateAsync(deps.Request);
        if (!validationResult.IsValid)
        {
            deps.OperationLogger.LogValidationFailure("UpdateProduct", deps.Request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var updated = await deps.Repository.UpdateAsync(deps.Id, deps.Request);
        if (updated is null)
        {
            deps.Logger.LogWarning("Update attempt for non-existent product: {ProductId}", deps.Id);
            return Results.NotFound();
        }
        
        deps.Logger.LogInformation("Product updated: {ProductId}", deps.Id);
        return Results.Ok(updated);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IProductRepository repository,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig)
    {
        if (authConfig.RequireAdminForProductDelete && !context.User.IsInRole(SeedDataConstants.AdminRole))
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

    private static async Task<IResult> AssignTags(
        Guid id,
        AssignTagsRequest request,
        IProductRepository repository,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig)
    {
        if (authConfig.RequireAdminForProductUpdate && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            logger.LogWarning("User {UserName} attempted to assign tags to product without Admin role", 
                context.User.Identity?.Name);
            return Results.Forbid();
        }

        try
        {
            var product = await repository.AssignTagsAsync(id, request.TagIds);
            if (product is null)
                return Results.NotFound($"Product with ID {id} not found");

            logger.LogInformation("Tags assigned to product {ProductId} by {UserName}: {TagCount} tags", 
                id, context.User.Identity?.Name, request.TagIds.Count);
            
            return Results.Ok(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning tags to product {ProductId}", id);
            return Results.Problem("Failed to assign tags to product");
        }
    }
}
