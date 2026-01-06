# Docker Configuration for SimpleBlog

## 📦 Database Architecture

SimpleBlog uses **PostgreSQL** as the primary database, managed externally via **docker-compose**.

**Architecture Philosophy:**
- 🔹 Development mirrors production
- 🔹 Manual database lifecycle management
- 🔹 Explicit infrastructure control
- 🔹 No automatic initialization

### Manual PostgreSQL Management (Required)

**You must start PostgreSQL before running the application:**

```powershell
# Start PostgreSQL and pgAdmin
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f postgres
```

---

## 🚀 Quick Start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- .NET 9.0 SDK
- .NET Aspire workload installed

### Step 1: Start PostgreSQL

```powershell
# From project root
docker-compose up -d
```

### Step 2: Start Application

```powershell
# Using script
.\start.ps1

# Or manually
dotnet run --project SimpleBlog.AppHost
```

The application will:
- 🔄 Automatically apply all database migrations
- 🏗️ Create/update database schema
- 📊 Be ready to use

No manual migration commands needed!

### Access Services

- **PostgreSQL Database:**
  - Host: `localhost`
  - Port: `5432`
  - Database: `simpleblog`
  - Username: `simpleblog_user`
  - Password: `simpleblog_dev_password_123`

- **pgAdmin (Database GUI):**
  - URL: http://localhost:5050
  - Email: `admin@simpleblog.local`
  - Password: `admin`

- **Aspire Dashboard:**
  - URL shown in console when running AppHost
  - Monitors application services (not database)

---

## 🔧 Alternative: Manual Docker Compose

If you want to run PostgreSQL independently (without Aspire managing it):

### Start PostgreSQL Manually

```powershell
# Start PostgreSQL + pgAdmin
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f postgres
```

### Manual Connection Details

- **PostgreSQL Database:**
  - Host: `localhost`
  - Port: `5432`
  - Database: `simpleblog`
  - Username: `simpleblog_user`
  - Password: `simpleblog_dev_password_123`

- **pgAdmin (Database GUI):**
  - URL: http://localhost:5050
  - Email: `admin@simpleblog.local`
  - Password: `admin`

**Note:** When using manual docker-compose, you need to update connection strings in `appsettings.shared.Development.json` to match the manual setup.

// Use external PostgreSQL from docker-compose
var postgres = builder.AddPostgres("postgres")
    .WithEndpoint("localhost", 5432)
    .WithEnvironment("POSTGRES_USER", "simpleblog_user")
    .WithEnvironment("POSTGRES_PASSWORD", "simpleblog_dev_password_123");

var blogDb = postgres.AddDatabase("simpleblog");

var apiService = builder.AddProject<Projects.SimpleBlog_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithReference(blogDb);

builder.AddProject<Projects.SimpleBlog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

await builder.Build().RunAsync();
```

Or manually set connection string in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "BlogDb": "Host=localhost;Port=5432;Database=simpleblog;Username=simpleblog_user;Password=simpleblog_dev_password_123"
  }
}
```

### Run Application with PostgreSQL

```powershell
# 1. Start Docker containers
docker-compose up -d

# 2. Run application
dotnet run --project SimpleBlog.AppHost
```

---

## 📊 pgAdmin Setup

1. Open http://localhost:5050
2. Login with credentials from above
3. **Add Server:**
   - Right-click "Servers" → Register → Server
   - **General Tab:**
     - Name: `SimpleBlog Local`
   - **Connection Tab:**
     - Host: `postgres` (use container name when pgAdmin is in same network)
     - Port: `5432`
     - Database: `simpleblog`
     - Username: `simpleblog_user`
     - Password: `simpleblog_dev_password_123`
   - Save

---

## 🔧 Management Commands

### Start Services
```powershell
docker-compose up -d
```

### Stop Services
```powershell
docker-compose stop
```

### Stop and Remove Everything
```powershell
docker-compose down
```

### Remove Everything Including Data
```powershell
docker-compose down -v
```

### View Logs
```powershell
# All services
docker-compose logs -f

# PostgreSQL only
docker-compose logs -f postgres

# pgAdmin only
docker-compose logs -f pgadmin
```

### Restart Services
```powershell
docker-compose restart
```

### Check Service Health
```powershell
docker-compose ps
```

---

## 🗄️ Database Management

### Backup Database
```powershell
docker exec simpleblog-postgres pg_dump -U simpleblog_user simpleblog > backup.sql
```

### Restore Database
```powershell
Get-Content backup.sql | docker exec -i simpleblog-postgres psql -U simpleblog_user -d simpleblog
```

### Access PostgreSQL CLI
```powershell
docker exec -it simpleblog-postgres psql -U simpleblog_user -d simpleblog
```

Common SQL commands:
```sql
-- List all tables
\dt

-- Describe table structure
\d "Posts"

-- View all posts
SELECT * FROM "Posts";

-- Exit
\q
```

---

## 🔄 Switching Between SQLite and PostgreSQL

### Current: SQLite
- **Location:** `SimpleBlog.ApiService/simpleblog.db`
- **Provider:** `Microsoft.EntityFrameworkCore.Sqlite`
---

## 🐛 Troubleshooting

### Aspire Can't Start PostgreSQL
```powershell
# Check if Docker Desktop is running
docker ps

# Check if port 5432 is already in use
netstat -ano | findstr :5432

# View Aspire logs in dashboard
# Or check container logs directly
docker logs <postgres-container-id>
```

### Connection Issues
- Ensure Docker Desktop is running
- Check Aspire Dashboard for actual port numbers (may be dynamic)
- Verify connection string in Aspire Dashboard resources section

### Database Migration Errors
```powershell
# Remove volumes and restart
docker-compose down -v
docker-compose up -d
```

### Can't Connect to Database
```powershell
# Check container is healthy
docker-compose ps

# Test connection
docker exec simpleblog-postgres pg_isready -U simpleblog_user
```

### pgAdmin Can't Connect
- Use host `postgres` (container name) not `localhost` when pgAdmin is in Docker
- Verify credentials match `.env` file

---

## 📝 Configuration Files

- **docker-compose.yml** - PostgreSQL + pgAdmin setup
- **docker-compose.dev.yml** - SQLite development (no containers)
- **.env.example** - Environment variables template
- **docker/postgres-init/01-init.sql** - Basic PostgreSQL extensions setup

### Database Initialization

The `postgres-init/01-init.sql` script runs once when PostgreSQL container starts for the first time. It only creates necessary PostgreSQL extensions:

```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";  -- UUID generation
CREATE EXTENSION IF NOT EXISTS "pg_trgm";    -- Text search
```

**Important:** The application (Entity Framework Core) manages all database schema, tables, and data:
- ✅ Applies pending migrations automatically on startup
- ✅ Creates database schema if it doesn't exist
- ✅ Seeds initial data (posts, products) if database is empty

This approach ensures:
- 🔹 Database changes are version-controlled through EF migrations
- 🔹 Consistent schema across development and production
- 🔹 Easy rollback and testing of database changes

---

## 🔒 Security Notes

**Development credentials are exposed for convenience.**

For production:
- ✅ Use strong passwords
- ✅ Store credentials in Azure Key Vault or similar
- ✅ Use managed databases (Render PostgreSQL, Azure Database, etc.)
- ✅ Enable SSL/TLS connections
- ✅ Restrict network access

---

## 📚 Related Documentation

- [DATABASES.md](../docs/DATABASES.md) - Database architecture
- [RENDER_DEPLOYMENT.md](../docs/RENDER_DEPLOYMENT.md) - Production deployment
- [README.md](../README.md) - General project documentation
