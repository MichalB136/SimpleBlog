var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database is managed externally via docker-compose
// User must run: docker-compose up -d
// Before starting the application

var apiService = builder.AddProject<Projects.SimpleBlog_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// Vite dev server for frontend (development only)
// Runs: npm run dev in SimpleBlog.Web/client
var vite = builder.AddExecutable("vite", "npm", "../SimpleBlog.Web/client", "run", "dev")
    .WaitFor(apiService);

builder.AddProject<Projects.SimpleBlog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

await builder.Build().RunAsync();
