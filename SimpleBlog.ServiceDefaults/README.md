# SimpleBlog.ServiceDefaults

## Overview

Shared configuration library providing common Aspire service defaults including health checks, service discovery, resilience, and telemetry for all SimpleBlog services.

## Technologies

- **.NET Aspire 13.1.0** - Cloud-ready app stack
- **OpenTelemetry** - Observability
- **Health Checks** - Service health monitoring

## Project Structure

```
SimpleBlog.ServiceDefaults/
└── Extensions.cs             # Service configuration extensions
```

## Key Features

- ✅ **Health Checks** - Automatic health endpoint registration
- ✅ **Service Discovery** - Aspire service resolution
- ✅ **Telemetry** - OpenTelemetry integration (logs, metrics, traces)
- ✅ **Resilience** - HTTP resilience patterns
- ✅ **Configuration** - Environment-aware setup

## Extension Methods

### AddServiceDefaults

Main method that configures all defaults:

```csharp
public static IHostApplicationBuilder AddServiceDefaults(
    this IHostApplicationBuilder builder)
{
    builder.ConfigureOpenTelemetry();
    builder.AddDefaultHealthChecks();
    builder.Services.AddServiceDiscovery();
    builder.Services.ConfigureHttpClientDefaults(http =>
    {
        http.AddStandardResilienceHandler();
        http.AddServiceDiscovery();
    });
    
    return builder;
}
```

### ConfigureOpenTelemetry

Sets up distributed tracing and metrics:

```csharp
private static IHostApplicationBuilder ConfigureOpenTelemetry(
    this IHostApplicationBuilder builder)
{
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
    });
    
    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics.AddRuntimeInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddAspNetCoreInstrumentation();
        })
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation();
        });
    
    builder.AddOpenTelemetryExporters();
    
    return builder;
}
```

### AddDefaultHealthChecks

Registers standard health checks:

```csharp
private static IHostApplicationBuilder AddDefaultHealthChecks(
    this IHostApplicationBuilder builder)
{
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    
    return builder;
}
```

## Usage in Services

### Program.cs Integration

```csharp
using SimpleBlog.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults (health checks, telemetry, service discovery)
builder.AddServiceDefaults();

// Add your services
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

var app = builder.Build();

// Map health checks
app.MapDefaultEndpoints();

// Map your endpoints
app.MapGet("/api/posts", () => { /* ... */ });

app.Run();
```

### MapDefaultEndpoints

```csharp
public static IEndpointRouteBuilder MapDefaultEndpoints(
    this IEndpointRouteBuilder routes)
{
    // Health check endpoints
    routes.MapHealthChecks("/health");
    routes.MapHealthChecks("/alive", new HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("live")
    });
    
    return routes;
}
```

## Telemetry Features

### Automatic Instrumentation

ServiceDefaults automatically instruments:

- **HTTP Requests** - Incoming ASP.NET Core requests
- **HTTP Client** - Outgoing HTTP client calls
- **Database** - Entity Framework Core queries
- **Runtime** - .NET runtime metrics (GC, threadpool, etc.)

### Viewing Telemetry

Telemetry is visible in **Aspire Dashboard**:

1. Start application via AppHost
2. Open Aspire Dashboard URL
3. Navigate to:
   - **Traces** - Distributed tracing
   - **Metrics** - Performance metrics
   - **Logs** - Structured logging

## Service Discovery

### Automatic Resolution

Services registered with `AddServiceDiscovery()` can resolve by name:

```csharp
// Instead of hardcoded URL:
// client.BaseAddress = new Uri("http://localhost:5000");

// Use service name:
builder.Services.AddHttpClient("ApiService", client =>
{
    client.BaseAddress = new Uri("https+http://apiservice");
});
```

**Benefits:**
- ✅ No hardcoded URLs
- ✅ Works in development and production
- ✅ Automatic port resolution
- ✅ Load balancing support

## Resilience

### Standard Resilience Handler

Adds retry, circuit breaker, and timeout policies:

```csharp
builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Adds standard resilience (retry, circuit breaker, timeout)
    http.AddStandardResilienceHandler();
});
```

**Policies:**
- **Retry** - Retries transient failures (3 attempts)
- **Circuit Breaker** - Opens circuit after consecutive failures
- **Timeout** - Request timeout (30 seconds)

### Custom Resilience

```csharp
builder.Services.AddHttpClient("ApiService")
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 5;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(60);
    });
```

## Health Checks

### Built-in Checks

- **Self** - Basic liveness check
- **Database** - DbContext health (if added)
- **Dependencies** - External service health

### Custom Health Checks

```csharp
// In your service
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddCheck<CustomHealthCheck>("custom");

// Custom health check implementation
public class CustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        // Check your service health
        bool isHealthy = CheckDependency();
        
        return Task.FromResult(
            isHealthy 
                ? HealthCheckResult.Healthy("Service is healthy")
                : HealthCheckResult.Unhealthy("Service is down"));
    }
}
```

## Configuration

### Environment-Aware

ServiceDefaults respects environment settings:

```csharp
// Development: Detailed logging, sensitive data
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

// Production: Performance optimized, secure
if (builder.Environment.IsProduction())
{
    builder.Services.AddResponseCompression();
}
```

### OpenTelemetry Exporters

Configure where telemetry is sent:

```json
{
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317",
  "OTEL_SERVICE_NAME": "SimpleBlog.ApiService"
}
```

## Benefits

### Consistency

All services get same baseline configuration:
- Health checks
- Telemetry
- Service discovery
- Resilience

### Maintainability

Single location for common configuration. Update once, apply everywhere.

### Best Practices

Built-in best practices:
- Structured logging
- Distributed tracing
- Circuit breaker pattern
- Health monitoring

## Dependencies

- `Microsoft.Extensions.Hosting` - Hosting abstractions
- `Microsoft.Extensions.ServiceDiscovery` - Service discovery
- `Microsoft.Extensions.Http.Resilience` - Resilience patterns
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` - Telemetry export

## Related Services

Used by:
- SimpleBlog.ApiService
- SimpleBlog.Web
- (Future microservices)

## Related Documentation

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- [Health Checks](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)

## Troubleshooting

### Telemetry Not Showing

1. Verify Aspire Dashboard is running
2. Check `OTEL_EXPORTER_OTLP_ENDPOINT` configuration
3. Ensure `AddServiceDefaults()` is called early in startup

### Service Discovery Not Working

1. Verify service names match in AppHost
2. Check `AddServiceDiscovery()` is registered
3. Use `https+http://` prefix for flexible scheme resolution

### Health Checks Failing

1. Review health check endpoint: `/health`
2. Check individual health check implementations
3. Verify database connections and dependencies

## Best Practices

1. **Call Early** - Call `AddServiceDefaults()` early in startup
2. **Standard Names** - Use consistent service names
3. **Custom Checks** - Add service-specific health checks
4. **Monitor Dashboard** - Regularly check Aspire Dashboard
5. **Production Ready** - Configure appropriate exporters for production

## Version Compatibility

| ServiceDefaults | .NET Aspire | .NET SDK |
|----------------|-------------|----------|
| 1.0.0 | 13.1.0 | 9.0+ |

## Contributing

When updating ServiceDefaults:
1. Consider impact on all services
2. Test with both development and production configurations
3. Update documentation
4. Maintain backwards compatibility when possible
