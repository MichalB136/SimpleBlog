using FluentValidation;
using SimpleBlog.ApiService;
using SimpleBlog.Common.Interfaces;
using SimpleBlog.Common.Logging;
using SimpleBlog.Common.Models;

namespace SimpleBlog.ApiService.Endpoints;

public static class AboutMeEndpoints
{
    public static void MapAboutMeEndpoints(this WebApplication app)
    {
        var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();

        var aboutMe = app.MapGroup(endpointConfig.AboutMe.Base);

        aboutMe.MapGet(endpointConfig.AboutMe.Get, Get);
        aboutMe.MapPut(endpointConfig.AboutMe.Update, Update).RequireAuthorization();
    }

    private static async Task<IResult> Get(
        IAboutMeRepository repository,
        IImageStorageService imageStorage)
    {
        var aboutMe = await repository.GetAsync();
        if (aboutMe is null)
        {
            return Results.NotFound();
        }

        var aboutWithSignedUrl = aboutMe with
        {
            ImageUrl = aboutMe.ImageUrl is not null
                ? imageStorage.GenerateSignedUrl(aboutMe.ImageUrl, expirationMinutes: 60)
                : null
        };

        return Results.Ok(aboutWithSignedUrl);
    }

    private static async Task<IResult> Update(
        UpdateAboutMeRequest request,
        IValidator<UpdateAboutMeRequest> validator,
        IAboutMeRepository repository,
        IImageStorageService imageStorage,
        IOperationLogger operationLogger,
        HttpContext context,
        ILogger<Program> logger)
    {
        logger.LogInformation("PUT /aboutme called by {UserName}", PiiMask.MaskUserName(context.User.Identity?.Name));
        
        // Require admin role to update AboutMe
        if (!context.User.IsInRole(SeedDataConstants.AdminRole))
        {
            logger.LogWarning("User {UserName} attempted to update AboutMe without Admin role", PiiMask.MaskUserName(context.User.Identity?.Name));
            return Results.Forbid();
        }

        // Validate request using FluentValidation
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            operationLogger.LogValidationFailure("UpdateAboutMe", request, validationResult.Errors);
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var username = context.User.Identity?.Name ?? "Unknown";
        var maskedUsername = PiiMask.MaskUserName(username);
        var updated = await repository.UpdateAsync(request, username);

        var updatedWithSignedUrl = updated with
        {
            ImageUrl = updated.ImageUrl is not null
                ? imageStorage.GenerateSignedUrl(updated.ImageUrl, expirationMinutes: 60)
                : null
        };
        
        logger.LogInformation("AboutMe updated by {UserName}", maskedUsername);
        return Results.Ok(updatedWithSignedUrl);
    }
}
