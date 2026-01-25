using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SimpleBlog.ApiService;
using SimpleBlog.ApiService.Handlers;
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

        products.MapGet(endpointConfig.Products.GetAll, (IProductHandler handler, HttpContext ctx) => handler.GetAll(ctx));
        products.MapGet(endpointConfig.Products.GetById, (IProductHandler handler, Guid id, HttpContext ctx) => handler.GetById(id, ctx));
        products.MapPost(endpointConfig.Products.Create, (IProductHandler handler, CreateProductRequest req) => handler.Create(req)).RequireAuthorization();
        products.MapPut(endpointConfig.Products.Update, (IProductHandler handler, Guid id, UpdateProductRequest req, HttpContext ctx) => handler.Update(id, req, ctx)).RequireAuthorization();
        products.MapDelete(endpointConfig.Products.Delete, (IProductHandler handler, Guid id, HttpContext ctx) => handler.Delete(id, ctx)).RequireAuthorization();
        products.MapPut("/{id:guid}/tags", (IProductHandler handler, Guid id, AssignTagsRequest req, HttpContext ctx) => handler.AssignTags(id, req, ctx)).RequireAuthorization();
        products.MapPost("/{id:guid}/images", 
            (Guid id, IFormFile file, IProductRepository repository, IImageStorageService imageStorage, HttpContext context, ILogger<Program> logger, CancellationToken ct) 
                => AddImageToProduct(id, file, repository, imageStorage, context, logger, ct))
                .RequireAuthorization()
                .DisableAntiforgery(); // Allow multipart/form-data without antiforgery cookie (API uses JWT auth)
        products.MapPost("/{id:guid}/view", (IProductHandler handler, Guid id, HttpContext ctx) => handler.RecordView(id, ctx));
        products.MapGet("/analytics/top-sold", (IProductHandler handler, HttpContext ctx) => handler.GetTopSold(ctx)).RequireAuthorization();
        products.MapGet("/analytics/top-viewed", (IProductHandler handler, HttpContext ctx) => handler.GetTopViewed(ctx)).RequireAuthorization();
    }

    private static async Task<IResult> GetAll(
        HttpContext context,
        IProductRepository repository,
        int page = 1,
        int pageSize = 10)
    {
        // Parse tagIds from query string
        var tagIds = context.Request.Query["tagIds"]
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => Guid.TryParse(s, out var guid) ? guid : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();
        
        var category = context.Request.Query["category"].FirstOrDefault();
        var searchTerm = context.Request.Query["searchTerm"].FirstOrDefault();
        
        var filter = new ProductFilterRequest(
            tagIds.Count > 0 ? tagIds : null,
            string.IsNullOrWhiteSpace(category) ? null : category,
            string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm
        );
        
        return Results.Ok(await repository.GetAllAsync(filter, page, pageSize));
    }

    private static async Task<IResult> GetById(Guid id, IProductRepository repository, HttpContext context)
    {
        var product = await repository.GetByIdAsync(id);
        if (product is null)
            return Results.NotFound();

        try
        {
            var userId = context.User?.Identity?.Name;
            var sessionId = context.Request.Headers.ContainsKey("X-Session-Id")
                ? context.Request.Headers["X-Session-Id"].FirstOrDefault()
                : context.Request.Query["sessionId"].FirstOrDefault();

            // Record view asynchronously (best-effort)
            await repository.RecordViewAsync(id, userId, sessionId);
        }
        catch
        {
            // Swallow logging here to avoid breaking product fetch on analytics errors
        }

        return Results.Ok(product);
    }

    private static async Task<IResult> Create(
        CreateProductRequest request,
        IProductRepository repository,
        ILogger<Program> logger,
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

    private static async Task<IResult> RecordView(Guid id, IProductRepository repository, HttpContext context)
    {
        try
        {
            // Optionally capture user id or session id
            var userId = context.User?.Identity?.Name;
            var sessionId = context.Request.Query["sessionId"].FirstOrDefault();
            await repository.RecordViewAsync(id, userId, sessionId);
            return Results.Accepted();
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error recording view for product {ProductId}", id);
            return Results.Problem("Failed to record view");
        }
    }

    private static async Task<IResult> GetTopSold(
        HttpContext context,
        IProductRepository repository,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 10,
        AuthorizationConfiguration authConfig = null)
    {
        if (authConfig is not null && authConfig.RequireAdminForOrderView && !context.User.IsInRole(SeedDataConstants.AdminRole))
            return Results.Forbid();

        var result = await repository.GetTopSoldProductsAsync(from, to, limit);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetTopViewed(
        HttpContext context,
        IProductRepository repository,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 10,
        AuthorizationConfiguration authConfig = null)
    {
        if (authConfig is not null && authConfig.RequireAdminForOrderView && !context.User.IsInRole(SeedDataConstants.AdminRole))
            return Results.Forbid();

        var result = await repository.GetTopViewedProductsAsync(from, to, limit);
        return Results.Ok(result);
    }

    private static async Task<IResult> AddImageToProduct(
        Guid id,
        IFormFile file,
        IProductRepository repository,
        IImageStorageService imageStorage,
        HttpContext context,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation("POST /products/{ProductId}/images called by {UserName}", id, context.User.Identity?.Name);

        // Require admin role
        if (!context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            logger.LogWarning("User {UserName} attempted to add image to product without Admin role", context.User.Identity?.Name);
            return Results.Forbid();
        }

        if (file.Length == 0)
            return Results.BadRequest(new { error = "File is empty" });

        if (file.Length > 10 * 1024 * 1024) // 10 MB limit
            return Results.BadRequest(new { error = "File size cannot exceed 10 MB" });

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return Results.BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, GIF, WebP" });

        try
        {
            await using var stream = file.OpenReadStream();
            var imageUrl = await imageStorage.UploadImageAsync(stream, file.FileName, "products", ct);

            // Update product ImageUrl
            var updateReq = new UpdateProductRequest(null, null, null, imageUrl, null, null, null);
            var updated = await repository.UpdateAsync(id, updateReq);
            if (updated is null)
            {
                logger.LogWarning("Add image attempt for non-existent product: {ProductId}", id);
                return Results.NotFound(new { error = "Product not found" });
            }

            logger.LogInformation("Image added to product {ProductId} by {UserName}: {ImageUrl}", id, context.User.Identity?.Name, imageUrl);

            // Return product with signed image URL for client consumption
            var signedImageUrl = imageStorage.GenerateSignedUrl(updated.ImageUrl, expirationMinutes: 60);
            var updatedWithSigned = updated with { ImageUrl = signedImageUrl };
            return Results.Ok(updatedWithSigned);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding image to product {ProductId}", id);
            return Results.Problem("Failed to add image to product");
        }
    }
}
