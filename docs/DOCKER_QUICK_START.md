# Docker Compose Quick Start

## Installation

1. **Install Docker Desktop**
   - Windows: https://www.docker.com/products/docker-desktop
   - Ensure WSL 2 backend is installed

2. **Verify Installation**
   ```bash
   docker --version
   docker-compose --version
   ```

## Start Database in 30 Seconds

### For SQL Server (Recommended)
```bash
cd c:\Code\SimpleBlog
docker-compose --profile sql-server up -d
```

Wait for "healthy" status:
```bash
docker ps
```

### For PostgreSQL
```bash
docker-compose --profile postgres up -d
```

## Connect Application

### Update Connection String
Edit `SimpleBlog.ApiService/appsettings.Development.json`:

**For SQL Server:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;User Id=sa;Password=Admin@12345;Database=SimpleBlogDb;TrustServerCertificate=True;"
  }
}
```

**For PostgreSQL:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Username=simpleblog;Password=Admin@12345;Database=SimpleBlogDb;"
  }
}
```

### Start Application
```bash
cd c:\Code\SimpleBlog
dotnet run --project SimpleBlog.AppHost
```

## Stop Database
```bash
docker-compose stop
```

## Common Commands

```bash
# View status
docker-compose ps

# View logs
docker-compose logs -f sqlserver

# Stop all
docker-compose down

# Remove everything (delete data)
docker-compose down -v

# Restart specific service
docker-compose restart sqlserver
```

## Accessing Database Directly

### SQL Server (SQL Server Management Studio / Azure Data Studio)
- Server: `localhost,1433`
- Username: `sa`
- Password: `Admin@12345`

### PostgreSQL (pgAdmin / DBeaver)
- Host: `localhost:5432`
- Username: `simpleblog`
- Password: `Admin@12345`
- Database: `SimpleBlogDb`

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection refused | Wait 30s for startup, check `docker ps` |
| Port already in use | Change port in docker-compose.yml: `"1434:1433"` |
| Container exits | Run `docker-compose logs sqlserver` to see error |
| Data disappeared | Use `down -v` to remove volumes, or just `down` to keep data |

## File Structure

```
SimpleBlog/
├── docker-compose.yml           # Docker services definition
├── DOCKER_COMPOSE_SETUP.md      # Detailed documentation
├── DOCKER_QUICK_START.md        # This file
├── .env.example                 # Environment variables template
└── docker/
    └── mssql/
        └── init/
            └── 01-init.sql      # SQL Server initialization
```

## Next Steps

1. ✅ Run `docker-compose --profile sql-server up -d`
2. ✅ Update connection string in appsettings.Development.json
3. ✅ Run `dotnet run --project SimpleBlog.AppHost`
4. ✅ Navigate to https://localhost:7030

For detailed documentation, see [DOCKER_COMPOSE_SETUP.md](DOCKER_COMPOSE_SETUP.md)
