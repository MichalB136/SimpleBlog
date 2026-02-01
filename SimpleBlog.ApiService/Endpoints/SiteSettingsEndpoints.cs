using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using SimpleBlog.Common.Interfaces;
using SimpleBlog.Common.Logging;
using SimpleBlog.Common.Models;

namespace SimpleBlog.ApiService.Endpoints;

public static class SiteSettingsEndpoints
{
    public static void MapSiteSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/site-settings")
            .WithTags("Site Settings")
            .WithOpenApi();

        group.MapGet("/", GetSiteSettings)
            .WithName("GetSiteSettings")
            .WithDescription("Get current site settings")
            .Produces<SiteSettings>()
            .Produces(404);

        group.MapPut("/", UpdateSiteSettings)
            .WithName("UpdateSiteSettings")
            .WithDescription("Update site settings (admin only)")
            .RequireAuthorization("AdminOnly")
            .Produces<SiteSettings>()
            .Produces(400)
            .Produces(401)
            .Produces(403);

        group.MapGet("/themes", GetAvailableThemes)
            .WithName("GetAvailableThemes")
            .WithDescription("Get list of available themes")
            .Produces<IReadOnlyList<string>>();

        group.MapPost("/logo", UploadLogo)
            .WithName("UploadLogo")
            .WithDescription("Upload site logo (admin only)")
            .RequireAuthorization("AdminOnly")
            .DisableAntiforgery()
            .Produces<SiteSettings>()
            .Produces(400)
            .Produces(401)
            .Produces(403);

        group.MapDelete("/logo", DeleteLogo)
            .WithName("DeleteLogo")
            .WithDescription("Delete site logo (admin only)")
            .RequireAuthorization("AdminOnly")
            .Produces<SiteSettings>()
            .Produces(401)
            .Produces(403);
    }

    private static async Task<IResult> GetSiteSettings(
        ISiteSettingsRepository repository,
        IImageStorageService imageStorage,
        CancellationToken ct)
    {
        var settings = await repository.GetAsync(ct);

        if (settings is null)
        {
            // Return default settings if none exist
            return Results.Ok(new SiteSettings(
                Guid.Empty,
                ThemeNames.Light,
                null,
                null,
                DateTimeOffset.UtcNow,
                "System"
            ));
        }

        // Generate signed URL for private logo if exists
        var settingsWithSignedUrl = settings with
        {
            LogoUrl = settings.LogoUrl is not null
                ? imageStorage.GenerateSignedUrl(settings.LogoUrl, expirationMinutes: 60)
                : null
        };

        return Results.Ok(settingsWithSignedUrl);
    }

    private static async Task<IResult> UpdateSiteSettings(
        UpdateSiteSettingsRequest request,
        ISiteSettingsRepository repository,
        IValidator<UpdateSiteSettingsRequest> validator,
        IOperationLogger operationLogger,
        HttpContext context,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation("PUT /site-settings called by {UserName}", PiiMask.MaskUserName(context.User.Identity?.Name));

        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            operationLogger.LogValidationFailure("UpdateSiteSettings", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var username = context.User.Identity?.Name ?? "Unknown";
        var maskedUsername = PiiMask.MaskUserName(username);
        var settings = await repository.UpdateAsync(request.Theme, request.ContactText, username, ct);

        logger.LogInformation("Site settings updated to theme '{Theme}' by {UserName}", request.Theme, maskedUsername);
        return Results.Ok(settings);
    }

    private static IResult GetAvailableThemes()
    {
        return Results.Ok(ThemeNames.All);
    }

    private static async Task<IResult> UploadLogo(
        IFormFile file,
        ISiteSettingsRepository repository,
        IImageStorageService imageStorage,
        HttpContext context,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation("POST /site-settings/logo called by {UserName}", PiiMask.MaskUserName(context.User.Identity?.Name));

        // Validate file
        if (file.Length == 0)
            return Results.BadRequest(new { error = "File is empty" });

        if (file.Length > 5 * 1024 * 1024) // 5 MB limit
            return Results.BadRequest(new { error = "File size cannot exceed 5 MB" });

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return Results.BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, GIF, WebP" });

        try
        {
            // Get current settings to delete old logo if exists
            var currentSettings = await repository.GetAsync(ct);
            if (currentSettings?.LogoUrl is not null)
            {
                await imageStorage.DeleteImageAsync(currentSettings.LogoUrl, ct);
                logger.LogInformation("Deleted old logo: {OldLogoUrl}", currentSettings.LogoUrl);
            }

            // Upload new logo with fixed name 'logo'
            await using var stream = file.OpenReadStream();
            var logoUrl = await imageStorage.UploadImageAsync(stream, "logo", "logos", ct);

            var username = context.User.Identity?.Name ?? "Unknown";
            var maskedUsername = PiiMask.MaskUserName(username);
            var settings = await repository.UpdateLogoAsync(logoUrl, username, ct);

            // Generate signed URL for response
            var settingsWithSignedUrl = settings with
            {
                LogoUrl = settings.LogoUrl is not null
                    ? imageStorage.GenerateSignedUrl(settings.LogoUrl, expirationMinutes: 60)
                    : null
            };

            logger.LogInformation("Logo uploaded successfully by {UserName}: {LogoUrl}", maskedUsername, logoUrl);
            return Results.Ok(settingsWithSignedUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading logo");
            return Results.Problem("Failed to upload logo");
        }
    }

    private static async Task<IResult> DeleteLogo(
        ISiteSettingsRepository repository,
        IImageStorageService imageStorage,
        HttpContext context,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation("DELETE /site-settings/logo called by {UserName}", PiiMask.MaskUserName(context.User.Identity?.Name));

        var currentSettings = await repository.GetAsync(ct);
        if (currentSettings?.LogoUrl is null)
            return Results.NotFound(new { error = "No logo to delete" });

        try
        {
            await imageStorage.DeleteImageAsync(currentSettings.LogoUrl, ct);
            logger.LogInformation("Deleted logo: {LogoUrl}", currentSettings.LogoUrl);

            var username = context.User.Identity?.Name ?? "Unknown";
            var maskedUsername = PiiMask.MaskUserName(username);
            var settings = await repository.UpdateLogoAsync(null, username, ct);

            logger.LogInformation("Logo deleted successfully by {UserName}", maskedUsername);
            return Results.Ok(settings); // No logo, so no signed URL needed
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting logo");
            return Results.Problem("Failed to delete logo");
        }
    }
}
