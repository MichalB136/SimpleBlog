using System.Text;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.ApiService;
using SimpleBlog.ApiService.Configuration;
using SimpleBlog.ApiService.Data;
using SimpleBlog.ApiService.Endpoints;
using SimpleBlog.ApiService.Identity;
using SimpleBlog.Blog.Services;
using SimpleBlog.Shop.Services;
using SimpleBlog.Email.Services;
using SimpleBlog.Common;

var builder = WebApplication.CreateBuilder(args);

// Load shared endpoint configuration from hierarchy:
// 1. appsettings.shared.json (base)
// 2. appsettings.shared.{Environment}.json (environment override)
// 3. Environment variables (SimpleBlog_* prefix)
builder.Configuration.AddSharedConfiguration(builder.Environment.EnvironmentName);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Configure Kestrel to bind to Render's PORT environment variable if present
if (Environment.GetEnvironmentVariable("PORT") is string port)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddLogging();

// Add API configurations (endpoints and authorization) to DI container
builder.Services.AddApiConfigurations(builder.Configuration);

// Configure FluentValidation
builder.Services.ConfigureValidation();

// Configure Operation Logging
builder.Services.ConfigureOperationLogging();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure CORS
builder.ConfigureCors();

// Configure JWT & get JWT parameters
var (jwtIssuer, jwtAudience, jwtKey) = builder.ConfigureJwt();

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("blogdb") 
    ?? throw new InvalidOperationException("Connection string 'blogdb' not found.");
builder.ConfigureDatabase(connectionString);

// Add Email Service
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IUserRepository, IdentityUserRepository>();

var app = builder.Build();

// Apply database migrations automatically
await DatabaseExtensions.MigrateDatabaseAsync(app);

// Seed database with dummy data if configured
var seedData = app.Configuration.GetValue<bool>("Database:SeedData");
if (seedData)
{
    await DatabaseExtensions.SeedDatabaseAsync(app);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Get configurations from DI container
var endpointConfig = app.Services.GetRequiredService<EndpointConfiguration>();

// Request logging middleware (correlation + timing)
app.UseMiddleware<SimpleBlog.ApiService.Middleware.RequestLoggingMiddleware>();

app.UseCors("AllowDevClients");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks(endpointConfig.Health);

// Map all endpoints
app.MapAuthEndpoints(jwtIssuer, jwtAudience, jwtKey);
app.MapPostEndpoints();
app.MapAboutMeEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();

app.MapDefaultEndpoints();

await app.RunAsync();

