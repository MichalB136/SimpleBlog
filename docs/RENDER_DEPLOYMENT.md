# Deploying SimpleBlog on Render

## When to use Render
- Use Render Web Services (Docker or .NET runtime) for API/Web; Render Static Sites is insufficient because the app needs running ASP.NET processes.
- Prefer containers for parity with local Aspire; fall back to Render .NET runtime if you deploy a single service without Docker.

## Deployment models
- **Split services (recommended)**: Deploy ApiService and Web as separate Render Web Services. Configure Web to call the API via the API’s Render URL.
- **Combined container**: Build one image that serves both API and static assets, run as a single Render Web Service. Simplifies routing but couples concerns.
- **Static assets only (not enough)**: You can host the built `wwwroot` as a Static Site only if the API lives elsewhere.

## Prerequisites
- Render account with Web Service plan (containers allowed).
- Registry access (Render registry or external) if using Docker builds.
- Managed database (Render PostgreSQL) instead of SQLite for production.

## Application layout on Render
- **ApiService**: ASP.NET Core API, should listen on `0.0.0.0:${PORT}`; expose `/health` for checks.
- **Web**: ASP.NET Core front end / proxy; configure its API base URL via env var.
- **AppHost**: Local-only for orchestration; do not run in Render. Deploy actual services directly.

## Configuration changes
- Replace Aspire service discovery URLs (e.g., `https+http://apiservice`) with an env-configurable API base URL (e.g., `Api__BaseUrl`).
- Bind Kestrel to Render’s port: `ASPNETCORE_URLS=http://0.0.0.0:${PORT}`.
- Externalize connection strings (e.g., `ConnectionStrings__Default` for Postgres).
- Ensure HTTPS redirect/offloading aligns with Render’s TLS termination (typically keep `UseHttpsRedirection` off when behind proxy if needed).

## Database (Render Postgres)
- Create a Render Postgres instance; grab the connection string.
- Update `ApplicationDbContext` configuration (or `appsettings.Production.json`) to use Postgres provider.
- Add and run EF Core migrations (`dotnet ef migrations add ... && dotnet ef database update`).

## Build & run (Docker path)
- **Dockerfile (ApiService)**: multi-stage `dotnet publish` to a runtime image. Expose `PORT` env. Entry: `dotnet SimpleBlog.ApiService.dll`.
- **Dockerfile (Web)**: similar publish; set API base URL env var at runtime.
- Render service config: Build command `docker build -t ... .`; Start command `docker run -p ${PORT}:${PORT} ...` (Render injects `PORT`).

## Build & run (Render .NET runtime path)
- Build command: `dotnet restore && dotnet publish SimpleBlog.ApiService/SimpleBlog.ApiService.csproj -c Release -o out` (adjust for Web if deploying that service).
- Start command: `dotnet out/SimpleBlog.ApiService.dll`.
- Set env vars: `PORT`, `ASPNETCORE_URLS=http://0.0.0.0:${PORT}`, API base URL (for Web), connection string.

## Health checks
- Ensure `/health` is mapped in ApiService and Web; configure Render health check URL to `/health` with reasonable timeout.

## Environment variables (examples)
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://0.0.0.0:${PORT}`
- `ConnectionStrings__Default=<Render Postgres URL>`
- `Api__BaseUrl=https://<your-api>.onrender.com` (consumed by Web client)

## Migration from Aspire local to Render
- Keep Aspire/AppHost for local dev and testing.
- For production, build/publish the individual service projects (ApiService/Web) and deploy them independently to Render.

## Next steps
- Add/update API base URL configuration in Web to read from env vars.
- Add Postgres provider and migrations; test locally against Postgres before deploying.
- Author Dockerfiles (or runtime scripts) per service and wire Render service settings.
