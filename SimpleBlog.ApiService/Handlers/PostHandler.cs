using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SimpleBlog.Common;
using SimpleBlog.Common.Logging;

namespace SimpleBlog.ApiService.Handlers;

public sealed class PostHandler : IPostHandler
{
    private readonly IPostRepository _repository;
    private readonly IImageStorageService _imageStorage;
    private readonly IValidator<CreatePostRequest> _createValidator;
    private readonly IValidator<UpdatePostRequest> _updateValidator;
    private readonly IValidator<CreateCommentRequest> _commentValidator;
    private readonly IOperationLogger _operationLogger;
    private readonly ILogger<PostHandler> _logger;
    private readonly EndpointConfiguration _endpointConfig;
    private readonly AuthorizationConfiguration _authConfig;

    public PostHandler(
        IPostRepository repository,
        IImageStorageService imageStorage,
        IValidator<CreatePostRequest> createValidator,
        IValidator<UpdatePostRequest> updateValidator,
        IValidator<CreateCommentRequest> commentValidator,
        IOperationLogger operationLogger,
        ILogger<PostHandler> logger,
        EndpointConfiguration endpointConfig,
        AuthorizationConfiguration authConfig)
    {
        _repository = repository;
        _imageStorage = imageStorage;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _commentValidator = commentValidator;
        _operationLogger = operationLogger;
        _logger = logger;
        _endpointConfig = endpointConfig;
        _authConfig = authConfig;
    }

    public async Task<IResult> GetAll(HttpContext context, int page = 1, int pageSize = 10)
    {
        var tagIds = context.Request.Query["tagIds"]
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => Guid.TryParse(s, out var guid) ? guid : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();

        var searchTerm = context.Request.Query["searchTerm"].FirstOrDefault();

        _logger.LogInformation("GetAll posts: page={Page}, pageSize={PageSize}, tagIds={TagCount}, searchTerm={SearchTerm}",
            page, pageSize, tagIds.Count, searchTerm ?? "none");

        var filter = new PostFilterRequest(
            tagIds.Count > 0 ? tagIds : null,
            string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm
        );

        var result = await _repository.GetAllAsync(filter, page, pageSize);
        _logger.LogInformation("GetAll returned {Count} posts (total: {Total})", result.Items.Count, result.Total);

        var postsWithSignedUrls = result.Items
            .Select(post => GenerateSignedUrlsForPost(post))
            .ToList();

        return Results.Ok(result with { Items = postsWithSignedUrls });
    }

    public async Task<IResult> GetById(Guid id)
    {
        var post = await _repository.GetByIdAsync(id);
        if (post is null)
            return Results.NotFound();

        var postWithSignedUrls = GenerateSignedUrlsForPost(post);
        return Results.Ok(postWithSignedUrls);
    }

    public async Task<IResult> Create(HttpContext context, CancellationToken ct)
    {
        _logger.LogInformation("POST {Endpoint} called by {UserName}", _endpointConfig.Posts.Base, PiiMask.MaskUserName(context.User.Identity?.Name));

        if (_authConfig.RequireAdminForPostCreate && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("User {UserName} attempted to create post without Admin role", PiiMask.MaskUserName(context.User.Identity?.Name));
            return Results.Forbid();
        }

        var (request, files) = await ParseCreatePostRequestAsync(context, ct);

        var fileValidationResult = ValidateUploadedFiles(files);
        if (fileValidationResult is not null)
            return fileValidationResult;

        var validationResult = await _createValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            _operationLogger.LogValidationFailure("CreatePost", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var post = await _repository.CreateAsync(request);
            _logger.LogInformation("Post created: {PostId}", post.Id);

            if (files is { Count: > 0 })
            {
                await UploadPostImagesAsync(post.Id, files, ct);
            }

            var updatedPost = await _repository.GetByIdAsync(post.Id);
            var postWithSignedUrls = GenerateSignedUrlsForPost(updatedPost!);

            return Results.Created($"{_endpointConfig.Posts.Base}/{post.Id}", postWithSignedUrls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            return Results.Problem("Failed to create post");
        }
    }

    public async Task<IResult> Update(Guid id, UpdatePostRequest request, HttpContext context)
    {
        if (_authConfig.RequireAdminForPostUpdate && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("User {UserName} attempted to update post without Admin role", PiiMask.MaskUserName(context.User.Identity?.Name));
            return Results.Forbid();
        }

        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _operationLogger.LogValidationFailure("UpdatePost", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var updated = await _repository.UpdateAsync(id, request);
        if (updated is null)
        {
            _logger.LogWarning("Update attempt for non-existent post: {PostId}", id);
            return Results.NotFound();
        }

        _logger.LogInformation("Post updated: {PostId}", id);
        return Results.Ok(updated);
    }

    public async Task<IResult> Delete(Guid id, HttpContext context)
    {
        if (_authConfig.RequireAdminForPostDelete && !context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("Unauthorized delete attempt for post: {PostId}", id);
            return Results.Forbid();
        }

        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Delete attempt for non-existent post: {PostId}", id);
            return Results.NotFound();
        }

        _logger.LogInformation("Post deleted: {PostId}", id);
        return Results.NoContent();
    }

    public async Task<IResult> GetComments(Guid id)
    {
        var comments = await _repository.GetCommentsAsync(id);
        return comments is not null ? Results.Ok(comments) : Results.NotFound();
    }

    public async Task<IResult> AddComment(Guid id, CreateCommentRequest request)
    {
        var validationResult = await _commentValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _operationLogger.LogValidationFailure("AddComment", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var created = await _repository.AddCommentAsync(id, request);
        if (created is null)
        {
            _logger.LogWarning("Comment creation attempt for non-existent post: {PostId}", id);
            return Results.NotFound();
        }

        _logger.LogInformation("Comment added to post: {PostId}", id);
        return Results.Created($"{_endpointConfig.Posts.Base}/{id}/comments/{created.Id}", created);
    }

    public async Task<IResult> PinPost(Guid id, HttpContext context)
    {
        if (!context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("User {UserName} attempted to pin post without Admin role", PiiMask.MaskUserName(context.User.Identity?.Name));
            return Results.Forbid();
        }

        var pinned = await _repository.SetPinnedAsync(id, true);
        if (pinned is null)
        {
            _logger.LogWarning("Pin attempt for non-existent post: {PostId}", id);
            return Results.NotFound();
        }

        _logger.LogInformation("Post pinned: {PostId} by {UserName}", id, PiiMask.MaskUserName(context.User.Identity?.Name));
        return Results.Ok(pinned);
    }

    public async Task<IResult> UnpinPost(Guid id, HttpContext context)
    {
        if (!context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("User {UserName} attempted to unpin post without Admin role", PiiMask.MaskUserName(context.User.Identity?.Name));
            return Results.Forbid();
        }

        var unpinned = await _repository.SetPinnedAsync(id, false);
        if (unpinned is null)
        {
            _logger.LogWarning("Unpin attempt for non-existent post: {PostId}", id);
            return Results.NotFound();
        }

        _logger.LogInformation("Post unpinned: {PostId} by {UserName}", id, PiiMask.MaskUserName(context.User.Identity?.Name));
        return Results.Ok(unpinned);
    }

    public async Task<IResult> AddImageToPost(Guid id, IFormFile file, HttpContext context, CancellationToken ct)
    {
        _logger.LogInformation("POST /posts/{PostId}/images called by {UserName}", id, PiiMask.MaskUserName(context.User.Identity?.Name));

        if (!context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("User {UserName} attempted to add image to post without Admin role", PiiMask.MaskUserName(context.User.Identity?.Name));
            return Results.Forbid();
        }

        if (file.Length == 0)
            return Results.BadRequest(new { error = "File is empty" });

        if (file.Length > 10 * 1024 * 1024)
            return Results.BadRequest(new { error = "File size cannot exceed 10 MB" });

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType?.ToLowerInvariant()))
            return Results.BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, GIF, WebP" });

        try
        {
            await using var stream = file.OpenReadStream();
            var imageUrl = await _imageStorage.UploadImageAsync(stream, file.FileName, "posts", ct);

            var updatedPost = await _repository.AddImageAsync(id, imageUrl);
            if (updatedPost is null)
            {
                _logger.LogWarning("Add image attempt for non-existent post: {PostId}", id);
                return Results.NotFound(new { error = "Post not found" });
            }

            _logger.LogInformation("Image added to post {PostId} by {UserName}: {ImageUrl}", id, PiiMask.MaskUserName(context.User.Identity?.Name), imageUrl);

            var postWithSignedUrls = GenerateSignedUrlsForPost(updatedPost);
            return Results.Ok(postWithSignedUrls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding image to post {PostId}", id);
            return Results.Problem("Failed to add image to post");
        }
    }

    public async Task<IResult> RemoveImageFromPost(Guid id, string imageUrl, HttpContext context, CancellationToken ct)
    {
        _logger.LogInformation("DELETE /posts/{PostId}/images called by {UserName}", id, PiiMask.MaskUserName(context.User.Identity?.Name));

        if (!context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            _logger.LogWarning("User {UserName} attempted to remove image from post without Admin role", PiiMask.MaskUserName(context.User.Identity?.Name));
            return Results.Forbid();
        }

        try
        {
            var updatedPost = await _repository.RemoveImageAsync(id, imageUrl);
            if (updatedPost is null)
            {
                _logger.LogWarning("Remove image attempt for non-existent post: {PostId}", id);
                return Results.NotFound(new { error = "Post not found" });
            }

            await _imageStorage.DeleteImageAsync(imageUrl, ct);

            _logger.LogInformation("Image removed from post {PostId} by {UserName}: {ImageUrl}", id, PiiMask.MaskUserName(context.User.Identity?.Name), imageUrl);

            var postWithSignedUrls = GenerateSignedUrlsForPost(updatedPost);
            return Results.Ok(postWithSignedUrls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing image from post {PostId}", id);
            return Results.Problem("Failed to remove image from post");
        }
    }

    public async Task<IResult> AssignTags(Guid id, AssignTagsRequest request, HttpContext context)
    {
        try
        {
            var post = await _repository.AssignTagsAsync(id, request.TagIds);
            if (post is null)
                return Results.NotFound($"Post with ID {id} not found");

            _logger.LogInformation("Tags assigned to post {PostId} by {UserName}: {TagCount} tags", id, PiiMask.MaskUserName(context.User.Identity?.Name), request.TagIds.Count);

            var postWithSignedUrls = GenerateSignedUrlsForPost(post);
            return Results.Ok(postWithSignedUrls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning tags to post {PostId}", id);
            return Results.Problem("Failed to assign tags to post");
        }
    }

    // Helpers
    private Post GenerateSignedUrlsForPost(Post post)
    {
        if (post.ImageUrls.Count == 0)
            return post;

        var signedUrls = post.ImageUrls
            .Select(url => _imageStorage.GenerateSignedUrl(url, expirationMinutes: 60))
            .ToList();

        return post with { ImageUrls = signedUrls };
    }

    private static IResult? ValidateUploadedFiles(IFormFileCollection? files)
    {
        if (files is null or { Count: 0 })
            return null;

        const long maxFileSize = 10 * 1024 * 1024; // 10 MB
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > maxFileSize)
            {
                return Results.BadRequest(new { error = $"File {file.FileName} exceeds 10 MB limit" });
            }

            if (!allowedTypes.Contains(file.ContentType?.ToLowerInvariant()))
            {
                return Results.BadRequest(new { error = $"File {file.FileName} has invalid type. Allowed: JPEG, PNG, GIF, WebP" });
            }
        }

        return null;
    }

    private static async Task<(CreatePostRequest request, IFormFileCollection? files)> ParseCreatePostRequestAsync(HttpContext context, CancellationToken ct)
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

    private async Task UploadPostImagesAsync(Guid postId, IFormFileCollection files, CancellationToken ct)
    {
        var uploadedCount = 0;
        foreach (var file in files.Where(f => f.Length > 0))
        {
            try
            {
                await using var stream = file.OpenReadStream();
                var imageUrl = await _imageStorage.UploadImageAsync(stream, file.FileName, "posts", ct);
                await _repository.AddImageAsync(postId, imageUrl);
                uploadedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image {FileName} for post {PostId}", file.FileName, postId);
            }
        }

        if (uploadedCount > 0)
            _logger.LogInformation("Post {PostId} created with {Count} images", postId, uploadedCount);
    }
}
