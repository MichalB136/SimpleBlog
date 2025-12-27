var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server resource
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent);

var blogDb = sqlServer.AddDatabase("blogdb");

var apiService = builder.AddProject<Projects.SimpleBlog_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithReference(blogDb)
    .WaitFor(blogDb);

builder.AddProject<Projects.SimpleBlog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
