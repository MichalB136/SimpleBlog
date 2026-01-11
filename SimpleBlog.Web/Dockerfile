# Frontend build stage (Node.js)
FROM node:20-alpine AS frontend-build
WORKDIR /app/client

# Copy package files
COPY SimpleBlog.Web/client/package*.json ./
COPY SimpleBlog.Web/client/tsconfig*.json ./
COPY SimpleBlog.Web/client/vite.config.ts ./
COPY SimpleBlog.Web/client/index.html ./

# Install dependencies and build
RUN npm ci && npm run build

# Backend build stage (.NET)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# Copy solution and project files
COPY SimpleBlog.sln .
COPY SimpleBlog.Web/SimpleBlog.Web.csproj SimpleBlog.Web/
COPY SimpleBlog.ServiceDefaults/SimpleBlog.ServiceDefaults.csproj SimpleBlog.ServiceDefaults/

# Restore dependencies
RUN dotnet restore SimpleBlog.Web/SimpleBlog.Web.csproj

# Copy source code
COPY SimpleBlog.Web/ SimpleBlog.Web/
COPY SimpleBlog.ServiceDefaults/ SimpleBlog.ServiceDefaults/

# Copy frontend build output to wwwroot/dist
COPY --from=frontend-build /app/client/dist SimpleBlog.Web/wwwroot/dist

# Build and publish
WORKDIR /src/SimpleBlog.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published app (includes wwwroot with dist)
COPY --from=backend-build /app/publish .

# Expose PORT environment variable (Render injects this)
ENV PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:"${PORT}"/health || exit 1

ENTRYPOINT ["dotnet", "SimpleBlog.Web.dll"]
