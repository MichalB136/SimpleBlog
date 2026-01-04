# Endpoint Configuration Guide

## Overview

SimpleBlog uses centralized endpoint configuration in `appsettings.json`. This makes it easy to:
- Change endpoint paths without touching code
- Adjust authorization requirements
- Manage different configurations for different environments

## File Structure

```
SimpleBlog/
├── appsettings.shared.json          # Shared endpoint definitions (reference)
│
└── SimpleBlog.ApiService/
    ├── appsettings.json             # Production configuration
    └── appsettings.Development.json # Development overrides (dev only)
```

## Configuration Files

### appsettings.shared.json
Reference file showing all available endpoint configurations. Located in root directory. Can be used in Docker/deployment setups if needed.

**Contents:**
- `Endpoints` - All API endpoint paths
- `Authorization` - Role-based access control settings

### appsettings.json (ApiService)
Main configuration file. Contains:
- JWT settings
- CORS origins
- Email configuration
- **Endpoints** - All REST API paths
- **Authorization** - Permission requirements

### appsettings.Development.json (ApiService)
Development-specific overrides. Currently only overrides JWT key for development.

## Configuration Structure

### Endpoints Section

```json
{
  "Endpoints": {
    "Login": "/login",
    "Health": "/health",
    "OpenApi": "/openapi/v1.json",
    
    "Posts": {
      "Base": "/posts",
      "GetAll": "",
      "GetById": "/{id:guid}",
      "Create": "",
      "Update": "/{id:guid}",
      "Delete": "/{id:guid}",
      "GetComments": "/{id:guid}/comments",
      "AddComment": "/{id:guid}/comments"
    },
    
    "Products": {
      "Base": "/products",
      "GetAll": "",
      "GetById": "/{id:guid}",
      "Create": "",
      "Update": "/{id:guid}",
      "Delete": "/{id:guid}"
    },
    
    "Orders": {
      "Base": "/orders",
      "GetAll": "",
      "GetById": "/{id:guid}",
      "Create": ""
    }
  }
}
```

### Authorization Section

```json
{
  "Authorization": {
    "RequireAdminForPostCreate": true,
    "RequireAdminForPostDelete": true,
    "RequireAdminForProductCreate": true,
    "RequireAdminForProductUpdate": true,
    "RequireAdminForProductDelete": true,
    "RequireAdminForOrderView": true,
    "TokenExpirationHours": 8
  }
}
```

## How It Works

### Loading Configuration

In `SimpleBlog.ApiService/Program.cs`:

```csharp
// Load endpoint configuration from appsettings
var endpointConfig = new EndpointConfiguration();
builder.Configuration.GetSection("Endpoints").Bind(endpointConfig);

var authConfig = new AuthorizationConfiguration();
builder.Configuration.GetSection("Authorization").Bind(authConfig);

// Add to DI container
builder.Services.AddSingleton(endpointConfig);
builder.Services.AddSingleton(authConfig);
```

### Using Configuration in Endpoints

```csharp
// Login endpoint - path from config
app.MapPost(endpointConfig.Login, (LoginRequest request, ...) => {
    var tokenExpiration = DateTime.UtcNow.AddHours(authConfig.TokenExpirationHours);
    // ...
});

// Posts endpoints - paths from config
var posts = app.MapGroup(endpointConfig.Posts.Base);
posts.MapGet(endpointConfig.Posts.GetAll, ...);
posts.MapPost(endpointConfig.Posts.Create, (CreatePostRequest request, ...) => {
    if (authConfig.RequireAdminForPostCreate && !context.User.IsInRole("Admin")) {
        return Results.Forbid();
    }
    // ...
});
```

## Customizing Endpoints

### Change an Endpoint Path

Edit `SimpleBlog.ApiService/appsettings.json`:

```json
{
  "Endpoints": {
    "Posts": {
      "Base": "/blog",  // Changed from "/posts"
      ...
    }
  }
}
```

Code automatically uses the new path. No code changes needed.

### Disable Admin Requirement

Edit `SimpleBlog.ApiService/appsettings.json`:

```json
{
  "Authorization": {
    "RequireAdminForPostCreate": false,  // Anyone can create posts
    "RequireAdminForPostDelete": false   // Anyone can delete posts
  }
}
```

### Adjust Token Expiration

```json
{
  "Authorization": {
    "TokenExpirationHours": 24  // JWT tokens valid for 24 hours
  }
}
```

## Environment-Specific Configuration

### Development

File: `SimpleBlog.ApiService/appsettings.Development.json`

Only contains development-specific overrides. Inherits endpoints from `appsettings.json`.

```json
{
  "Jwt": {
    "Key": "development_only_key_never_use_in_production"
  }
}
```

### Production

For Render deployment:
1. Set `JWT:Key` in Render environment variables
2. Set `Cors:AllowedOrigins` to your domain
3. Other settings from `appsettings.json` are used

### Docker

Add to `docker-compose.yml` or pass as environment variables:

```yaml
environment:
  - Jwt__Key=your-production-key
  - Cors__AllowedOrigins__0=https://yourdomain.com
```

## Testing Endpoints

All endpoints respect the configuration. Examples:

### Login (default: POST /login)
```bash
curl -X POST http://localhost:8080/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

### Get All Posts (default: GET /posts)
```bash
curl http://localhost:8080/posts
```

### Create Post (requires admin role)
```bash
curl -X POST http://localhost:8080/posts \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"title":"My Post","content":"Content here"}'
```

## Adding New Endpoints

When adding new endpoints:

1. Update `EndpointConfiguration.cs` with new endpoint paths
2. Add to `appsettings.json` under `Endpoints` section
3. Use configuration in `Program.cs`:
   ```csharp
   app.MapGet(endpointConfig.NewEndpoint, handler);
   ```

## Related Files

- [SimpleBlog.ApiService/EndpointConfiguration.cs](../SimpleBlog.ApiService/EndpointConfiguration.cs) - Configuration model classes
- [SimpleBlog.ApiService/appsettings.json](../SimpleBlog.ApiService/appsettings.json) - Main configuration
- [appsettings.shared.json](../appsettings.shared.json) - Shared reference configuration
