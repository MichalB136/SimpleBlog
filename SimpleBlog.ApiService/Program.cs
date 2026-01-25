using CloudinaryDotNet;
using Microsoft.Extensions.Logging;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.ApiService;
using Microsoft.AspNetCore.Antiforgery;
using SimpleBlog.ApiService.Configuration;
using SimpleBlog.ApiService.Data;
using SimpleBlog.ApiService.Endpoints;
using SimpleBlog.ApiService.Handlers;
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

// Resolve connection string from configuration or common environment aliases (DATABASE_URL)
var rawConnection = builder.Configuration.GetConnectionString("blogdb");

// If not present in configuration, check common environment variables Render provides
if (string.IsNullOrWhiteSpace(rawConnection))
{
    rawConnection = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__blogdb")
        ?? Environment.GetEnvironmentVariable("SimpleBlog_ConnectionStrings__blogdb");
}

if (string.IsNullOrWhiteSpace(rawConnection))
{
    throw new InvalidOperationException("Connection string 'blogdb' not found. Set SimpleBlog_ConnectionStrings__blogdb or DATABASE_URL.");
}

var connectionString = NormalizeConnectionString(rawConnection);


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
    // Prefer Aspire-provided PORT env var so AppHost/App orchestrator controls mapping.
    var envPort = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrWhiteSpace(envPort) && int.TryParse(envPort, out var parsedPort))
    {
        builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(parsedPort));
        return;
    }

    // Fallback to port configuration from appsettings (centralized). If present, bind Kestrel accordingly.
    var configHttp = builder.Configuration["Ports:ApiService:Http"];
    var configHttps = builder.Configuration["Ports:ApiService:Https"];
    if (int.TryParse(configHttp, out var cfgHttp))
    {
        builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(cfgHttp));
    }
    if (int.TryParse(configHttps, out var cfgHttps))
    {
        builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(cfgHttps, listenOptions => listenOptions.UseHttps()));
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
    // Antiforgery for endpoints that require it (e.g. multipart/form-data handlers)
    builder.Services.AddAntiforgery(options =>
    {
        // Expect CSRF token in header for API calls
        options.HeaderName = "X-CSRF-TOKEN";
    });
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
    builder.Services.AddScoped<IUserRepository, IdentityUserRepository>();
    builder.Services.AddScoped<IPostHandler, PostHandler>();
    builder.Services.AddScoped<IProductHandler, ProductHandler>();
    builder.Services.AddScoped<IOrderHandler, OrderHandler>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IAuthService, JwtAuthService>();
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

    // Enable antiforgery middleware so endpoints that carry antiforgery metadata are supported.
    // Must be after authentication/authorization and before mapping endpoints.
    app.UseAntiforgery();

    var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();
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

static string NormalizeConnectionString(string raw)
{
    if (string.IsNullOrWhiteSpace(raw)) return raw;

    if (raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) || raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(raw);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = userInfo.Length > 0 ? userInfo[0] : string.Empty,
            Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Require,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }

    return raw;
}

