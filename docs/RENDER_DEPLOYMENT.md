# Deploying SimpleBlog on Render

## Overview

SimpleBlog uses .NET Aspire for local development orchestration, but for production deployment on Render, we deploy the individual services directly as **Web Services** with Docker containers.

> **Important:** Render Static Sites are **not sufficient** for SimpleBlog. Static Sites only serve pre-built HTML/CSS/JS over a CDN and cannot run ASP.NET Core applications. You **must use Render Web Services** for both API and Web components.

## Architecture on Render

SimpleBlog deploys as three components:

1. **PostgreSQL Database** - Managed Render PostgreSQL instance
2. **API Service** - ASP.NET Core Web Service (Docker container)
3. **Web Frontend** - ASP.NET Core Web Service (Docker container) serving Blazor/React UI

### Why Web Services instead of Static Sites?

- **Static Sites**: Only host static files (HTML, CSS, JS) on a CDN - no server-side execution
- **Web Services**: Run your ASP.NET Core applications with server-side rendering, APIs, and dynamic content
- SimpleBlog requires server-side processing for both API and Web proxy, making Web Services the correct choice

## Prerequisites

- Render account (free tier available, but Starter plan recommended for production)
- GitHub/GitLab repository with SimpleBlog code
- Basic understanding of Docker and PostgreSQL

## Deployment Options

### Option 1: Blueprint Deployment (Recommended)

Use the included `render.yaml` Infrastructure as Code file to deploy all services at once.

**Steps:**
1. Push your code to GitHub/GitLab
2. In Render Dashboard, go to **Blueprints** → **New Blueprint Instance**
3. Connect your repository
4. Render will detect `render.yaml` and create all services automatically
5. Review environment variables and click **Apply**

The blueprint creates:
- PostgreSQL database (`simpleblog-db`)
- API service (`simpleblog-api`) 
- Web frontend service (`simpleblog-web`)
- Automatic service discovery between components

### Option 2: Manual Deployment

Deploy each service individually through the Render Dashboard.

#### Step 1: Create PostgreSQL Database

1. Dashboard → **New** → **PostgreSQL**
2. Name: `simpleblog-db`
3. Database: `simpleblog`
4. Region: Choose closest to your users
5. Plan: Starter ($7/month) or Free (with limitations)
6. Click **Create Database**
7. **Save the connection string** (Internal Database URL)

#### Step 2: Deploy API Service

1. Dashboard → **New** → **Web Service**
2. Connect your repository
3. Configuration:
   - **Name:** `simpleblog-api`
   - **Region:** Same as database
   - **Branch:** `main`
   - **Root Directory:** `.` (repository root)
   - **Environment:** `Docker`
   - **Dockerfile Path:** `SimpleBlog.ApiService/Dockerfile`
   - **Docker Context:** `.`
   - **Plan:** Starter ($7/month minimum)
4. Advanced settings:
   - **Health Check Path:** `/health`
   - **Auto-Deploy:** Yes

5. Environment Variables:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   Database__Provider=postgresql
   ConnectionStrings__Default=<paste Internal Database URL from Step 1>
   Jwt__Key=<generate-strong-random-key-at-least-32-chars>
   Jwt__Issuer=SimpleBlog
   Jwt__Audience=SimpleBlog
   ```

6. Click **Create Web Service**

#### Step 3: Deploy Web Frontend

1. Dashboard → **New** → **Web Service**
2. Connect your repository (same as API)
3. Configuration:
   - **Name:** `simpleblog-web`
   - **Region:** Same as API and database
   - **Branch:** `main`
   - **Root Directory:** `.`
   - **Environment:** `Docker`
   - **Dockerfile Path:** `SimpleBlog.Web/Dockerfile`
   - **Docker Context:** `.`
   - **Plan:** Starter ($7/month)
4. Advanced:
   - **Health Check Path:** `/health`
   - **Auto-Deploy:** Yes

5. Environment Variables:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   API_BASE_URL=<simpleblog-api URL from Step 2>
   ```
   
   Example: `API_BASE_URL=https://simpleblog-api.onrender.com`

6. Click **Create Web Service**

#### Step 4: Update API CORS

After Web service is created, update API environment variables:

```
Cors__AllowedOrigins__0=https://simpleblog-web.onrender.com
```

Redeploy the API service for CORS changes to take effect.

## Configuration Details

### Database Configuration

The application automatically detects PostgreSQL based on:
- `Database__Provider=postgresql` environment variable
- OR connection string containing "postgres"

PostgreSQL provider is included via `Npgsql.EntityFrameworkCore.PostgreSQL` package.

### PORT Binding

Render injects a `PORT` environment variable. The application automatically binds to `http://0.0.0.0:${PORT}` when `PORT` is set. This is configured in both service `Program.cs` files:

```csharp
if (Environment.GetEnvironmentVariable("PORT") is string port)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
```

### Service Discovery

- **Local (Aspire):** Uses `https+http://apiservice` URL scheme
- **Render:** Uses explicit API URL via `API_BASE_URL` environment variable
- Application checks `API_BASE_URL` env var first, falls back to Aspire discovery

### HTTPS Considerations

Render terminates TLS at the load balancer level. Your application receives HTTP traffic internally. The `UseHttpsRedirection()` middleware is safe to keep - it redirects external HTTP to HTTPS via Render's proxy headers.

## Database Migrations

### Initial Setup

After deploying for the first time, the database schema is created automatically via `EnsureCreated()`. For production, consider using migrations:

```bash
# Local development - create migration
dotnet ef migrations add InitialCreate --project SimpleBlog.ApiService --context ApplicationDbContext

# Apply migrations manually (from Render Shell or locally)
dotnet ef database update --project SimpleBlog.ApiService --context ApplicationDbContext --connection "<connection-string>"
```

### Automated Migrations (Advanced)

Add a migration step to the Dockerfile or create a Render Cron Job that runs migrations before app startup:

```dockerfile
# In ApiService/Dockerfile before ENTRYPOINT
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
```

Then create a startup script that runs migrations first.

## Monitoring & Health Checks

### Health Endpoints

Both services expose `/health` endpoints:
- API: `https://simpleblog-api.onrender.com/health`
- Web: `https://simpleblog-web.onrender.com/health`

Render uses these for health checks (configured in service settings).

### Logging

View logs in Render Dashboard:
- Select service → **Logs** tab
- Filter by log level, search text
- Logs are retained for 7 days on free tier, longer on paid plans

### Metrics

Render Dashboard provides:
- CPU usage
- Memory usage
- Response times
- HTTP status codes
- Bandwidth usage

## Troubleshooting

### Service Won't Start

1. **Check Logs:** Render Dashboard → Service → Logs
2. **Common Issues:**
   - Database connection string incorrect
   - Missing environment variables
   - PORT binding not configured
   - Docker build errors

### Database Connection Errors

1. Verify connection string format:
   ```
   postgresql://user:password@host:5432/dbname
   ```
2. Use **Internal Database URL** (not External)
3. Check database status in Render Dashboard
4. Verify `Database__Provider=postgresql` is set

### CORS Errors

1. Verify `Cors__AllowedOrigins__0` includes your Web service URL
2. Include both `http://` and `https://` if needed
3. Don't include trailing slashes
4. Redeploy API after changing CORS settings

### Health Check Failures

1. Ensure `/health` endpoint is accessible
2. Check that service is binding to correct PORT
3. Verify health check path in service settings
4. Increase timeout if startup is slow

## Cost Optimization

### Free Tier Limitations

- Services spin down after 15 minutes of inactivity
- First request after spin-down takes 30-60 seconds (cold start)
- 750 hours/month free compute (across all services)
- Not suitable for production

### Starter Plan ($7/month per service)

- Always-on services (no cold starts)
- Better performance
- Total cost: ~$21/month (API + Web + Database)

## Security Best Practices

1. **JWT Secret:** Generate cryptographically secure key (min 32 characters)
2. **Connection Strings:** Use Render's encrypted environment variables
3. **CORS:** Restrict to specific origins (don't use wildcards in production)
4. **HTTPS Only:** Render provides automatic TLS certificates
5. **Database:** Use strong passwords, restrict access to internal network

## Scaling

Render Web Services can scale horizontally:

1. Service Settings → **Scaling**
2. Increase instance count (paid plans only)
3. Database can be upgraded to higher plans
4. Consider adding Redis for session state if scaling beyond 2-3 instances

## Custom Domains

1. Service → **Settings** → **Custom Domains**
2. Add your domain (e.g., `blog.example.com`)
3. Update DNS records with your provider:
   - Add CNAME record pointing to Render URL
4. Render automatically provisions TLS certificate

## CI/CD & Auto-Deploy

- **Auto-Deploy:** Enabled by default - pushes to your branch trigger deployments
- **Manual Deploy:** Dashboard → Service → **Manual Deploy** → Select commit
- **Deploy Hooks:** Generate webhook URL for external CI/CD triggers
- **Preview Environments:** Create PR previews for testing (Pro plan)

## Differences from Local Development

| Aspect | Local (Aspire) | Render (Production) |
|--------|---------------|---------------------|
| Orchestration | AppHost project | Manual/Blueprint |
| Database | PostgreSQL container | Managed PostgreSQL |
| Service Discovery | Automatic | Environment variables |
| Port Binding | Dynamic (Aspire) | PORT env var |
| HTTPS | Developer certificates | Render-managed TLS |
| Secrets | appsettings/user-secrets | Environment variables |

## Next Steps

After successful deployment:

1. Test all endpoints via API service URL
2. Verify Web service can reach API
3. Check database connectivity
4. Run through authentication flow
5. Monitor logs for errors
6. Set up custom domain (optional)
7. Configure monitoring/alerts

## Additional Resources

- [Render Docs - Web Services](https://render.com/docs/web-services)
- [Render Docs - PostgreSQL](https://render.com/docs/postgresql)
- [Render Docs - Docker](https://render.com/docs/docker)
- [Render Docs - Environment Variables](https://render.com/docs/configure-environment-variables)
- [Render Blueprint Spec](https://render.com/docs/blueprint-spec)

## Support

- Render Community Forum: https://community.render.com/
- Render Discord: Available via dashboard
- SimpleBlog Issues: Create issue in GitHub repository
