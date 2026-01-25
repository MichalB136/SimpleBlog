using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SimpleBlog.ApiService;
using SimpleBlog.ApiService.Handlers;
using SimpleBlog.Common;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.ApiService.Endpoints;

public static class PostEndpoints
{
    private sealed record CreatePostDependencies(
        HttpContext HttpContext,
        IValidator<CreatePostRequest> Validator,
        IPostRepository Repository,
        IImageStorageService ImageStorage,
        IOperationLogger OperationLogger,
        ILogger<Program> Logger,
        EndpointConfiguration EndpointConfig,
        AuthorizationConfiguration AuthConfig);

    private sealed record UpdatePostDependencies(
        Guid Id,
        UpdatePostRequest Request,
        IValidator<UpdatePostRequest> Validator,
        IPostRepository Repository,
        IOperationLogger OperationLogger,
        HttpContext HttpContext,
        ILogger<Program> Logger,
        AuthorizationConfiguration AuthConfig,
        EndpointConfiguration EndpointConfig);

    public static void MapPostEndpoints(this WebApplication app)
    {
        var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();

        var posts = app.MapGroup(endpointConfig.Posts.Base);

        posts.MapGet(endpointConfig.Posts.GetAll, (IPostHandler handler, HttpContext ctx) => handler.GetAll(ctx));
        posts.MapGet(endpointConfig.Posts.GetById, (IPostHandler handler, Guid id) => handler.GetById(id));
        posts.MapPost(endpointConfig.Posts.Create, (IPostHandler handler, HttpContext ctx, CancellationToken ct) => handler.Create(ctx, ct))
            .RequireAuthorization()
            .DisableAntiforgery(); // Allow multipart/form-data
        posts.MapPut(endpointConfig.Posts.Update, (IPostHandler handler, Guid id, UpdatePostRequest req, HttpContext ctx) => handler.Update(id, req, ctx)).RequireAuthorization();
        posts.MapDelete(endpointConfig.Posts.Delete, (IPostHandler handler, Guid id, HttpContext ctx) => handler.Delete(id, ctx)).RequireAuthorization();
        posts.MapGet(endpointConfig.Posts.GetComments, (IPostHandler handler, Guid id) => handler.GetComments(id));
        posts.MapPost(endpointConfig.Posts.AddComment, (IPostHandler handler, Guid id, CreateCommentRequest req) => handler.AddComment(id, req));
        posts.MapPut("/{id:guid}/pin", (IPostHandler handler, Guid id, HttpContext ctx) => handler.PinPost(id, ctx)).RequireAuthorization();
        posts.MapPut("/{id:guid}/unpin", (IPostHandler handler, Guid id, HttpContext ctx) => handler.UnpinPost(id, ctx)).RequireAuthorization();
        posts.MapPost("/{id:guid}/images", (IPostHandler handler, Guid id, IFormFile file, HttpContext ctx, CancellationToken ct) => handler.AddImageToPost(id, file, ctx, ct))
            .RequireAuthorization()
            .DisableAntiforgery();
        posts.MapDelete("/{id:guid}/images", (IPostHandler handler, Guid id, [FromQuery] string imageUrl, HttpContext ctx, CancellationToken ct) => handler.RemoveImageFromPost(id, imageUrl, ctx, ct))
            .RequireAuthorization();
        posts.MapPut("/{id:guid}/tags", (IPostHandler handler, Guid id, AssignTagsRequest req, HttpContext ctx) => handler.AssignTags(id, req, ctx))
            .RequireAuthorization();
    }

    private static async Task<IResult> GetAll(
        HttpContext context,
        IPostRepository repository,
        IImageStorageService imageStorage,
        ILogger<Program> logger,
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
        
        var searchTerm = context.Request.Query["searchTerm"].FirstOrDefault();
        
        logger.LogInformation("GetAll posts: page={Page}, pageSize={PageSize}, tagIds={TagCount}, searchTerm={SearchTerm}", 
            page, pageSize, tagIds.Count, searchTerm ?? "none");
        
        var filter = new PostFilterRequest(
            tagIds.Count > 0 ? tagIds : null,
            string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm
        );
        
        var result = await repository.GetAllAsync(filter, page, pageSize);
        logger.LogInformation("GetAll returned {Count} posts (total: {Total})", result.Items.Count, result.Total);
        
        var postsWithSignedUrls = result.Items
            .Select(post => GenerateSignedUrlsForPost(post, imageStorage))
            .ToList();
        
        return Results.Ok(result with { Items = postsWithSignedUrls });
    }

    private static async Task<IResult> GetById(
        Guid id,
        IPostRepository repository,
        IImageStorageService imageStorage)
    {
        var post = await repository.GetByIdAsync(id);
        if (post is null)
            return Results.NotFound();
        
        var postWithSignedUrls = GenerateSignedUrlsForPost(post, imageStorage);
        return Results.Ok(postWithSignedUrls);
    }

    private static async Task<IResult> Create([AsParameters] CreatePostDependencies deps, CancellationToken ct)
    {
        deps.Logger.LogInformation("POST {Endpoint} called by {UserName}", deps.EndpointConfig.Posts.Base, deps.HttpContext.User.Identity?.Name);

        var authorizationResult = EnsureCreateAuthorization(deps);
        if (authorizationResult is not null)
            return authorizationResult;

        var (request, files) = await ParseCreatePostRequestAsync(deps.HttpContext, ct);

        var fileValidationResult = ValidateUploadedFiles(files, deps.Logger);
        if (fileValidationResult is not null)
            return fileValidationResult;

        var validationResult = await ValidateCreateRequestAsync(request, deps.Validator, deps.OperationLogger, ct);
        if (validationResult is not null)
            return validationResult;

        return await CreatePostWithAssetsAsync(request, files, deps, ct);
    }

    private static IResult? EnsureCreateAuthorization(CreatePostDependencies deps)
    {
        if (deps.AuthConfig.RequireAdminForPostCreate && !deps.HttpContext.User.IsInRole(SeedDataConstants.AdminRole))
        {
            deps.Logger.LogWarning("User {UserName} attempted to create post without Admin role", deps.HttpContext.User.Identity?.Name);
            return Results.Forbid();
        }

        return null;
    }

    private static async Task<IResult?> ValidateCreateRequestAsync(
        CreatePostRequest request,
        IValidator<CreatePostRequest> validator,
        IOperationLogger operationLogger,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (validationResult.IsValid)
            return null;

        operationLogger.LogValidationFailure("CreatePost", request, validationResult.Errors);
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    private static async Task<IResult> CreatePostWithAssetsAsync(
        CreatePostRequest request,
        IFormFileCollection? files,
        CreatePostDependencies deps,
        CancellationToken ct)
    {
        try
        {
            var post = await deps.Repository.CreateAsync(request);
            deps.Logger.LogInformation("Post created: {PostId}", post.Id);

            if (files is { Count: > 0 })
            {
                await UploadPostImagesAsync(post.Id, files, deps.Repository, deps.ImageStorage, deps.Logger, ct);
            }

            var updatedPost = await deps.Repository.GetByIdAsync(post.Id);
            var postWithSignedUrls = GenerateSignedUrlsForPost(updatedPost!, deps.ImageStorage);

            return Results.Created($"{deps.EndpointConfig.Posts.Base}/{post.Id}", postWithSignedUrls);
        }
        catch (Exception ex)
        {
            deps.Logger.LogError(ex, "Error creating post");
            return Results.Problem("Failed to create post");
        }
    }

    private static async Task<(CreatePostRequest request, IFormFileCollection? files)> ParseCreatePostRequestAsync(
        HttpContext context,
        CancellationToken ct)
    {
        if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync(ct);
            var request = new CreatePostRequest(
                form["title"].ToString(),
                form["content"].ToString(),
                form["author"].ToString());
            return (request, form.Files);
        }

        var jsonRequest = await context.Request.ReadFromJsonAsync<CreatePostRequest>(ct)
            ?? throw new InvalidOperationException("Invalid request body");
        return (jsonRequest, null);
    }

    private static IResult? ValidateUploadedFiles(IFormFileCollection? files, ILogger<Program> logger)
    {
        if (files is null or { Count: 0 })
            return null;

        const long maxFileSize = 10 * 1024 * 1024; // 10 MB
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > maxFileSize)
            {
                logger.LogWarning("File {FileName} exceeds size limit: {Size} bytes", file.FileName, file.Length);
                return Results.BadRequest(new { error = $"File {file.FileName} exceeds 10 MB limit" });
            }

            if (!allowedTypes.Contains(file.ContentType?.ToLowerInvariant()))
            {
                logger.LogWarning("File {FileName} has invalid type: {ContentType}", file.FileName, file.ContentType);
                return Results.BadRequest(new { error = $"File {file.FileName} has invalid type. Allowed: JPEG, PNG, GIF, WebP" });
            }
        }

        return null;
    }

    private static async Task UploadPostImagesAsync(
        Guid postId,
        IFormFileCollection files,
        IPostRepository repository,
        IImageStorageService imageStorage,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var uploadedCount = 0;
        foreach (var file in files.Where(f => f.Length > 0))
        {
            try
            {
                await using var stream = file.OpenReadStream();
                var imageUrl = await imageStorage.UploadImageAsync(stream, file.FileName, "posts", ct);
                await repository.AddImageAsync(postId, imageUrl);
                uploadedCount++;

                logger.LogInformation(
                    "Image {Count}/{Total} uploaded for post {PostId}: {FileName}",
                    uploadedCount, files.Count, postId, file.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to upload image {FileName} for post {PostId}", file.FileName, postId);
            }
        }

        if (uploadedCount > 0)
            logger.LogInformation("Post {PostId} created with {Count} images", postId, uploadedCount);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdatePostRequest request,
        IValidator<UpdatePostRequest> validator,
        IPostRepository repository,
        IOperationLogger operationLogger,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig,
        EndpointConfiguration endpointConfig)
    {
        var deps = new UpdatePostDependencies(id, request, validator, repository, operationLogger, context, logger, authConfig, endpointConfig);
        return await PerformPostUpdateAsync(deps);
    }

    private static async Task<IResult> PerformPostUpdateAsync(UpdatePostDependencies deps)
    {
        // Require admin role to update posts
        if (deps.AuthConfig.RequireAdminForPostUpdate && !deps.HttpContext.User.IsInRole(SeedDataConstants.AdminRole))
        {
            deps.Logger.LogWarning("User {UserName} attempted to update post without Admin role", deps.HttpContext.User.Identity?.Name);
            return Results.Forbid();
        }

        // Validate request using FluentValidation
        var validationResult = await deps.Validator.ValidateAsync(deps.Request);
        if (!validationResult.IsValid)
        {
            deps.OperationLogger.LogValidationFailure("UpdatePost", deps.Request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var updated = await deps.Repository.UpdateAsync(deps.Id, deps.Request);
        if (updated is null)
        {
            deps.Logger.LogWarning("Update attempt for non-existent post: {PostId}", deps.Id);
            return Results.NotFound();
        }
        
        deps.Logger.LogInformation("Post updated: {PostId}", deps.Id);
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
        if (authConfig.RequireAdminForPostDelete && !context.User.IsInRole(SeedDataConstants.AdminRole))
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
        ILogger<Program> logger)
    {
        // Require admin role to pin posts
        if (!context.User.IsInRole(SeedDataConstants.AdminRole))
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
        ILogger<Program> logger)
    {
        // Require admin role to unpin posts
        if (!context.User.IsInRole(SeedDataConstants.AdminRole))
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

    private static async Task<IResult> AddImageToPost(
        Guid id,
        IFormFile file,
        IPostRepository repository,
        IImageStorageService imageStorage,
        HttpContext context,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation("POST /posts/{PostId}/images called by {UserName}", id, context.User.Identity?.Name);

        // Require admin role
        if (!context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            logger.LogWarning("User {UserName} attempted to add image to post without Admin role", context.User.Identity?.Name);
            return Results.Forbid();
        }

        // Validate file
        if (file.Length == 0)
            return Results.BadRequest(new { error = "File is empty" });

        if (file.Length > 10 * 1024 * 1024) // 10 MB limit for post images
            return Results.BadRequest(new { error = "File size cannot exceed 10 MB" });

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return Results.BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, GIF, WebP" });

        try
        {
            // Upload image to Cloudinary
            await using var stream = file.OpenReadStream();
            var imageUrl = await imageStorage.UploadImageAsync(stream, file.FileName, "posts", ct);

            // Add image URL to post
            var updatedPost = await repository.AddImageAsync(id, imageUrl);
            if (updatedPost is null)
            {
                logger.LogWarning("Add image attempt for non-existent post: {PostId}", id);
                return Results.NotFound(new { error = "Post not found" });
            }

            logger.LogInformation("Image added to post {PostId} by {UserName}: {ImageUrl}", 
                id, context.User.Identity?.Name, imageUrl);
            
            var postWithSignedUrls = GenerateSignedUrlsForPost(updatedPost, imageStorage);
            return Results.Ok(postWithSignedUrls);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding image to post {PostId}", id);
            return Results.Problem("Failed to add image to post");
        }
    }

    private static async Task<IResult> RemoveImageFromPost(
        Guid id,
        [FromQuery] string imageUrl,
        IPostRepository repository,
        IImageStorageService imageStorage,
        HttpContext context,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation("DELETE /posts/{PostId}/images called by {UserName}", id, context.User.Identity?.Name);

        // Require admin role
        if (!context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            logger.LogWarning("User {UserName} attempted to remove image from post without Admin role", context.User.Identity?.Name);
            return Results.Forbid();
        }

        try
        {
            // Remove image from post
            var updatedPost = await repository.RemoveImageAsync(id, imageUrl);
            if (updatedPost is null)
            {
                logger.LogWarning("Remove image attempt for non-existent post: {PostId}", id);
                return Results.NotFound(new { error = "Post not found" });
            }

            // Delete image from Cloudinary
            await imageStorage.DeleteImageAsync(imageUrl, ct);

            logger.LogInformation("Image removed from post {PostId} by {UserName}: {ImageUrl}", 
                id, context.User.Identity?.Name, imageUrl);
            
            var postWithSignedUrls = GenerateSignedUrlsForPost(updatedPost, imageStorage);
            return Results.Ok(postWithSignedUrls);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing image from post {PostId}", id);
            return Results.Problem("Failed to remove image from post");
        }
    }

    private static async Task<IResult> AssignTags(
        Guid id,
        AssignTagsRequest request,
        IPostRepository repository,
        IImageStorageService imageStorage,
        HttpContext context,
        ILogger<Program> logger)
    {
        try
        {
            var post = await repository.AssignTagsAsync(id, request.TagIds);
            if (post is null)
                return Results.NotFound($"Post with ID {id} not found");

            logger.LogInformation("Tags assigned to post {PostId} by {UserName}: {TagCount} tags", 
                id, context.User.Identity?.Name, request.TagIds.Count);
            
            var postWithSignedUrls = GenerateSignedUrlsForPost(post, imageStorage);
            return Results.Ok(postWithSignedUrls);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning tags to post {PostId}", id);
            return Results.Problem("Failed to assign tags to post");
        }
    }

    private static Post GenerateSignedUrlsForPost(Post post, IImageStorageService imageStorage)

    {
        if (post.ImageUrls.Count == 0)
            return post;

        var signedUrls = post.ImageUrls
            .Select(url => imageStorage.GenerateSignedUrl(url, expirationMinutes: 60))
            .ToList();

        return post with { ImageUrls = signedUrls };
    }
}
