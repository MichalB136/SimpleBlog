using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SimpleBlog.Common;
using SimpleBlog.ApiService.Services;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.ApiService.Handlers;

public sealed class ProductHandler : IProductHandler
{
    private readonly IProductRepository _repository;
    private readonly IImageStorageService _imageStorage;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly IOperationLogger _operationLogger;
    private readonly ILogger<ProductHandler> _logger;
    private readonly EndpointConfiguration _endpointConfig;
    private readonly AuthorizationConfiguration _authConfig;

    public ProductHandler(
        IProductRepository repository,
        IImageStorageService imageStorage,
        IValidator<UpdateProductRequest> updateValidator,
        IOperationLogger operationLogger,
        ILogger<ProductHandler> logger,
        EndpointConfiguration endpointConfig,
        AuthorizationConfiguration authConfig)
    {
        _repository = repository;
        _imageStorage = imageStorage;
        _updateValidator = updateValidator;
        _operationLogger = operationLogger;
        _logger = logger;
        _endpointConfig = endpointConfig;
        _authConfig = authConfig;
    }

    // Backwards-compatible constructor used in tests or callers that don't supply IImageStorageService
    public ProductHandler(
        IProductRepository repository,
        IValidator<UpdateProductRequest> updateValidator,
        IOperationLogger operationLogger,
        ILogger<ProductHandler> logger,
        EndpointConfiguration endpointConfig,
        AuthorizationConfiguration authConfig)
        : this(repository, new NoOpImageStorageService(), updateValidator, operationLogger, logger, endpointConfig, authConfig)
    {
    }

    private Product GenerateSignedUrlForProduct(Product product)
    {
        if (string.IsNullOrEmpty(product.ImageUrl))
            return product;

        try
        {
            _logger.LogInformation(
                "Generating signed URL for product {ProductId}. Original ImageUrl: {ImageUrl}",
                product.Id,
                product.ImageUrl);
                
            var signed = _imageStorage.GenerateSignedUrl(product.ImageUrl, expirationMinutes: 60);
            
            _logger.LogInformation(
                "Generated signed URL for product {ProductId}. Signed ImageUrl: {SignedUrl}",
                product.Id,
                signed);
                
            return product with { ImageUrl = signed };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate signed URL for product image: {ImageUrl}", product.ImageUrl);
            return product;
        }
    }

    public async Task<IResult> GetAll(HttpContext context)
    {
        var tagIds = context.Request.Query["tagIds"]
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => Guid.TryParse(s, out var guid) ? guid : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();

        var category = context.Request.Query["category"].FirstOrDefault();
        var searchTerm = context.Request.Query["searchTerm"].FirstOrDefault();

        var page = int.TryParse(context.Request.Query["page"].FirstOrDefault(), out var p) ? p : 1;
        var pageSize = int.TryParse(context.Request.Query["pageSize"].FirstOrDefault(), out var ps) ? ps : 10;

        var filter = new ProductFilterRequest(
            tagIds.Count > 0 ? tagIds : null,
            string.IsNullOrWhiteSpace(category) ? null : category,
            string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm
        );

        var result = await _repository.GetAllAsync(filter, page, pageSize);

        // Generate signed URLs for product images so client can load them
        var itemsWithSigned = result.Items
            .Select(p => GenerateSignedUrlForProduct(p))
            .ToList();

        return Results.Ok(result with { Items = itemsWithSigned });
    }

    public async Task<IResult> GetById(Guid id, HttpContext context)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            return Results.NotFound();

        try
        {
            var userId = context.User?.Identity?.Name;
            var sessionId = context.Request.Headers.ContainsKey("X-Session-Id")
                ? context.Request.Headers["X-Session-Id"].FirstOrDefault()
                : context.Request.Query["sessionId"].FirstOrDefault();

            await _repository.RecordViewAsync(id, userId, sessionId);
        }
        catch
        {
            // swallow
        }

        var productWithSigned = GenerateSignedUrlForProduct(product);
        return Results.Ok(productWithSigned);
    }

    public async Task<IResult> Create(CreateProductRequest request)
    {
        var created = await _repository.CreateAsync(request);
        _logger.LogInformation("Product created: {ProductId}", created.Id);
        var createdWithSigned = GenerateSignedUrlForProduct(created);
        return Results.Created($"{_endpointConfig.Products.Base}/{created.Id}", createdWithSigned);
    }

    public async Task<IResult> Update(Guid id, UpdateProductRequest request, HttpContext context)
    {
        if (_authConfig.RequireAdminForProductUpdate && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("Unauthorized update attempt for product: {ProductId}", id);
            return Results.Forbid();
        }

        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _operationLogger.LogValidationFailure("UpdateProduct", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var updated = await _repository.UpdateAsync(id, request);
        if (updated is null)
        {
            _logger.LogWarning("Update attempt for non-existent product: {ProductId}", id);
            return Results.NotFound();
        }

        _logger.LogInformation("Product updated: {ProductId}", id);
        var updatedWithSigned = GenerateSignedUrlForProduct(updated);
        return Results.Ok(updatedWithSigned);
    }

    public async Task<IResult> Delete(Guid id, HttpContext context)
    {
        if (_authConfig.RequireAdminForProductDelete && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("Unauthorized delete attempt for product: {ProductId}", id);
            return Results.Forbid();
        }

        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Delete attempt for non-existent product: {ProductId}", id);
            return Results.NotFound();
        }

        _logger.LogInformation("Product deleted: {ProductId}", id);
        return Results.NoContent();
    }

    public async Task<IResult> AssignTags(Guid id, AssignTagsRequest request, HttpContext context)
    {
        if (_authConfig.RequireAdminForProductUpdate && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("User {UserName} attempted to assign tags to product without Admin role", PiiMask.MaskUserName(context.User.Identity?.Name));
            return Results.Forbid();
        }

        try
        {
            var product = await _repository.AssignTagsAsync(id, request.TagIds);
            if (product is null)
                return Results.NotFound($"Product with ID {id} not found");

            _logger.LogInformation("Tags assigned to product {ProductId} by {UserName}: {TagCount} tags", id, PiiMask.MaskUserName(context.User.Identity?.Name), request.TagIds.Count);
            var productWithSigned = GenerateSignedUrlForProduct(product);
            return Results.Ok(productWithSigned);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning tags to product {ProductId}", id);
            return Results.Problem("Failed to assign tags to product");
        }
    }

    public async Task<IResult> RecordView(Guid id, HttpContext context)
    {
        try
        {
            var userId = context.User?.Identity?.Name;
            var sessionId = context.Request.Query["sessionId"].FirstOrDefault();
            await _repository.RecordViewAsync(id, userId, sessionId);
            return Results.Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording view for product {ProductId}", id);
            return Results.Problem("Failed to record view");
        }
    }

    public async Task<IResult> GetTopSold(HttpContext context)
    {
        if (_authConfig.RequireAdminForOrderView && !context.User.IsInRole(SeedDataConstants.AdminRole))
            return Results.Forbid();

        var from = DateTime.TryParse(context.Request.Query["from"].FirstOrDefault(), out var f) ? f : (DateTime?)null;
        var to = DateTime.TryParse(context.Request.Query["to"].FirstOrDefault(), out var t) ? t : (DateTime?)null;
        var limit = int.TryParse(context.Request.Query["limit"].FirstOrDefault(), out var l) ? l : 10;

        var result = await _repository.GetTopSoldProductsAsync(from, to, limit);
        return Results.Ok(result);
    }

    public async Task<IResult> GetTopViewed(HttpContext context)
    {
        if (_authConfig.RequireAdminForOrderView && !context.User.IsInRole(SeedDataConstants.AdminRole))
            return Results.Forbid();

        var from = DateTime.TryParse(context.Request.Query["from"].FirstOrDefault(), out var f) ? f : (DateTime?)null;
        var to = DateTime.TryParse(context.Request.Query["to"].FirstOrDefault(), out var t) ? t : (DateTime?)null;
        var limit = int.TryParse(context.Request.Query["limit"].FirstOrDefault(), out var l) ? l : 10;

        var result = await _repository.GetTopViewedProductsAsync(from, to, limit);
        return Results.Ok(result);
    }
}
