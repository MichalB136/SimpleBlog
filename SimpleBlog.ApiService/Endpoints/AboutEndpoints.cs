using Microsoft.AspNetCore.Authorization;
using SimpleBlog.Common.Interfaces;
using SimpleBlog.Common.Logging;
using SimpleBlog.Common.Models;

namespace SimpleBlog.ApiService.Endpoints;

public static class AboutEndpoints
{
    public static void MapAboutEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/about")
            .WithTags("About")
            .WithOpenApi();

        group.MapPost("/image", UploadAboutImage)
            .WithName("UploadAboutImage")
            .WithDescription("Upload about section image (admin only)")
            .RequireAuthorization("AdminOnly")
            .DisableAntiforgery()
            .Produces<AboutMe>()
            .Produces(400)
            .Produces(401)
            .Produces(403);

        group.MapDelete("/image", DeleteAboutImage)
            .WithName("DeleteAboutImage")
            .WithDescription("Delete about section image (admin only)")
            .RequireAuthorization("AdminOnly")
            .Produces<AboutMe>()
            .Produces(401)
            .Produces(403);
    }

    private static async Task<IResult> UploadAboutImage(
        IFormFile file,
        IAboutMeRepository aboutRepository,
        IImageStorageService imageStorage,
        HttpContext context,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation("POST /about/image called by {UserName}", PiiMask.MaskUserName(context.User.Identity?.Name));

        if (file.Length == 0)
            return Results.BadRequest(new { error = "File is empty" });

        if (file.Length > 10 * 1024 * 1024) // 10 MB limit
            return Results.BadRequest(new { error = "File size cannot exceed 10 MB" });

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return Results.BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, GIF, WebP" });

        try
        {
            var currentAbout = await aboutRepository.GetAsync();
            if (currentAbout?.ImageUrl is not null)
            {
                await imageStorage.DeleteImageAsync(currentAbout.ImageUrl, ct);
                logger.LogInformation("Deleted old about image: {OldImageUrl}", currentAbout.ImageUrl);
            }

            await using var stream = file.OpenReadStream();
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"about-{Guid.NewGuid():N}{extension}";
            var imageUrl = await imageStorage.UploadImageAsync(stream, uniqueFileName, "about", ct);

            var username = context.User.Identity?.Name ?? "Unknown";
            var maskedUsername = PiiMask.MaskUserName(username);
            var updated = await aboutRepository.UpdateImageAsync(imageUrl, username);

            var settingsWithSignedUrl = updated with
            {
                ImageUrl = updated.ImageUrl is not null
                    ? imageStorage.GenerateSignedUrl(updated.ImageUrl, expirationMinutes: 60)
                    : null
            };

            logger.LogInformation("About image uploaded successfully by {UserName}: {ImageUrl}", maskedUsername, imageUrl);
            return Results.Ok(settingsWithSignedUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading about image");
            return Results.Problem("Failed to upload about image");
        }
    }

    private static async Task<IResult> DeleteAboutImage(
        IAboutMeRepository aboutRepository,
        IImageStorageService imageStorage,
        HttpContext context,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation("DELETE /about/image called by {UserName}", PiiMask.MaskUserName(context.User.Identity?.Name));

        var currentAbout = await aboutRepository.GetAsync();
        if (currentAbout?.ImageUrl is null)
            return Results.NotFound(new { error = "No about image to delete" });

        try
        {
            await imageStorage.DeleteImageAsync(currentAbout.ImageUrl, ct);
            logger.LogInformation("Deleted about image: {ImageUrl}", currentAbout.ImageUrl);

            var username = context.User.Identity?.Name ?? "Unknown";
            var maskedUsername = PiiMask.MaskUserName(username);
            var updated = await aboutRepository.UpdateImageAsync(null, username);

            logger.LogInformation("About image deleted successfully by {UserName}", maskedUsername);
            return Results.Ok(updated);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting about image");
            return Results.Problem("Failed to delete about image");
        }
    }
}
