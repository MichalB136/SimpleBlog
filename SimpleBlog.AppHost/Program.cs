var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.SimpleBlog_ApiService>("apiservice")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.SimpleBlog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
