# Render Deployment Guide

> ## Document Metadata
> 
> ### ✅ Required
> **Title:** Render Deployment Guide - SimpleBlog Production Deployment  
> **Description:** Complete guide for deploying SimpleBlog to Render.com including PostgreSQL database, API service, and web frontend configuration  
> **Audience:** developer, devops  
> **Topic:** deployment  
> **Last Update:** 2026-01-17
>
> ### 📌 Recommended
> **Parent Document:** [README.md](./README.md)  
> **Difficulty:** intermediate  
> **Estimated Time:** 45 min  
> **Version:** 1.0.0  
> **Status:** approved
>
> ### 🏷️ Optional
> **Prerequisites:** Render.com account, GitHub/GitLab repository, Docker knowledge, Basic PostgreSQL understanding  
> **Related Docs:** [../development/database-guide.md](../development/database-guide.md), [../technical/architecture-overview.md](../technical/architecture-overview.md)  
> **Tags:** `deployment`, `render`, `postgresql`, `docker`, `production`, `devops`

---

## 📋 Overview

SimpleBlog uses **.NET Aspire** for local development orchestration but deploys to **Render.com** as individual **Web Services** with Docker containers for production. This guide walks through both automated (Blueprint) and manual deployment approaches.

### Why Render Web Services?

> ⚠️ **Important:** Render **Static Sites** are not sufficient for SimpleBlog. Static Sites only serve pre-built HTML/CSS/JS over a CDN and cannot run ASP.NET Core applications. You **must use Render Web Services** for both API and Web components.

**Static Sites:** Only host static files (HTML, CSS, JS) on CDN - no server-side execution  
**Web Services:** Run ASP.NET Core applications with server-side rendering, APIs, and dynamic content

SimpleBlog requires server-side processing for both API and Web proxy, making Web Services the correct choice.

---

## 🏗️ Architecture on Render

SimpleBlog deploys as **three components**:

```
┌─────────────────────────────────────────────────────────┐
│                    Render Platform                       │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────────┐      ┌──────────────────┐        │
│  │   Web Frontend   │      │    API Service   │        │
│  │  (Web Service)   │─────▶│  (Web Service)   │        │
│  │  ASP.NET Core    │      │  ASP.NET Core    │        │
│  │  Docker          │      │  Docker          │        │
│  └──────────────────┘      └────────┬─────────┘        │
│                                     │                   │
│                                     ▼                   │
│                            ┌──────────────────┐        │
│                            │   PostgreSQL DB  │        │
│                            │ (Managed Service)│        │
│                            └──────────────────┘        │
│                                                          │
└─────────────────────────────────────────────────────────┘

Public Internet
     │
     ▼
  HTTPS (TLS termination at Render load balancer)
```

**Components:**
1. **PostgreSQL Database** - Managed Render PostgreSQL instance
2. **API Service** - ASP.NET Core Web Service (Docker container)
3. **Web Frontend** - ASP.NET Core Web Service (Docker container) serving React/Blazor UI

---

## ✅ Prerequisites

Before starting deployment:

- ✅ **Render account** (free tier available, Starter plan recommended for production)
- ✅ **GitHub or GitLab repository** with SimpleBlog code
- ✅ **Basic understanding** of Docker and PostgreSQL
- ✅ **Strong JWT secret** ready (min 32 characters)

---

## 🚀 Deployment Options

### Option 1: Blueprint Deployment (Recommended)

Use the included `render.yaml` Infrastructure as Code file to deploy all services at once.

#### Steps:

1. **Push code to GitHub/GitLab**
   ```bash
   git push origin main
   ```

2. **Create Blueprint Instance**
   - Go to Render Dashboard
   - Navigate to **Blueprints** → **New Blueprint Instance**
   - Connect your repository

3. **Auto-Detection**
   - Render detects `render.yaml` in repository root
   - Shows preview of all services to be created

4. **Review Configuration**
   - Check environment variables
   - Verify service names
   - Confirm pricing tier

5. **Deploy**
   - Click **Apply**
   - Render creates all services automatically

#### What Gets Created:

- PostgreSQL database (`simpleblog-db`)
- API service (`simpleblog-api`)
- Web frontend service (`simpleblog-web`)
- Automatic service discovery between components

---

### Option 2: Manual Deployment

Deploy each service individually through the Render Dashboard.

#### Step 1: Create PostgreSQL Database

1. **Create Database**
   - Dashboard → **New** → **PostgreSQL**
   - **Name:** `simpleblog-db`
   - **Database:** `simpleblog`
   - **Region:** Choose closest to your users
   - **Plan:** Starter ($7/month) or Free (with limitations)

2. **Click Create Database**

3. **Save Connection String**
   - Copy **Internal Database URL** from database settings
   - Format: `postgresql://user:password@host:5432/dbname`

> 💡 **Tip:** Use the Internal Database URL, not External. Internal URLs are faster and more secure within Render's network.

---

#### Step 2: Deploy API Service

1. **Create Web Service**
   - Dashboard → **New** → **Web Service**
   - Connect your repository

2. **Basic Configuration**
   ```
   Name:              simpleblog-api
   Region:            Same as database
   Branch:            main
   Root Directory:    .
   Environment:       Docker
   Dockerfile Path:   SimpleBlog.ApiService/Dockerfile
   Docker Context:    .
   Plan:              Starter ($7/month minimum)
   ```

3. **Advanced Settings**
   ```
   Health Check Path:  /health
   Auto-Deploy:        Yes
   ```

4. **Environment Variables**
   ```env
   ASPNETCORE_ENVIRONMENT=Production
   Database__Provider=postgresql
   ConnectionStrings__Default=<paste Internal Database URL from Step 1>
   Jwt__Key=<generate-strong-random-key-at-least-32-chars>
   Jwt__Issuer=SimpleBlog
   Jwt__Audience=SimpleBlog
   ```

   > 🔒 **Security:** Generate a cryptographically secure JWT key with at least 32 characters.

5. **Create Web Service**
   - Click **Create Web Service**
   - Wait for initial deployment to complete

---

#### Step 3: Deploy Web Frontend

1. **Create Web Service**
   - Dashboard → **New** → **Web Service**
   - Connect same repository as API

2. **Basic Configuration**
   ```
   Name:              simpleblog-web
   Region:            Same as API and database
   Branch:            main
   Root Directory:    .
   Environment:       Docker
   Dockerfile Path:   SimpleBlog.Web/Dockerfile
   Docker Context:    .
   Plan:              Starter ($7/month)
   ```

3. **Advanced Settings**
   ```
   Health Check Path:  /health
   Auto-Deploy:        Yes
   ```

4. **Environment Variables**
   ```env
   ASPNETCORE_ENVIRONMENT=Production
   API_BASE_URL=https://simpleblog-api.onrender.com
   ```
   
   > ⚠️ Replace with your actual API service URL from Step 2

5. **Create Web Service**
   - Click **Create Web Service**
   - Wait for deployment

---

#### Step 4: Update API CORS

After Web service is created, update API environment variables:

1. Go to API service settings
2. Add environment variable:
   ```env
   Cors__AllowedOrigins__0=https://simpleblog-web.onrender.com
   ```
3. **Manual Deploy** API service for CORS changes to take effect

---

## ⚙️ Configuration Details

### Database Configuration

The application automatically detects PostgreSQL based on:

- `Database__Provider=postgresql` environment variable
- **OR** connection string containing "postgres"

PostgreSQL provider is included via `Npgsql.EntityFrameworkCore.PostgreSQL` package.

---

### PORT Binding

Render injects a `PORT` environment variable. The application automatically binds to `http://0.0.0.0:${PORT}` when `PORT` is set.

**Implementation in Program.cs:**
```csharp
if (Environment.GetEnvironmentVariable("PORT") is string port)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
```

This configuration is present in both `SimpleBlog.ApiService/Program.cs` and `SimpleBlog.Web/Program.cs`.

---

### Service Discovery

| Environment | Discovery Method |
|-------------|------------------|
| **Local (Aspire)** | `https+http://apiservice` URL scheme |
| **Render** | Explicit API URL via `API_BASE_URL` environment variable |

Application checks `API_BASE_URL` env var first, falls back to Aspire discovery for local development.

---

### HTTPS Considerations

- **TLS Termination:** Render terminates TLS at the load balancer level
- **Internal Traffic:** Your application receives HTTP traffic internally
- **Middleware:** `UseHttpsRedirection()` is safe to keep - it redirects external HTTP to HTTPS via Render's proxy headers

---

## 🔄 Database Migrations

### Initial Setup

After deploying for the first time, the database schema is created automatically via `EnsureCreated()`.

**For production environments, use migrations:**

```bash
# Local development - create migration
dotnet ef migrations add InitialCreate `
    --project SimpleBlog.ApiService `
    --context ApplicationDbContext

# Apply migrations manually (from Render Shell or locally)
dotnet ef database update `
    --project SimpleBlog.ApiService `
    --context ApplicationDbContext `
    --connection "<connection-string>"
```

---

### Automated Migrations (Advanced)

Add a migration step to the Dockerfile or create a Render Cron Job that runs migrations before app startup.

**Example Dockerfile modification:**
```dockerfile
# In SimpleBlog.ApiService/Dockerfile before ENTRYPOINT
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
```

Then create a startup script that runs migrations first.

---

## 📊 Monitoring & Health Checks

### Health Endpoints

Both services expose `/health` endpoints:

- **API:** `https://simpleblog-api.onrender.com/health`
- **Web:** `https://simpleblog-web.onrender.com/health`

Render uses these for health checks (configured in service settings).

---

### Logging

**View logs in Render Dashboard:**

1. Select service → **Logs** tab
2. Filter by log level, search text
3. Logs retained for 7 days (free tier), longer on paid plans

---

### Metrics

**Render Dashboard provides:**

- ✅ CPU usage
- ✅ Memory usage
- ✅ Response times
- ✅ HTTP status codes
- ✅ Bandwidth usage

---

## 🐛 Troubleshooting

### Problem: Service Won't Start

**Symptoms:**
- Deployment fails
- Service shows "Deploying" indefinitely
- Red status indicator

**Solutions:**

1. **Check Logs**
   - Render Dashboard → Service → Logs tab

2. **Common Issues:**
   - ❌ Database connection string incorrect
   - ❌ Missing environment variables
   - ❌ PORT binding not configured
   - ❌ Docker build errors

**Verification:**
```bash
# Check Dockerfile builds locally
docker build -f SimpleBlog.ApiService/Dockerfile .
```

---

### Problem: Database Connection Errors

**Symptoms:**
- "Could not connect to database" errors
- Timeout exceptions
- Connection refused

**Solutions:**

1. **Verify connection string format:**
   ```
   postgresql://user:password@host:5432/dbname
   ```

2. **Use Internal Database URL** (not External)

3. **Check database status** in Render Dashboard

4. **Verify environment variable:**
   ```env
   Database__Provider=postgresql
   ```

**Verification:**
```bash
# Test connection using psql (if available locally)
psql "<connection-string>"
```

---

### Problem: CORS Errors

**Symptoms:**
- Browser console shows CORS errors
- Preflight requests fail
- 403 Forbidden from API

**Solutions:**

1. **Verify CORS configuration:**
   ```env
   Cors__AllowedOrigins__0=https://simpleblog-web.onrender.com
   ```

2. Include both `http://` and `https://` if needed

3. Don't include trailing slashes

4. **Redeploy API** after changing CORS settings

**Verification:**
```bash
# Test CORS with curl
curl -H "Origin: https://simpleblog-web.onrender.com" `
     -H "Access-Control-Request-Method: GET" `
     -H "Access-Control-Request-Headers: Content-Type" `
     -X OPTIONS `
     https://simpleblog-api.onrender.com/api/posts
```

---

### Problem: Health Check Failures

**Symptoms:**
- Service marked as unhealthy
- Constant restarts
- 503 Service Unavailable

**Solutions:**

1. Ensure `/health` endpoint is accessible

2. Check that service is binding to correct PORT

3. Verify health check path in service settings

4. Increase timeout if startup is slow (Advanced Settings)

**Verification:**
```bash
# Test health endpoint
curl https://simpleblog-api.onrender.com/health
```

---

## 💰 Cost Optimization

### Free Tier Limitations

- ⚠️ Services spin down after **15 minutes** of inactivity
- ⚠️ First request after spin-down takes **30-60 seconds** (cold start)
- ⚠️ 750 hours/month free compute (across all services)
- ⚠️ Not suitable for production

---

### Starter Plan ($7/month per service)

- ✅ Always-on services (no cold starts)
- ✅ Better performance
- ✅ **Total cost:** ~$21/month (API + Web + Database)

---

## 🔒 Security Best Practices

1. **JWT Secret:** Generate cryptographically secure key (min 32 characters)
   ```bash
   # Generate random key (PowerShell)
   -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})
   ```

2. **Connection Strings:** Use Render's encrypted environment variables

3. **CORS:** Restrict to specific origins (don't use wildcards in production)

4. **HTTPS Only:** Render provides automatic TLS certificates

5. **Database:** Use strong passwords, restrict access to internal network

---

## 📈 Scaling

Render Web Services can scale horizontally:

1. Service Settings → **Scaling**
2. Increase instance count (paid plans only)
3. Database can be upgraded to higher plans
4. Consider adding Redis for session state if scaling beyond 2-3 instances

---

## 🌐 Custom Domains

1. Service → **Settings** → **Custom Domains**
2. Add your domain (e.g., `blog.example.com`)
3. Update DNS records with your provider:
   - Add **CNAME** record pointing to Render URL
4. Render automatically provisions TLS certificate

---

## 🔄 CI/CD & Auto-Deploy

- **Auto-Deploy:** Enabled by default - pushes to your branch trigger deployments
- **Manual Deploy:** Dashboard → Service → **Manual Deploy** → Select commit
- **Deploy Hooks:** Generate webhook URL for external CI/CD triggers
- **Preview Environments:** Create PR previews for testing (Pro plan)

---

## 📊 Differences from Local Development

| Aspect | Local (Aspire) | Render (Production) |
|--------|----------------|---------------------|
| **Orchestration** | AppHost project | Manual/Blueprint |
| **Database** | PostgreSQL container | Managed PostgreSQL |
| **Service Discovery** | Automatic | Environment variables |
| **Port Binding** | Dynamic (Aspire) | PORT env var |
| **HTTPS** | Developer certificates | Render-managed TLS |
| **Secrets** | appsettings/user-secrets | Environment variables |

---

## ✅ Post-Deployment Checklist

After successful deployment:

- [ ] Test all API endpoints via API service URL
- [ ] Verify Web service can reach API
- [ ] Check database connectivity
- [ ] Run through authentication flow (login/register)
- [ ] Monitor logs for errors
- [ ] Set up custom domain (optional)
- [ ] Configure monitoring/alerts
- [ ] Test CORS from Web frontend
- [ ] Verify health check endpoints

---

## 🔗 External Resources

- [Render Docs - Web Services](https://render.com/docs/web-services)
- [Render Docs - PostgreSQL](https://render.com/docs/postgresql)
- [Render Docs - Docker](https://render.com/docs/docker)
- [Render Docs - Environment Variables](https://render.com/docs/configure-environment-variables)
- [Render Blueprint Spec](https://render.com/docs/blueprint-spec)

---

## 💬 Support

- **Render Community Forum:** https://community.render.com/
- **Render Discord:** Available via dashboard
- **SimpleBlog Issues:** Create issue in GitHub repository

---

## 📚 Related Documents

- [Database Guide](../development/database-guide.md) - PostgreSQL setup and migrations
- [Architecture Overview](../technical/architecture-overview.md) - System architecture
- [Getting Started](../development/getting-started.md) - Local development setup

---

## 📝 Changelog

| Date | Version | Changes |
|------|---------|---------|
| 2026-01-17 | 1.0.0 | Converted from legacy RENDER_DEPLOYMENT.md, added proper metadata |
