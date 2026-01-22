using SimpleBlog.Web;
using SimpleBlog.Web.Configuration;
using SimpleBlog.Common.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Kestrel to bind to Render's PORT environment variable if present
if (Environment.GetEnvironmentVariable("PORT") is string port)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Support for both Aspire service discovery (dev) and external API URL (production/Render)
// In dev, prefer explicit localhost URLs to avoid service discovery timeout issues
var apiBaseUrl = builder.Environment.IsDevelopment()
    ? (Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5433")
    : (builder.Configuration["Api:BaseUrl"] 
        ?? Environment.GetEnvironmentVariable("API_BASE_URL")
        ?? "https+http://apiservice");

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
// Client serving (dev proxy or static files)
WebAppSetup.ConfigureClientServing(app);

// API endpoints
WebAppSetup.MapApiEndpoints(app);

// SPA fallback handled in ConfigureClientServing for production

app.MapHealthChecks("/health");
app.MapDefaultEndpoints();

await app.RunAsync();
