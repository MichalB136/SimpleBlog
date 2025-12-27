# SimpleBlog Database Configuration

This guide explains the database setup options for SimpleBlog development.

## Current Setup

SimpleBlog supports **3 database backends**:

| Database | Location | Best For | Setup Time |
|----------|----------|----------|------------|
| **SQLite** (Current) | Local file `simpleblog.db` | Quick prototyping | < 1 min |
| **SQL Server 2022** | Docker container | Production-like testing | 30 sec + 30 sec startup |
| **PostgreSQL 16** | Docker container | Open source alternative | 30 sec + 10 sec startup |

## Quick Start by Scenario

### Scenario 1: Fast Local Development (No Database Overhead)
**You want:** Persistent local database, no Docker
**Setup:**
```bash
cd c:\Code\SimpleBlog
dotnet run --project SimpleBlog.AppHost
```
✅ Uses: SQLite (`simpleblog.db` file)
✅ Data persists between runs
✅ No Docker required
⏱️ Ready: Immediate

### Scenario 2: SQL Server Development (Production-like)
**You want:** Containerized SQL Server matching production
**Setup:**
```bash
# 1. Start SQL Server container (30 sec)
docker-compose --profile sql-server up -d

# 2. Update appsettings.Development.json
# Change connection string to SQL Server (see section below)

# 3. Run application
dotnet run --project SimpleBlog.AppHost
```
✅ Uses: SQL Server 2022 in Docker
✅ Network isolated
✅ Easy to share setup with team
⏱️ Ready: ~1 minute (startup delay)

### Scenario 3: PostgreSQL Development (Open Source)
**You want:** Lightweight open-source database
**Setup:**
```bash
# 1. Start PostgreSQL container
docker-compose --profile postgres up -d

# 2. Update appsettings.Development.json
# Change connection string to PostgreSQL (see section below)

# 3. Run application
dotnet run --project SimpleBlog.AppHost
```
✅ Uses: PostgreSQL 16 in Docker
✅ Minimal resource usage
✅ Cross-platform friendly
⏱️ Ready: ~30 seconds

## Database Configuration

### Using SQLite (Default - No Changes Needed)
✅ Already configured in `SimpleBlog.ApiService/Program.cs`
- Database file: `c:\Code\SimpleBlog\simpleblog.db`
- Connection: Built-in SQLite provider
- No configuration needed

### Switching to SQL Server

**Step 1:** Start the container
```bash
docker-compose --profile sql-server up -d
```

Wait for "healthy" status:
```bash
docker ps  # Look for "simpleblog-sqlserver (healthy)"
```

**Step 2:** Update connection string in `SimpleBlog.ApiService/appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;User Id=sa;Password=Admin@12345;Database=SimpleBlogDb;TrustServerCertificate=True;"
  }
}
```

**Step 3:** Run application
```bash
dotnet run --project SimpleBlog.AppHost
```

**Step 4 (Optional):** Apply migrations
```bash
cd SimpleBlog.ApiService
dotnet ef database update
```

### Switching to PostgreSQL

**Step 1:** Start the container
```bash
docker-compose --profile postgres up -d
```

**Step 2:** Update connection string in `SimpleBlog.ApiService/appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Username=simpleblog;Password=Admin@12345;Database=SimpleBlogDb;"
  }
}
```

**Step 3:** Run application
```bash
dotnet run --project SimpleBlog.AppHost
```

**Step 4 (Optional):** Apply migrations
```bash
cd SimpleBlog.ApiService
dotnet ef database update
```

## Docker Commands Reference

### Container Management
```bash
# Start SQL Server
docker-compose --profile sql-server up -d

# Start PostgreSQL
docker-compose --profile postgres up -d

# Start both
docker-compose --profile sql-server --profile postgres up -d

# View running containers
docker-compose ps

# View logs
docker-compose logs -f sqlserver

# Stop all (keeps data)
docker-compose stop

# Stop and remove containers (keeps data)
docker-compose down

# Stop and delete everything including data
docker-compose down -v
```

### Database Access

**SQL Server via SSMS/Azure Data Studio:**
- Host: `localhost,1433`
- User: `sa`
- Password: `Admin@12345`

**PostgreSQL via pgAdmin/DBeaver:**
- Host: `localhost:5432`
- User: `simpleblog`
- Password: `Admin@12345`
- Database: `SimpleBlogDb`

## Project Structure

```
SimpleBlog/
├── docker-compose.yml              # Docker services configuration
├── DOCKER_QUICK_START.md           # Quick reference (30 sec setup)
├── DOCKER_COMPOSE_SETUP.md         # Complete documentation
├── DATABASE_SETUP.md               # This file
├── PERSISTENT_DB.md                # SQLite persistence info
├── .env.example                    # Environment template
│
├── SimpleBlog.ApiService/
│   ├── appsettings.Development.json    # Database connection string
│   ├── appsettings.json
│   ├── Program.cs                      # Database initialization
│   └── Data/
│       ├── ApplicationDbContext.cs     # EF Core DbContext
│       └── Entities.cs
│
└── docker/
    └── mssql/
        └── init/
            └── 01-init.sql         # SQL Server init script
```

## Common Issues & Solutions

### Issue: "Connection refused"
```
Connection string: Server=localhost,1433...
Error: Cannot connect to server
```
**Solution:**
1. Check container is running: `docker-compose ps`
2. Wait 30 seconds for SQL Server startup
3. Verify port 1433 is available: `netstat -an | findstr 1433`
4. Check logs: `docker-compose logs sqlserver`

### Issue: "Port 1433 already in use"
```
Error: Cannot start container, port is already in use
```
**Solution:**
1. Find process using port: `netstat -ano | findstr :1433`
2. Kill process: `taskkill /PID <number> /F`
3. Or use different port in docker-compose.yml: `"1434:1433"`

### Issue: "Cannot connect after switching databases"
**Solution:**
1. Ensure container is running: `docker-compose ps`
2. Verify connection string in `appsettings.Development.json`
3. Restart application: `dotnet run --project SimpleBlog.AppHost`
4. Check logs for connection errors

### Issue: "Database already exists error"
**Solution:**
The application handles this automatically with `EnsureCreated()`:
- First run: Creates database and schema
- Subsequent runs: Uses existing database
- No conflicts or errors expected

### Issue: "Data disappeared after docker-compose down"
⚠️ **Important:** Only happens with `docker-compose down -v` (removes volumes)

**Solution:** Use `docker-compose down` instead (keeps data)
```bash
docker-compose down      # Stops containers, keeps data
docker-compose down -v   # Removes everything including data
```

## Development Workflow

### Daily Start
```bash
# 1. Start database (skip if using SQLite)
docker-compose --profile sql-server up -d

# 2. Wait for "healthy" status
docker ps

# 3. Run application
dotnet run --project SimpleBlog.AppHost
```

### Daily End
```bash
# Stop application (Ctrl+C)

# Stop database (data persists)
docker-compose stop
```

### Full Cleanup (Keep Data)
```bash
docker-compose down
```

### Full Cleanup (Delete Data)
```bash
docker-compose down -v
```

## Production Considerations

⚠️ **Docker Compose is for development only.** For production:

### Option 1: Managed Database Services
- **SQL Server:** Azure SQL Database, AWS RDS
- **PostgreSQL:** Azure Database for PostgreSQL, AWS RDS
- **SQLite:** Not recommended (single-file, limited concurrency)

### Option 2: Kubernetes
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: simpleblog-db
data:
  connection-string: "Server=sqlserver.default:1433;..."
```

### Option 3: Docker Swarm
```bash
docker stack deploy -c docker-compose.yml simpleblog
```

## Files Reference

- **[docker-compose.yml](docker-compose.yml)** - Service definitions
- **[DOCKER_QUICK_START.md](DOCKER_QUICK_START.md)** - 30-second setup guide
- **[DOCKER_COMPOSE_SETUP.md](DOCKER_COMPOSE_SETUP.md)** - Complete Docker reference
- **[PERSISTENT_DB.md](PERSISTENT_DB.md)** - SQLite persistence details
- **[.env.example](.env.example)** - Environment variables template

## Next Steps

1. **Choose your database:** SQLite (default), SQL Server, or PostgreSQL
2. **Start containers** (if using Docker): `docker-compose --profile sql-server up -d`
3. **Update connection string** (if needed): Edit `appsettings.Development.json`
4. **Run application:** `dotnet run --project SimpleBlog.AppHost`
5. **Verify:** Open https://localhost:7030

## Support

For issues:
1. Check logs: `docker-compose logs -f`
2. Review connection string in appsettings.json
3. Ensure container is healthy: `docker ps`
4. Try stopping and restarting: `docker-compose restart sqlserver`

For detailed information, see [DOCKER_COMPOSE_SETUP.md](DOCKER_COMPOSE_SETUP.md)
