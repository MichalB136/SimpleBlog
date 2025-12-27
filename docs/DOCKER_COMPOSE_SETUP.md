# Docker Compose Setup - SimpleBlog

## Overview

This `docker-compose.yml` provides containerized database services for SimpleBlog development:
- **SQL Server 2022** - Default enterprise-grade database
- **PostgreSQL 16** - Lightweight alternative
- **Redis** - Optional caching layer

## Quick Start

### Option 1: Start SQL Server (Default)
```bash
docker-compose --profile sql-server up -d
```

This starts:
- SQL Server 2022 on `localhost:1433`
- SA username: `sa`
- SA password: `Admin@12345`
- Database: Auto-created by application

### Option 2: Start PostgreSQL
```bash
docker-compose --profile postgres up -d
```

This starts:
- PostgreSQL 16 on `localhost:5432`
- Username: `simpleblog`
- Password: `Admin@12345`
- Database: `SimpleBlogDb`

### Option 3: Start Both (SQL Server + PostgreSQL)
```bash
docker-compose --profile sql-server --profile postgres up -d
```

### Option 4: Start Everything (with Redis)
```bash
docker-compose --profile sql-server --profile postgres --profile optional up -d
```

## Configuration

### SQL Server Connection String
```
Server=localhost,1433;User Id=sa;Password=Admin@12345;Database=SimpleBlogDb;TrustServerCertificate=True;
```

Update in `SimpleBlog.ApiService/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;User Id=sa;Password=Admin@12345;Database=SimpleBlogDb;TrustServerCertificate=True;"
  }
}
```

### PostgreSQL Connection String
```
Host=localhost;Port=5432;Username=simpleblog;Password=Admin@12345;Database=SimpleBlogDb;
```

Update in `SimpleBlog.ApiService/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Username=simpleblog;Password=Admin@12345;Database=SimpleBlogDb;"
  }
}
```

### Redis Connection String (Optional)
```
localhost:6379
```

## Managing Containers

### View Running Containers
```bash
docker-compose ps
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f sqlserver
docker-compose logs -f postgres
```

### Stop All Services
```bash
docker-compose down
```

### Stop and Remove Data
```bash
docker-compose down -v
```

### Restart Services
```bash
docker-compose restart sqlserver
```

## Database Access

### SQL Server (SSMS or Azure Data Studio)
1. **Server:** `localhost,1433`
2. **Authentication:** SQL Server Authentication
3. **Username:** `sa`
4. **Password:** `Admin@12345`

### PostgreSQL (pgAdmin or DBeaver)
1. **Host:** `localhost`
2. **Port:** `5432`
3. **Username:** `simpleblog`
4. **Password:** `Admin@12345`
5. **Database:** `SimpleBlogDb`

## Switching Between Databases

### From SQLite to SQL Server
1. Start SQL Server: `docker-compose --profile sql-server up -d`
2. Update connection string in `appsettings.Development.json`
3. Restart application
4. Run migrations: `dotnet ef database update`

### From SQL Server to PostgreSQL
1. Start PostgreSQL: `docker-compose --profile postgres up -d`
2. Update connection string in `appsettings.Development.json`
3. Restart application
4. Run migrations: `dotnet ef database update`

## Health Checks

Services include health checks:
```bash
# View container health
docker ps --format "table {{.Names}}\t{{.Status}}"
```

Example output:
```
NAMES                     STATUS
simpleblog-sqlserver      Up 2 minutes (healthy)
simpleblog-postgres       Up 2 minutes (healthy)
simpleblog-redis          Up 2 minutes (healthy)
```

## Troubleshooting

### "Connection refused"
- Ensure container is running: `docker-compose ps`
- Check if port is already in use: `netstat -an | grep 1433`
- Wait for health check to pass (10-30 seconds)

### "Authentication failed"
- Verify credentials in docker-compose.yml
- Verify connection string in appsettings.json
- Restart container: `docker-compose restart sqlserver`

### "Disk space" error
- Clean up unused volumes: `docker volume prune`
- Remove all containers: `docker-compose down -v`

### Container won't start
- Check logs: `docker-compose logs sqlserver`
- Ensure Docker daemon is running: `docker --version`
- Verify 1433/5432 ports are available

## Development Workflow

### Initial Setup
```bash
# 1. Start database
docker-compose --profile sql-server up -d

# 2. Wait for health check
docker ps

# 3. Update connection string in appsettings.Development.json

# 4. Run migrations (if using EF Core)
cd SimpleBlog.ApiService
dotnet ef database update

# 5. Start application
dotnet run --project SimpleBlog.AppHost
```

### Daily Usage
```bash
# Start: once at beginning of day
docker-compose --profile sql-server up -d

# Code and test normally
dotnet build
dotnet test
dotnet run

# Stop: at end of day (data persists in volumes)
docker-compose stop
```

### Complete Cleanup
```bash
# Remove everything (containers, volumes, networks)
docker-compose down -v
```

## Database Persistence

All databases use **named volumes** for data persistence:
- `sqlserver-data` - SQL Server database files
- `postgres-data` - PostgreSQL database files
- `redis-data` - Redis data

**Important:** `docker-compose down` alone **does NOT delete data** - it only stops containers.
Your data persists and is available when you restart: `docker-compose up -d`

To delete data, use: `docker-compose down -v`

## Security Notes

⚠️ **Development Only**
- Credentials are hardcoded (NOT for production)
- Network is open (NOT for production)
- Use secrets management in production

For production deployment, use:
- Azure SQL Managed Instance / AWS RDS
- Azure Key Vault / AWS Secrets Manager
- Docker secrets / Kubernetes secrets

## Common Issues & Solutions

### Port Already in Use
```bash
# Find process using port 1433
netstat -ano | findstr :1433

# Kill process (replace PID with actual number)
taskkill /PID <PID> /F

# OR use different port in docker-compose.yml
# Change: "1433:1433" to "1434:1433"
```

### Container Exits Immediately
```bash
# Check what went wrong
docker-compose logs sqlserver

# Common causes:
# - Port already in use
# - Insufficient disk space
# - Memory constraints
```

### Database Corrupted
```bash
# Backup data (if needed)
docker cp simpleblog-sqlserver:/var/opt/mssql ./backup

# Remove and recreate
docker-compose down -v
docker-compose --profile sql-server up -d
```

## References

- SQL Server Docker: https://hub.docker.com/_/microsoft-mssql-server
- PostgreSQL Docker: https://hub.docker.com/_/postgres
- Redis Docker: https://hub.docker.com/_/redis
- Docker Compose: https://docs.docker.com/compose/
