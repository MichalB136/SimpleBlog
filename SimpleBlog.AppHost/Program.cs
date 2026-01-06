var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database is managed externally via docker-compose
// User must run: docker-compose up -d
// Before starting the application

var apiService = builder.AddProject<Projects.SimpleBlog_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", "5433")
    .WithEnvironment("ASPNETCORE_HTTPS_PORTS", "7589");

builder.AddProject<Projects.SimpleBlog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", "5080")
    .WithEnvironment("ASPNETCORE_HTTPS_PORTS", "7166")
    .WithReference(apiService)
    .WaitFor(apiService);

await builder.Build().RunAsync();
