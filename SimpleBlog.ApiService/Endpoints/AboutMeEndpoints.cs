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

    private static async Task<IResult> Get(IAboutMeRepository repository)
    {
        var aboutMe = await repository.GetAsync();
        return aboutMe is not null ? Results.Ok(aboutMe) : Results.NotFound();
    }

    private static async Task<IResult> Update(
        UpdateAboutMeRequest request,
        IValidator<UpdateAboutMeRequest> validator,
        IAboutMeRepository repository,
        IOperationLogger operationLogger,
        HttpContext context,
        ILogger<Program> logger)
    {
        logger.LogInformation("PUT /aboutme called by {UserName}", context.User.Identity?.Name);
        
        // Require admin role to update AboutMe
        if (!context.User.IsInRole(SeedDataConstants.AdminUsername))
        {
            logger.LogWarning("User {UserName} attempted to update AboutMe without Admin role", context.User.Identity?.Name);
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
        var updated = await repository.UpdateAsync(request, username);
        
        logger.LogInformation("AboutMe updated by {UserName}", username);
        return Results.Ok(updated);
    }
}
