using CommunityToolkit.Aspire.Hosting.NodeJS;

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database is managed externally via docker-compose
// User must run: docker-compose up -d
// Before starting the application

var apiService = builder.AddProject<Projects.SimpleBlog_ApiService>("apiservice")
    .WithHttpEndpoint(port: 5433, name: "apiservice-http", env: "PORT")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

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
