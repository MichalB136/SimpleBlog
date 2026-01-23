# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY SimpleBlog.sln .
COPY SimpleBlog.ApiService/SimpleBlog.ApiService.csproj SimpleBlog.ApiService/
COPY SimpleBlog.ServiceDefaults/SimpleBlog.ServiceDefaults.csproj SimpleBlog.ServiceDefaults/
COPY SimpleBlog.Common/SimpleBlog.Common.csproj SimpleBlog.Common/
COPY SimpleBlog.Common.Api/SimpleBlog.Common.Api.csproj SimpleBlog.Common.Api/
COPY SimpleBlog.Blog.Services/SimpleBlog.Blog.Services.csproj SimpleBlog.Blog.Services/
COPY SimpleBlog.Shop.Services/SimpleBlog.Shop.Services.csproj SimpleBlog.Shop.Services/
COPY SimpleBlog.Email.Services/SimpleBlog.Email.Services.csproj SimpleBlog.Email.Services/

# Restore dependencies
RUN dotnet restore SimpleBlog.ApiService/SimpleBlog.ApiService.csproj

# Copy source code
COPY SimpleBlog.ApiService/ SimpleBlog.ApiService/
COPY SimpleBlog.ServiceDefaults/ SimpleBlog.ServiceDefaults/
COPY SimpleBlog.Common/ SimpleBlog.Common/
COPY SimpleBlog.Common.Api/ SimpleBlog.Common.Api/
COPY SimpleBlog.Blog.Services/ SimpleBlog.Blog.Services/
COPY SimpleBlog.Shop.Services/ SimpleBlog.Shop.Services/
COPY SimpleBlog.Email.Services/ SimpleBlog.Email.Services/

# Build and publish
WORKDIR /src/SimpleBlog.ApiService
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Copy shared configuration files expected at runtime
COPY appsettings.shared.json ./
COPY appsettings.shared.Development.json ./
COPY appsettings.shared.Production.json ./

# Expose PORT environment variable (Render injects this)
ENV PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Health check
RUN apt-get update \
  && apt-get install -y --no-install-recommends curl \
  && rm -rf /var/lib/apt/lists/*

HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:"${PORT}"/health || exit 1

ENTRYPOINT ["dotnet", "SimpleBlog.ApiService.dll"]
