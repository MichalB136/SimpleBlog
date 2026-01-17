# SimpleBlog.ApiService

## Overview

REST API service for SimpleBlog providing endpoints for blog posts, comments, shop products, orders, and user authentication.

## Technologies

- **.NET 9.0** - Framework
- **ASP.NET Core Minimal APIs** - API endpoints
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **JWT Authentication** - Security
- **Swagger/OpenAPI** - API documentation

## Project Structure

```
SimpleBlog.ApiService/
├── Configuration/          # Configuration classes
├── Data/                   # DbContext and migrations
│   └── Migrations/         # EF Core migrations
├── Endpoints/              # API endpoint definitions
│   ├── AboutEndpoints.cs
│   ├── AuthEndpoints.cs
│   ├── CommentEndpoints.cs
│   ├── OrderEndpoints.cs
│   ├── PostEndpoints.cs
│   └── ProductEndpoints.cs
├── Identity/               # User management
├── Middleware/             # Custom middleware
├── Seeding/                # Database seeding
├── Constants.cs            # Application constants
├── DatabaseSeeder.cs       # Seed data logic
├── Program.cs              # Application entry point
└── Dockerfile              # Container definition
```

## Key Features

- **Modular Endpoints** - Organized by domain (Blog, Shop, Auth)
- **JWT Authentication** - Secure token-based auth
- **Role-Based Authorization** - Admin and User roles
- **Input Validation** - FluentValidation integration
- **Database Seeding** - Auto-seed test data
- **Health Checks** - `/health` endpoint
- **CORS Support** - Configurable origins

## Configuration

### Environment Variables

```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__Default=<postgres-connection-string>
Database__Provider=postgresql
Jwt__Key=<your-secret-key>
Jwt__Issuer=SimpleBlog
Jwt__Audience=SimpleBlog
Cors__AllowedOrigins__0=http://localhost:5173
```

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters-long",
    "Issuer": "SimpleBlog",
    "Audience": "SimpleBlog",
    "ExpiryMinutes": 1440
  }
}
```

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user (returns JWT)

### Blog Posts

- `GET /api/posts` - Get all posts
- `GET /api/posts/{id}` - Get post by ID
- `POST /api/posts` - Create post (Admin only)
- `PUT /api/posts/{id}` - Update post (Admin only)
- `DELETE /api/posts/{id}` - Delete post (Admin only)
- `PUT /api/posts/{id}/pin` - Pin/unpin post (Admin only)

### Comments

- `GET /api/posts/{postId}/comments` - Get comments for post
- `POST /api/posts/{postId}/comments` - Add comment (Authenticated)
- `DELETE /api/comments/{id}` - Delete comment (Admin or Author)

### Products

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product (Admin only)
- `PUT /api/products/{id}` - Update product (Admin only)
- `DELETE /api/products/{id}` - Delete product (Admin only)

### Orders

- `GET /api/orders` - Get user orders (Authenticated)
- `POST /api/orders` - Create order (Authenticated)

### About

- `GET /api/about` - Get about page content
- `PUT /api/about` - Update about page (Admin only)

## Running Locally

```bash
# Via Aspire (recommended)
dotnet run --project ../SimpleBlog.AppHost

# Standalone (requires PostgreSQL running)
dotnet run --project SimpleBlog.ApiService.csproj

# Access Swagger UI
http://localhost:<port>/swagger
```

## Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName --context ApplicationDbContext

# Apply migrations
dotnet ef database update --context ApplicationDbContext

# Remove last migration
dotnet ef migrations remove --context ApplicationDbContext
```

## Testing

```bash
# Run all tests
dotnet test ../SimpleBlog.Tests/SimpleBlog.Tests.csproj

# Run specific test
dotnet test --filter "FullyQualifiedName~AuthEndpoints"
```

## Docker Build

```bash
# Build image
docker build -f Dockerfile -t simpleblog-api ..

# Run container
docker run -p 8080:8080 \
  -e ConnectionStrings__Default="<connection-string>" \
  -e Jwt__Key="<your-secret-key>" \
  simpleblog-api
```

## Default Users (Development)

| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Admin |
| user | user123 | User |

> ⚠️ **Warning:** These are development-only credentials. Never use in production!

## Dependencies

- `Microsoft.AspNetCore.OpenApi` - OpenAPI/Swagger
- `Microsoft.EntityFrameworkCore.Design` - EF Core tools
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL provider
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT auth
- `SimpleBlog.Common` - Shared models
- `SimpleBlog.Blog.Services` - Blog domain
- `SimpleBlog.Shop.Services` - Shop domain

## Related Documentation

- [Database Guide](../docs/development/database-guide.md)
- [API Documentation](../docs/technical/api-specification.md)
- [Deployment Guide](../docs/deployment/render-guide.md)

## Troubleshooting

### Port Already in Use

```bash
# Find process using port
netstat -ano | findstr :<port>

# Kill process
taskkill /PID <process-id> /F
```

### Database Connection Errors

1. Verify PostgreSQL is running
2. Check connection string format
3. Ensure database exists
4. Verify firewall rules

### JWT Authentication Errors

1. Verify `Jwt__Key` is set and >= 32 characters
2. Check token expiry
3. Verify issuer/audience match
4. Ensure clock synchronization

## Contributing

See [Git Workflow Guide](../docs/development/git-workflow.md) for branching strategy and commit conventions.
