using FluentValidation;
using SimpleBlog.ApiService;
using SimpleBlog.Common;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.ApiService.Endpoints;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this WebApplication app)
    {
        var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();

        var posts = app.MapGroup(endpointConfig.Posts.Base);

        posts.MapGet(endpointConfig.Posts.GetAll, GetAll);
        posts.MapGet(endpointConfig.Posts.GetById, GetById);
        posts.MapPost(endpointConfig.Posts.Create, Create).RequireAuthorization();
        posts.MapPut(endpointConfig.Posts.Update, Update).RequireAuthorization();
        posts.MapDelete(endpointConfig.Posts.Delete, Delete).RequireAuthorization();
        posts.MapGet(endpointConfig.Posts.GetComments, GetComments);
        posts.MapPost(endpointConfig.Posts.AddComment, AddComment);
        posts.MapPut("/{id:guid}/pin", PinPost).RequireAuthorization();
        posts.MapPut("/{id:guid}/unpin", UnpinPost).RequireAuthorization();
    }

    private static async Task<IResult> GetAll(IPostRepository repository, int page = 1, int pageSize = 10) => 
        Results.Ok(await repository.GetAllAsync(page, pageSize));

    private static async Task<IResult> GetById(Guid id, IPostRepository repository)
    {
        var post = await repository.GetByIdAsync(id);
        return post is not null ? Results.Ok(post) : Results.NotFound();
    }

    private static async Task<IResult> Create(
        CreatePostRequest request,
        IValidator<CreatePostRequest> validator,
        IPostRepository repository,
        IOperationLogger operationLogger,
        HttpContext context,
        ILogger<Program> logger,
        EndpointConfiguration endpointConfig,
        AuthorizationConfiguration authConfig)
    {
        logger.LogInformation("POST {Endpoint} called by {UserName}", endpointConfig.Posts.Base, context.User.Identity?.Name);
        
        // Require admin role to create posts
        if (authConfig.RequireAdminForPostCreate && !context.User.IsInRole(SeedDataConstants.AdminUsername))
        {
            logger.LogWarning("User {UserName} attempted to create post without Admin role", context.User.Identity?.Name);
            return Results.Forbid();
        }

        // Validate request using FluentValidation
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            operationLogger.LogValidationFailure("CreatePost", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var created = await repository.CreateAsync(request);
        return Results.Created($"{endpointConfig.Posts.Base}/{created.Id}", created);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdatePostRequest request,
        IValidator<UpdatePostRequest> validator,
        IPostRepository repository,
        IOperationLogger operationLogger,
        ILogger<Program> logger,
        EndpointConfiguration endpointConfig)
    {
        // Validate request using FluentValidation
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            operationLogger.LogValidationFailure("UpdatePost", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var updated = await repository.UpdateAsync(id, request);
        if (updated is null)
        {
            logger.LogWarning("Update attempt for non-existent post: {PostId}", id);
            return Results.NotFound();
        }
        
        logger.LogInformation("Post updated: {PostId}", id);
        return Results.Ok(updated);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IPostRepository repository,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig)
    {
        // Require admin role to delete posts
        if (authConfig.RequireAdminForPostDelete && !context.User.IsInRole(SeedDataConstants.AdminUsername))
        {
            logger.LogWarning("Unauthorized delete attempt for post: {PostId}", id);
            return Results.Forbid();
        }
        
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
        {
            logger.LogWarning("Delete attempt for non-existent post: {PostId}", id);
            return Results.NotFound();
        }
        
        logger.LogInformation("Post deleted: {PostId}", id);
        return Results.NoContent();
    }

    private static async Task<IResult> GetComments(Guid id, IPostRepository repository)
    {
        var comments = await repository.GetCommentsAsync(id);
        return comments is not null ? Results.Ok(comments) : Results.NotFound();
    }

    private static async Task<IResult> AddComment(
        Guid id,
        CreateCommentRequest request,
        IValidator<CreateCommentRequest> validator,
        IPostRepository repository,
        IOperationLogger operationLogger,
        ILogger<Program> logger,
        EndpointConfiguration endpointConfig)
    {
        // Validate request using FluentValidation
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            operationLogger.LogValidationFailure("AddComment", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var created = await repository.AddCommentAsync(id, request);
        if (created is null)
        {
            logger.LogWarning("Comment creation attempt for non-existent post: {PostId}", id);
            return Results.NotFound();
        }
        
        logger.LogInformation("Comment added to post: {PostId}", id);
        return Results.Created($"{endpointConfig.Posts.Base}/{id}/comments/{created.Id}", created);
    }

    private static async Task<IResult> PinPost(
        Guid id,
        IPostRepository repository,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig)
    {
        // Require admin role to pin posts
        if (!context.User.IsInRole(SeedDataConstants.AdminUsername))
        {
            logger.LogWarning("User {UserName} attempted to pin post without Admin role", context.User.Identity?.Name);
            return Results.Forbid();
        }

        var pinned = await repository.SetPinnedAsync(id, true);
        if (pinned is null)
        {
            logger.LogWarning("Pin attempt for non-existent post: {PostId}", id);
            return Results.NotFound();
        }

        logger.LogInformation("Post pinned: {PostId} by {UserName}", id, context.User.Identity?.Name);
        return Results.Ok(pinned);
    }

    private static async Task<IResult> UnpinPost(
        Guid id,
        IPostRepository repository,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig)
    {
        // Require admin role to unpin posts
        if (!context.User.IsInRole(SeedDataConstants.AdminUsername))
        {
            logger.LogWarning("User {UserName} attempted to unpin post without Admin role", context.User.Identity?.Name);
            return Results.Forbid();
        }

        var unpinned = await repository.SetPinnedAsync(id, false);
        if (unpinned is null)
        {
            logger.LogWarning("Unpin attempt for non-existent post: {PostId}", id);
            return Results.NotFound();
        }

        logger.LogInformation("Post unpinned: {PostId} by {UserName}", id, context.User.Identity?.Name);
        return Results.Ok(unpinned);
    }
}
