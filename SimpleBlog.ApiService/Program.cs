using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.ApiService;
using SimpleBlog.ApiService.Configuration;
using SimpleBlog.ApiService.Data;
using SimpleBlog.ApiService.Endpoints;
using SimpleBlog.ApiService.Identity;
using SimpleBlog.ApiService.Services;
using SimpleBlog.Blog.Services;
using SimpleBlog.Shop.Services;
using SimpleBlog.Email.Services;
using SimpleBlog.Common;
using SimpleBlog.Common.Interfaces;

var builder = WebApplication.CreateBuilder(args);
ConfigureConfiguration(builder);
ConfigureHosting(builder);

var connectionString = builder.Configuration.GetConnectionString("blogdb")
    ?? throw new InvalidOperationException("Connection string 'blogdb' not found.");

var jwtParameters = builder.ConfigureJwt();
ConfigureServices(builder, connectionString);

var cloudinarySetup = CloudinarySetup.Configure(builder.Services, builder.Configuration);

var app = builder.Build();

LogCloudinarySetup(app, cloudinarySetup);
await InitializeDatabaseAsync(app);
ConfigurePipeline(app, jwtParameters);

await app.RunAsync();

static void ConfigureConfiguration(WebApplicationBuilder builder)
{
    builder.Configuration.AddSharedConfiguration(builder.Environment.EnvironmentName);
    builder.AddServiceDefaults();
}

static void ConfigureHosting(WebApplicationBuilder builder)
{
    if (Environment.GetEnvironmentVariable("PORT") is string port && int.TryParse(port, out var parsedPort))
    {
        builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(parsedPort));
    }
}

static void ConfigureServices(WebApplicationBuilder builder, string connectionString)
{
    builder.Services.AddProblemDetails();
    builder.Services.AddLogging();
    builder.Services.AddApiConfigurations(builder.Configuration);
    builder.Services.ConfigureValidation();
    builder.Services.ConfigureOperationLogging();
    builder.Services.AddOpenApi();
    builder.ConfigureCors();
    builder.ConfigureDatabase(connectionString);
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
    builder.Services.AddScoped<IUserRepository, IdentityUserRepository>();
}

static async Task InitializeDatabaseAsync(WebApplication app)
{
    await DatabaseExtensions.MigrateDatabaseAsync(app);

    var seedData = app.Configuration.GetValue<bool>("Database:SeedData");
    if (seedData)
    {
        await DatabaseExtensions.SeedDatabaseAsync(app);
    }
}

static void ConfigurePipeline(WebApplication app, (string Issuer, string Audience, byte[] Key) jwtParameters)
{
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseMiddleware<SimpleBlog.ApiService.Middleware.RequestLoggingMiddleware>();
    app.UseCors("AllowDevClients");
    app.UseAuthentication();
    app.UseAuthorization();

    var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();
    app.MapHealthChecks(endpointConfig.Health);
    app.MapAuthEndpoints(jwtParameters.Issuer, jwtParameters.Audience, jwtParameters.Key);
    app.MapPostEndpoints();
    app.MapAboutMeEndpoints();
    app.MapProductEndpoints();
    app.MapOrderEndpoints();
    app.MapSiteSettingsEndpoints();
    app.MapTagEndpoints();
    app.MapDefaultEndpoints();
}

static void LogCloudinarySetup(WebApplication app, CloudinarySetup.Result cloudinarySetup)
{
    if (cloudinarySetup.Message is null)
    {
        return;
    }

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    if (cloudinarySetup.Configured)
    {
        logger.LogInformation("{Message}", cloudinarySetup.Message);
    }
    else
    {
        logger.LogWarning("{Message}", cloudinarySetup.Message);
    }
}

