using FluentValidation;
using Microsoft.AspNetCore.Mvc;
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
        posts.MapPost(endpointConfig.Posts.Create, Create)
            .RequireAuthorization()
            .DisableAntiforgery(); // Allow multipart/form-data
        posts.MapPut(endpointConfig.Posts.Update, Update).RequireAuthorization();
        posts.MapDelete(endpointConfig.Posts.Delete, Delete).RequireAuthorization();
        posts.MapGet(endpointConfig.Posts.GetComments, GetComments);
        posts.MapPost(endpointConfig.Posts.AddComment, AddComment);
        posts.MapPut("/{id:guid}/pin", PinPost).RequireAuthorization();
        posts.MapPut("/{id:guid}/unpin", UnpinPost).RequireAuthorization();
        posts.MapPost("/{id:guid}/images", AddImageToPost)
            .RequireAuthorization()
            .DisableAntiforgery();
        posts.MapDelete("/{id:guid}/images", RemoveImageFromPost)
            .RequireAuthorization();
        posts.MapPut("/{id:guid}/tags", AssignTags)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetAll(
        IPostRepository repository,
        IImageStorageService imageStorage,
        int page = 1,
        int pageSize = 10)
    {
        var result = await repository.GetAllAsync(page, pageSize);
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

    private static async Task<IResult> Create(
        HttpContext context,
        IValidator<CreatePostRequest> validator,
        IPostRepository repository,
        IImageStorageService imageStorage,
        IOperationLogger operationLogger,
        ILogger<Program> logger,
        EndpointConfiguration endpointConfig,
        AuthorizationConfiguration authConfig,
        CancellationToken ct)
    {
        logger.LogInformation("POST {Endpoint} called by {UserName}", endpointConfig.Posts.Base, context.User.Identity?.Name);
        
        // Require admin role to create posts
        if (authConfig.RequireAdminForPostCreate && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            logger.LogWarning("User {UserName} attempted to create post without Admin role", context.User.Identity?.Name);
            return Results.Forbid();
        }

        CreatePostRequest request;
        IFormFileCollection? files = null;

        // Check if it's multipart (with files) or JSON
        if (context.Request.HasFormContentType)
        {
            // Parse form data
            var form = await context.Request.ReadFormAsync(ct);
            var title = form["title"].ToString();
            var content = form["content"].ToString();
            var author = form["author"].ToString();
            files = form.Files;
            
            request = new CreatePostRequest(title, content, author);
            
            // Validate files if present
            if (files.Count > 0)
            {
                const long maxFileSize = 10 * 1024 * 1024; // 10 MB
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                
                foreach (var file in files)
                {
                    if (file.Length == 0)
                        continue;
                        
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
            }
        }
        else
        {
            // JSON request (backwards compatibility)
            request = await context.Request.ReadFromJsonAsync<CreatePostRequest>(ct) 
                ?? throw new InvalidOperationException("Invalid request body");
        }

        // Validate request using FluentValidation
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            operationLogger.LogValidationFailure("CreatePost", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            // Create post first
            var post = await repository.CreateAsync(request);
            logger.LogInformation("Post created: {PostId}", post.Id);

            // Upload images if present
            if (files is not null && files.Count > 0)
            {
                var uploadedCount = 0;
                foreach (var file in files)
                {
                    if (file.Length == 0)
                        continue;
                        
                    try
                    {
                        await using var stream = file.OpenReadStream();
                        var imageUrl = await imageStorage.UploadImageAsync(stream, file.FileName, "posts", ct);
                        
                        await repository.AddImageAsync(post.Id, imageUrl);
                        uploadedCount++;
                        
                        logger.LogInformation("Image {Count}/{Total} uploaded for post {PostId}: {FileName}", 
                            uploadedCount, files.Count, post.Id, file.FileName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to upload image {FileName} for post {PostId}", file.FileName, post.Id);
                        // Continue with other images - don't fail entire operation
                    }
                }
                
                if (uploadedCount > 0)
                {
                    logger.LogInformation("Post {PostId} created with {Count} images", post.Id, uploadedCount);
                }
            }

            // Fetch updated post with images and generate signed URLs
            var updatedPost = await repository.GetByIdAsync(post.Id);
            var postWithSignedUrls = GenerateSignedUrlsForPost(updatedPost!, imageStorage);
            
            return Results.Created($"{endpointConfig.Posts.Base}/{post.Id}", postWithSignedUrls);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating post");
            return Results.Problem("Failed to create post");
        }
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdatePostRequest request,
        IValidator<UpdatePostRequest> validator,
        IPostRepository repository,
        IOperationLogger operationLogger,
        HttpContext context,
        ILogger<Program> logger,
        AuthorizationConfiguration authConfig)
    {
        // Require admin role to update posts
        if (authConfig.RequireAdminForPostUpdate && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            logger.LogWarning("User {UserName} attempted to update post without Admin role", context.User.Identity?.Name);
            return Results.Forbid();
        }

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
