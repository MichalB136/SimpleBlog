# SimpleBlog.Common.Api

## Overview

Shared API configuration library providing common ASP.NET Core setup including authentication, CORS, endpoint configuration, and health checks.

## Technologies

- **.NET 9.0** - Framework
- **ASP.NET Core** - Web framework
- **JWT Bearer Authentication** - Security

## Project Structure

```
SimpleBlog.Common.Api/
├── Configuration/            # Configuration classes
│   ├── JwtConfiguration.cs
│   ├── CorsConfiguration.cs
│   └── ApiConfiguration.cs
└── Extensions/               # Service registration extensions
    ├── AuthenticationExtensions.cs
    ├── CorsExtensions.cs
    └── EndpointExtensions.cs
```

## Key Components

### JWT Configuration

```csharp
public class JwtConfiguration
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 1440; // 24 hours
}
```

### CORS Configuration

```csharp
public class CorsConfiguration
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public bool AllowCredentials { get; set; } = true;
    public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
    public string[] AllowedHeaders { get; set; } = new[] { "*" };
}
```

## Extension Methods

### Authentication Setup

```csharp
public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection("Jwt").Get<JwtConfiguration>();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtConfig.Key))
                };
            });
        
        return services;
    }
}
```

### CORS Setup

```csharp
public static class CorsExtensions
{
    public static IServiceCollection AddConfiguredCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var corsConfig = configuration.GetSection("Cors").Get<CorsConfiguration>();
        
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins(corsConfig.AllowedOrigins)
                       .WithMethods(corsConfig.AllowedMethods)
                       .WithHeaders(corsConfig.AllowedHeaders);
                
                if (corsConfig.AllowCredentials)
                {
                    builder.AllowCredentials();
                }
            });
        });
        
        return services;
    }
}
```

### Endpoint Configuration

```csharp
public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapCommonEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        // Health check
        endpoints.MapHealthChecks("/health");
        
        // Swagger in development
        if (endpoints.ServiceProvider
            .GetRequiredService<IHostEnvironment>()
            .IsDevelopment())
        {
            endpoints.MapSwagger();
        }
        
        return endpoints;
    }
    
    public static RouteHandlerBuilder RequireAuthorization(
        this RouteHandlerBuilder builder,
        params string[] roles)
    {
        if (roles.Length > 0)
        {
            return builder.RequireAuthorization(policy =>
                policy.RequireRole(roles));
        }
        
        return builder.RequireAuthorization();
    }
}
```

## Usage in API Project

### Program.cs Setup

```csharp
using SimpleBlog.Common.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services from Common.Api
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddConfiguredCors(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use middleware
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map common endpoints
app.MapCommonEndpoints();

// Map your endpoints
app.MapGet("/api/posts", () => { /* ... */ })
   .RequireAuthorization("Admin");

app.Run();
```

### appsettings.json

```json
{
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters-long",
    "Issuer": "SimpleBlog",
    "Audience": "SimpleBlog",
    "ExpiryMinutes": 1440
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "https://simpleblog-web.onrender.com"
    ],
    "AllowCredentials": true,
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "PATCH"],
    "AllowedHeaders": ["*"]
  }
}
```

## Features

### JWT Token Generation

```csharp
public static string GenerateJwtToken(
    string userId,
    string username,
    string[] roles,
    JwtConfiguration config)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, userId),
        new(ClaimTypes.Name, username)
    };
    
    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
    
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Key));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var token = new JwtSecurityToken(
        issuer: config.Issuer,
        audience: config.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(config.ExpiryMinutes),
        signingCredentials: creds);
    
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Role-Based Authorization

```csharp
// Require any authenticated user
app.MapGet("/api/profile", () => { /* ... */ })
   .RequireAuthorization();

// Require specific role
app.MapPost("/api/posts", () => { /* ... */ })
   .RequireAuthorization("Admin");

// Require multiple roles (any of)
app.MapDelete("/api/posts/{id}", () => { /* ... */ })
   .RequireAuthorization("Admin", "Moderator");
```

## Configuration Options

### Development vs Production

```csharp
// Development
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"],
    "AllowCredentials": true
  }
}

// Production
{
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com"],
    "AllowCredentials": true,
    "AllowedHeaders": ["Content-Type", "Authorization"]
  }
}
```

### Security Best Practices

1. **JWT Key Length** - Minimum 32 characters
2. **HTTPS Only** - Enforce HTTPS in production
3. **Specific Origins** - Never use `*` for CORS origins in production
4. **Token Expiry** - Set appropriate expiry (24 hours recommended)
5. **Secure Storage** - Store JWT key in environment variables or secrets

## Dependencies

- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `Microsoft.AspNetCore.Cors` - CORS support
- `Microsoft.Extensions.Configuration.Abstractions` - Configuration

## Testing

```csharp
public class AuthenticationTests
{
    [Fact]
    public void GenerateJwtToken_CreatesValidToken()
    {
        // Arrange
        var config = new JwtConfiguration
        {
            Key = "this-is-a-secret-key-at-least-32-characters-long",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        };
        
        // Act
        var token = JwtHelper.GenerateJwtToken(
            "user1",
            "testuser",
            new[] { "User" },
            config);
        
        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }
}
```

## Troubleshooting

### CORS Issues

```
Access to fetch at 'http://api' from origin 'http://app' has been blocked by CORS policy
```

**Solution:**
1. Add origin to `AllowedOrigins` in configuration
2. Ensure `app.UseCors()` is called before `app.UseAuthorization()`
3. Verify origin matches exactly (including http/https)

### JWT Validation Errors

```
IDX10503: Signature validation failed
```

**Solution:**
1. Verify JWT Key matches between generation and validation
2. Check Issuer and Audience match
3. Ensure key is at least 32 characters

## Related Documentation

- [Authentication Guide](../docs/development/authentication.md)
- [Security Checklist](../docs/deployment/security-checklist.md)
- [JWT.io](https://jwt.io/) - JWT debugger

## Contributing

When extending this library:
1. Keep API-specific code here, general utilities in SimpleBlog.Common
2. Make configuration flexible via appsettings
3. Follow security best practices
4. Add comprehensive XML documentation
5. Include usage examples
