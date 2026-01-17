# SimpleBlog.AppHost

## Overview

.NET Aspire orchestration project that manages local development environment, service discovery, and resource provisioning for SimpleBlog.

## Technologies

- **.NET Aspire 13.1.0** - Orchestration framework
- **Service Discovery** - Automatic endpoint resolution
- **Resource Management** - PostgreSQL, services
- **Dashboard** - Built-in monitoring UI

## Project Structure

```
SimpleBlog.AppHost/
├── Properties/
│   └── launchSettings.json  # Launch configuration
├── Program.cs                # Orchestration setup
└── appsettings.json          # Configuration
```

## What is AppHost?

AppHost is the orchestrator for local development. It:

- 🐳 **Manages Docker containers** (PostgreSQL)
- 🔍 **Service discovery** (automatic URL resolution)
- 📊 **Monitoring dashboard** (logs, metrics, traces)
- 🔄 **Automatic restarts** on code changes
- ⚙️ **Environment configuration** per service

## Configured Resources

### Services

- **SimpleBlog.ApiService** - REST API backend
- **SimpleBlog.Web** - Frontend web application

### Databases

- **PostgreSQL** - Managed by Aspire with persistent volumes

## Service Discovery

Services reference each other by name:

```csharp
// In SimpleBlog.Web
builder.Services.AddHttpClient("ApiService", client =>
{
    // "apiservice" resolves automatically via Aspire
    client.BaseAddress = new Uri("https+http://apiservice");
});
```

## Running AppHost

```bash
# Start all services with dashboard
dotnet run --project SimpleBlog.AppHost

# Or use the helper script
..\scripts\Start-SimpleBlog.ps1
```

### Dashboard Access

Once started, Aspire Dashboard URL appears in console:

```
Now listening on: http://localhost:15xxx
```

**Dashboard Features:**
- 📊 **Resources** - View all services and databases
- 📝 **Logs** - Centralized logging
- 📈 **Metrics** - Performance monitoring
- 🔍 **Traces** - Distributed tracing
- 🔗 **Endpoints** - Service URLs

## Program.cs Structure

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("simpleblog-postgres-data");

var blogDb = postgres.AddDatabase("blogdb");

// Add API service
var apiService = builder.AddProject<Projects.SimpleBlog_ApiService>("apiservice")
    .WithReference(blogDb)
    .WithExternalHttpEndpoints();

// Add Web frontend
builder.AddProject<Projects.SimpleBlog_Web>("webfrontend")
    .WithReference(apiService)
    .WithExternalHttpEndpoints()
    .WaitFor(apiService);

builder.Build().Run();
```

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Aspire": "Information"
    }
  }
}
```

### Environment Variables

Aspire automatically injects:

- `ConnectionStrings__blogdb` - PostgreSQL connection
- `services__apiservice__http__0` - API service HTTP URL
- `services__apiservice__https__0` - API service HTTPS URL

## Advantages Over Docker Compose

| Feature | Aspire | Docker Compose |
|---------|--------|----------------|
| Service Discovery | ✅ Automatic | ❌ Manual |
| Dashboard | ✅ Built-in | ❌ None |
| Hot Reload | ✅ Yes | ❌ No |
| Distributed Tracing | ✅ Yes | ❌ Manual setup |
| .NET Integration | ✅ Native | ⚠️ External |

## Troubleshooting

### Port Conflicts

Edit `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:17xxx;http://localhost:15xxx"
    }
  }
}
```

### Service Not Starting

1. Check Dashboard logs
2. Verify database connection
3. Check service health endpoints
4. Review service dependencies

### PostgreSQL Issues

```bash
# Check container status in Dashboard
# Or manually:
docker ps | findstr postgres

# View logs
docker logs <container-id>
```

## Development Workflow

1. **Start AppHost** - `dotnet run`
2. **Open Dashboard** - Click URL in console
3. **View Services** - Check status in Resources tab
4. **Make Changes** - Code changes trigger auto-restart
5. **View Logs** - Centralized in Dashboard

## Related Documentation

- [Getting Started](../docs/development/getting-started.md)
- [Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Service Discovery](https://learn.microsoft.com/dotnet/aspire/service-discovery/)

## Key Concepts

### Resource Management

AppHost manages lifecycle:
- Container creation
- Port allocation
- Volume management
- Environment injection

### Dependency Ordering

Use `.WaitFor()` to control startup sequence:

```csharp
var api = builder.AddProject<ApiService>("api")
    .WithReference(db)
    .WaitFor(db);  // API starts after DB

var web = builder.AddProject<Web>("web")
    .WithReference(api)
    .WaitFor(api);  // Web starts after API
```

### Health Checks

Aspire monitors `/health` endpoints automatically and shows status in Dashboard.

## Production Deployment

⚠️ **AppHost is for local development only!**

For production:
- Use container orchestrator (Kubernetes, Docker Swarm)
- Use managed databases (Azure SQL, Render PostgreSQL)
- Implement proper service discovery (Consul, Azure Service Discovery)

See [Deployment Guide](../docs/deployment/render-guide.md) for production setup.
