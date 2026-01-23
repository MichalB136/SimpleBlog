using CommunityToolkit.Aspire.Hosting.NodeJS;

var builder = DistributedApplication.CreateBuilder(args);
// Read API service port from shared appsettings or fall back to env PORT or default 5433
var apiPortConfig = builder.Configuration["Ports:ApiService:Http"];
int apiPort;
if (!int.TryParse(apiPortConfig, out apiPort))
{
    if (!int.TryParse(Environment.GetEnvironmentVariable("PORT"), out apiPort))
    {
        apiPort = 5433;
    }
}
// PostgreSQL database is managed externally via docker-compose
// User must run: docker-compose up -d
// Before starting the application

// Use configured port from shared appsettings so ports are managed centrally
var apiService = builder.AddProject<Projects.SimpleBlog_ApiService>("apiservice")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithHttpEndpoint(port: apiPort, name: "apiservice-http", env: "PORT");
// Vite dev server orchestrated by Aspire (always available for dev; ignored in publish)
var viteApp = builder.AddViteApp("vite", "../SimpleBlog.Web/client")
    .WithNpmPackageInstallation()
    .WithExternalHttpEndpoints()
    .WithEnvironment("PORT", "5175")
    .WithEnvironment("BROWSER", "none")
    .WithEnvironment("FORCE_COLOR", "1")
    .WithEnvironment("VITE_API_BASE_URL", "http://localhost:5433")
    .WaitFor(apiService);

// WebFrontend serves built SPA (production behavior) and remains available in dev
builder.AddProject<Projects.SimpleBlog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(viteApp)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Vite__DevServerUrl", "http://localhost:5175")
    .WaitFor(apiService)
    .WaitFor(viteApp);

await builder.Build().RunAsync();
