using SimpleBlog.Web;
using SimpleBlog.Web.Configuration;
using SimpleBlog.Common.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
// Ensure ProblemDetails is registered so UseExceptionHandler() works in Production
builder.Services.AddProblemDetails();

// Configure Kestrel/URLs using centralized appsettings Ports. Fall back to env PORT only if missing.
var webPortConfig = builder.Configuration["Ports:Web:Http"];
if (int.TryParse(webPortConfig, out var webPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{webPort}");
}
else if (Environment.GetEnvironmentVariable("PORT") is string envPort)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{envPort}");
}

// Support for both Aspire service discovery (dev) and external API URL (production/Render)
// In dev, prefer explicit localhost URLs to avoid service discovery timeout issues
var apiBaseUrl = builder.Environment.IsDevelopment()
    ? (Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5433")
    : (builder.Configuration["Api:BaseUrl"] 
        ?? Environment.GetEnvironmentVariable("API_BASE_URL"));

builder.Services.AddHttpClient(ApiConstants.ClientName, client =>
{
    client.BaseAddress = new(apiBaseUrl);
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
// Map API endpoints before serving the SPA to ensure /api routes are handled
WebAppSetup.MapApiEndpoints(app);

// Client serving (dev proxy or static files)
WebAppSetup.ConfigureClientServing(app);

// SPA fallback handled in ConfigureClientServing for production

app.MapHealthChecks("/health");
app.MapDefaultEndpoints();

await app.RunAsync();
