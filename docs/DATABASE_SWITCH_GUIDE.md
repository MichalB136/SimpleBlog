# Database Switching Guide

Quick reference for switching between SQLite, SQL Server, and PostgreSQL.

## Option A: SQLite (Default - Already Configured)

```
┌─────────────────────────────────────┐
│  Current Default Setup              │
└─────────────────────────────────────┘

Database: SQLite
Location: c:\Code\SimpleBlog\simpleblog.db
Setup:    ✅ Already configured
Status:   ✅ Ready to use (No setup needed)
Startup:  Immediate

How to use:
  1. Just run the app!
  2. dotnet run --project SimpleBlog.AppHost
  3. Data persists automatically

Files:
  ✓ SimpleBlog.ApiService/Program.cs
  ✓ simpleblog.db (created on first run)
```

## Option B: SQL Server 2022 (Docker)

```
┌─────────────────────────────────────┐
│  Production-Like Environment        │
└─────────────────────────────────────┘

Database:   SQL Server 2022 (Container)
Host:       localhost:1433
Username:   sa
Password:   Admin@12345
Setup Time: 30 seconds + 30 sec startup
Status:     ✅ Ready to deploy

STEP 1: Start SQL Server Container
  docker-compose --profile sql-server up -d
  ⏱️  Wait ~30 seconds for health check

STEP 2: Verify Container is Running
  docker ps
  Look for: simpleblog-sqlserver (healthy)

STEP 3: Update Connection String
  File: SimpleBlog.ApiService/appsettings.Development.json
  
  Change FROM:
  (Remove/comment current connection string)
  
  Change TO:
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost,1433;User Id=sa;Password=Admin@12345;Database=SimpleBlogDb;TrustServerCertificate=True;"
    }
  }

STEP 4: Run Application
  dotnet run --project SimpleBlog.AppHost

STEP 5 (Optional): Apply Migrations
  cd SimpleBlog.ApiService
  dotnet ef database update
  cd ..

Files:
  ✓ docker-compose.yml (profile: sql-server)
  ✓ docker/mssql/init/01-init.sql
  ✓ SimpleBlog.ApiService/appsettings.Development.json (edit)
```

## Option C: PostgreSQL 16 (Docker)

```
┌─────────────────────────────────────┐
│  Open Source Alternative            │
└─────────────────────────────────────┘

Database:   PostgreSQL 16 (Container)
Host:       localhost:5432
Username:   simpleblog
Password:   Admin@12345
Database:   SimpleBlogDb
Setup Time: 30 seconds + 10 sec startup
Status:     ✅ Ready to use

STEP 1: Start PostgreSQL Container
  docker-compose --profile postgres up -d
  ⏱️  Wait ~10 seconds for health check

STEP 2: Verify Container is Running
  docker ps
  Look for: simpleblog-postgres (healthy)

STEP 3: Update Connection String
  File: SimpleBlog.ApiService/appsettings.Development.json
  
  Change FROM:
  (Remove/comment current connection string)
  
  Change TO:
  {
    "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Port=5432;Username=simpleblog;Password=Admin@12345;Database=SimpleBlogDb;"
    }
  }

STEP 4: Run Application
  dotnet run --project SimpleBlog.AppHost

STEP 5 (Optional): Apply Migrations
  cd SimpleBlog.ApiService
  dotnet ef database update
  cd ..

Files:
  ✓ docker-compose.yml (profile: postgres)
  ✓ SimpleBlog.ApiService/appsettings.Development.json (edit)
```

## Switching Workflows

### From SQLite → SQL Server

```
1. docker-compose --profile sql-server up -d
2. Edit appsettings.Development.json
3. Paste SQL Server connection string
4. Save and restart application
5. Done! (New database created automatically)
```

### From SQL Server → SQLite

```
1. docker-compose stop
2. Edit appsettings.Development.json
3. Remove SQL Server connection string (comment it out)
4. Restart application
5. App automatically uses SQLite
6. Done! (simpleblog.db file reappears)
```

### From SQL Server → PostgreSQL

```
1. docker-compose --profile postgres up -d
2. docker-compose --profile sql-server stop  (optional - keep running both)
3. Edit appsettings.Development.json
4. Replace SQL Server connection string with PostgreSQL
5. Save and restart application
6. Done! (Data migrates automatically on first run)
```

## Connection String Cheat Sheet

### SQLite
```
No connection string needed
(Built-in provider, uses simpleblog.db file)
```

### SQL Server
```
Server=localhost,1433;User Id=sa;Password=Admin@12345;Database=SimpleBlogDb;TrustServerCertificate=True;
```

### PostgreSQL
```
Host=localhost;Port=5432;Username=simpleblog;Password=Admin@12345;Database=SimpleBlogDb;
```

## Docker Commands Cheat Sheet

```bash
# Start specific database
docker-compose --profile sql-server up -d     # SQL Server
docker-compose --profile postgres up -d       # PostgreSQL

# View status
docker-compose ps                             # Show all containers

# View logs
docker-compose logs -f sqlserver              # SQL Server logs
docker-compose logs -f postgres               # PostgreSQL logs

# Stop (keeps data)
docker-compose stop                           # Stop all
docker-compose stop sqlserver                 # Stop specific

# Restart (restart application)
docker-compose restart sqlserver              # Restart SQL Server
docker-compose restart postgres               # Restart PostgreSQL

# Cleanup (keeps data)
docker-compose down                           # Remove containers

# Full cleanup (deletes data)
docker-compose down -v                        # Remove containers + data
```

## Comparison Table

| Feature | SQLite | SQL Server | PostgreSQL |
|---------|--------|------------|------------|
| Setup Time | Immediate | 30s + 30s | 30s + 10s |
| Docker Required | No | Yes | Yes |
| Data Persistence | ✅ File-based | ✅ Volume-based | ✅ Volume-based |
| Production Ready | ⚠️ Development | ✅ Yes | ✅ Yes |
| Team Sharing | ⚠️ File sync | ✅ Container | ✅ Container |
| Resource Usage | Minimal | Medium | Low |
| Connection String Edit | Not needed | Required | Required |
| Best For | Solo dev | Production-like | Open source |

## Troubleshooting

### "Cannot connect to database"
```
Check:
  1. Is container running?     docker ps
  2. Is port available?        netstat -an | findstr 1433
  3. Connection string correct? appsettings.Development.json
  4. Container healthy?         docker-compose ps
     
Solution:
  • Wait 30 seconds for startup
  • Check logs: docker-compose logs -f
  • Restart: docker-compose restart sqlserver
```

### "Port already in use"
```
Edit docker-compose.yml:
  
  Before: ports:
            - "1433:1433"
  
  After:  ports:
            - "1434:1433"
```

### "Data lost after docker-compose down"
```
⚠️  Only use: docker-compose down -v  if you want to DELETE data
✅ Use:      docker-compose down      to keep data
```

## File Reference

- **docker-compose.yml** - Database service definitions
- **appsettings.Development.json** - Connection string location
- **DATABASE_SETUP.md** - Complete setup guide
- **DOCKER_QUICK_START.md** - 30-second setup
- **DOCKER_COMPOSE_SETUP.md** - Full Docker reference

## Next Steps

1. Pick your database (SQLite, SQL Server, or PostgreSQL)
2. Follow the steps above for your choice
3. Verify: `docker-compose ps` (if using Docker)
4. Run: `dotnet run --project SimpleBlog.AppHost`
5. Open: https://localhost:7030

Need help? Check [DOCKER_COMPOSE_SETUP.md](DOCKER_COMPOSE_SETUP.md) for detailed troubleshooting.
