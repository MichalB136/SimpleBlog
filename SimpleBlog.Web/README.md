# SimpleBlog.Web

## Overview

ASP.NET Core web application serving as a frontend proxy and host for the React-based SimpleBlog UI.

## Technologies

- **ASP.NET Core 9.0** - Web framework
- **React 18** - UI library (in `client/` directory)
- **Vite** - Frontend build tool
- **TypeScript** - Type-safe JavaScript
- **YARP** - Reverse proxy for API forwarding

## Project Structure

```
SimpleBlog.Web/
├── client/                    # React + Vite frontend
│   ├── src/
│   │   ├── api/              # API service layer
│   │   ├── components/       # React components
│   │   ├── context/          # React context
│   │   ├── hooks/            # Custom hooks
│   │   ├── styles/           # CSS files
│   │   ├── types/            # TypeScript types
│   │   ├── App.tsx           # Main component
│   │   └── main.tsx          # Entry point
│   ├── index.html
│   ├── vite.config.ts
│   ├── package.json
│   └── README.md             # Frontend-specific docs
├── wwwroot/                   # Static files (legacy)
├── Properties/
├── Program.cs                 # Web host setup
└── Dockerfile
```

## Key Features

- **Reverse Proxy** - Forwards `/api` requests to ApiService
- **Static Files** - Serves built React app
- **CORS Handling** - Managed by API service
- **Service Discovery** - Uses Aspire for API location
- **Health Checks** - `/health` endpoint

## Configuration

### Environment Variables

```env
ASPNETCORE_ENVIRONMENT=Development
API_BASE_URL=https+http://apiservice  # Aspire discovery
# Or for production:
API_BASE_URL=https://simpleblog-api.onrender.com
```

### Program.cs

```csharp
// Service discovery for API
builder.Services.AddHttpClient("ApiService", client =>
{
    var apiUrl = builder.Configuration["API_BASE_URL"] 
                 ?? "https+http://apiservice";
    client.BaseAddress = new Uri(apiUrl);
});

// Reverse proxy configuration
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
```

## Running Locally

### Development Mode

```bash
# Start backend via Aspire (includes Web)
dotnet run --project ../SimpleBlog.AppHost

# Or start Web standalone
dotnet run --project SimpleBlog.Web.csproj

# For frontend development with hot reload
cd client
npm install
npm run dev
# Access at http://localhost:5173
```

### Production Build

```bash
# Build React app
cd client
npm run build

# Start .NET app (serves built React)
dotnet run --configuration Release
```

## Frontend (client/)

### Available Scripts

```bash
# Install dependencies
npm install

# Development server (hot reload)
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Type checking
npm run type-check

# Linting
npm run lint
```

### Frontend Features

- ✅ **TypeScript** - Full type safety
- ✅ **React Router** - Client-side routing
- ✅ **Dark Mode** - Theme switcher
- ✅ **JWT Auth** - Token management
- ✅ **API Layer** - Centralized HTTP client
- ✅ **Custom Hooks** - `useAuth`, `usePosts`, etc.
- ✅ **Responsive Design** - Mobile-friendly

See [client/README.md](client/README.md) for detailed frontend documentation.

## Proxy Configuration

Requests to `/api/*` are forwarded to ApiService:

```json
{
  "ReverseProxy": {
    "Routes": {
      "api-route": {
        "ClusterId": "api-cluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "api-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "https://apiservice"
          }
        }
      }
    }
  }
}
```

## Docker Build

```bash
# Build image
docker build -f Dockerfile -t simpleblog-web ..

# Run container
docker run -p 8080:8080 \
  -e API_BASE_URL="https://api.example.com" \
  simpleblog-web
```

## Deployment

### Render.com

```yaml
# render.yaml
services:
  - type: web
    name: simpleblog-web
    env: docker
    dockerfilePath: ./SimpleBlog.Web/Dockerfile
    dockerContext: .
    envVars:
      - key: API_BASE_URL
        value: https://simpleblog-api.onrender.com
```

See [Deployment Guide](../docs/deployment/render-guide.md) for full instructions.

## Legacy wwwroot

The `wwwroot/` directory contains the old React UMD implementation. This is kept for backwards compatibility but will be removed in a future version. Use `client/` for all new development.

## Dependencies

### .NET

- `Microsoft.AspNetCore.OpenApi` - API documentation
- `Yarp.ReverseProxy` - API forwarding
- `SimpleBlog.ServiceDefaults` - Shared config

### Node (client/)

- `react` & `react-dom` - UI library
- `react-router-dom` - Routing
- `vite` - Build tool
- `typescript` - Type checking

## Troubleshooting

### API Requests Failing

1. Check API_BASE_URL is set correctly
2. Verify ApiService is running
3. Check reverse proxy configuration
4. Review CORS settings in ApiService

### Frontend Not Loading

1. Verify `npm run build` completed successfully
2. Check `wwwroot/` or client build output exists
3. Review browser console for errors
4. Ensure static files middleware is configured

### Hot Reload Not Working

1. Use `npm run dev` in `client/` directory
2. Ensure Vite dev server is running on port 5173
3. Check Vite proxy configuration
4. Verify WebSocket connection

## Related Documentation

- [Frontend README](client/README.md) - React app details
- [Getting Started](../docs/development/getting-started.md) - Setup guide
- [API Documentation](../docs/technical/api-specification.md) - API reference
- [Deployment Guide](../docs/deployment/render-guide.md) - Production deployment

## Contributing

For frontend changes:
1. Work in `client/` directory
2. Use TypeScript strict mode
3. Follow React best practices
4. Add tests for new components
5. Build and verify before committing

See [Git Workflow](../docs/development/git-workflow.md) for branching strategy.
