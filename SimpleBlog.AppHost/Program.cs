var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database is managed externally via docker-compose
// User must run: docker-compose up -d
// Before starting the application

var apiService = builder.AddProject<Projects.SimpleBlog_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

builder.AddProject<Projects.SimpleBlog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

await builder.Build().RunAsync();
