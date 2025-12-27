# SimpleBlog - Complete Database Setup Documentation

Welcome! This guide helps you set up and manage databases for SimpleBlog development.

## 📚 Quick Navigation

### For Different Users

**I'm a beginner - Get me started in 30 seconds:**
→ Read [DOCKER_QUICK_START.md](DOCKER_QUICK_START.md)

**I need to understand database options:**
→ Read [DATABASE_SETUP.md](DATABASE_SETUP.md)

**I want to switch between databases:**
→ Read [DATABASE_SWITCH_GUIDE.md](DATABASE_SWITCH_GUIDE.md)

**I need detailed Docker documentation:**
→ Read [DOCKER_COMPOSE_SETUP.md](DOCKER_COMPOSE_SETUP.md)

**I want to keep my local SQLite database persistent:**
→ Read [PERSISTENT_DB.md](PERSISTENT_DB.md)

---

## 🗂️ File Reference

### Configuration Files
| File | Purpose | Edit? |
|------|---------|-------|
| [docker-compose.yml](docker-compose.yml) | Database service definitions | ⚠️ Advanced users only |
| [.env.example](.env.example) | Environment variables template | ℹ️ Reference only |
| [docker/mssql/init/01-init.sql](docker/mssql/init/01-init.sql) | SQL Server initialization | ℹ️ Reference only |

### Documentation
| File | Best For | Read Time |
|------|----------|-----------|
| [DOCKER_QUICK_START.md](DOCKER_QUICK_START.md) | Fast setup | 5 min |
| [DATABASE_SETUP.md](DATABASE_SETUP.md) | Understanding options | 10 min |
| [DATABASE_SWITCH_GUIDE.md](DATABASE_SWITCH_GUIDE.md) | Switching databases | 5 min |
| [DOCKER_COMPOSE_SETUP.md](DOCKER_COMPOSE_SETUP.md) | Complete reference | 20 min |
| [PERSISTENT_DB.md](PERSISTENT_DB.md) | SQLite persistence | 5 min |

---

## 🚀 Quick Start (30 seconds)

### Option 1: Use SQLite (Default - No Setup Needed)
```bash
# It's already configured! Just run:
dotnet run --project SimpleBlog.AppHost
```
✅ Data persists in `simpleblog.db` file

### Option 2: Use SQL Server (Docker)
```bash
# 1. Start database
docker-compose --profile sql-server up -d

# 2. Wait for startup
docker ps  # Check for "healthy"

# 3. Update connection string in appsettings.Development.json
# (See DATABASE_SWITCH_GUIDE.md for exact connection string)

# 4. Run application
dotnet run --project SimpleBlog.AppHost
```

### Option 3: Use PostgreSQL (Docker)
```bash
# 1. Start database
docker-compose --profile postgres up -d

# 2. Update connection string in appsettings.Development.json
# (See DATABASE_SWITCH_GUIDE.md for exact connection string)

# 3. Run application
dotnet run --project SimpleBlog.AppHost
```

---

## 📊 Database Comparison

| Aspect | SQLite | SQL Server | PostgreSQL |
|--------|--------|------------|------------|
| **Setup Time** | Immediate | 30s + 30s | 30s + 10s |
| **Docker Required** | ❌ No | ✅ Yes | ✅ Yes |
| **Best For** | Solo development | Production-like | Open source |
| **Configuration** | Already done | 2 minutes | 2 minutes |
| **Data Persistence** | ✅ Automatic | ✅ In volumes | ✅ In volumes |

---

## 🐳 Docker Commands Cheat Sheet

```bash
# Start services
docker-compose --profile sql-server up -d        # SQL Server
docker-compose --profile postgres up -d          # PostgreSQL
docker-compose ps                                 # View status

# Manage services
docker-compose logs -f sqlserver                 # View logs
docker-compose restart sqlserver                 # Restart
docker-compose stop                              # Stop (keep data)
docker-compose down                              # Remove (keep data)
docker-compose down -v                           # Remove (delete data)
```

---

## 📝 Configuration Checklist

### Using SQLite (Default)
- ✅ No setup required
- ✅ Uses `simpleblog.db` file
- ✅ Data persists automatically

### Using SQL Server
- [ ] Run: `docker-compose --profile sql-server up -d`
- [ ] Wait for "healthy" status: `docker ps`
- [ ] Update connection string in `appsettings.Development.json`
- [ ] Run: `dotnet run --project SimpleBlog.AppHost`

### Using PostgreSQL
- [ ] Run: `docker-compose --profile postgres up -d`
- [ ] Update connection string in `appsettings.Development.json`
- [ ] Run: `dotnet run --project SimpleBlog.AppHost`

---

## ❓ Common Questions

**Q: Which database should I use?**
A: SQLite for quick development, SQL Server for production-like testing, PostgreSQL for lightweight open source.

**Q: Do I need Docker?**
A: No - SQLite works without Docker. Docker is optional for SQL Server/PostgreSQL.

**Q: How do I switch databases?**
A: Update the connection string in `appsettings.Development.json` and restart the app. See [DATABASE_SWITCH_GUIDE.md](DATABASE_SWITCH_GUIDE.md).

**Q: Will my data persist?**
A: Yes - all three databases persist data. SQLite uses `simpleblog.db` file, SQL Server/PostgreSQL use Docker volumes.

**Q: Can I run multiple databases at once?**
A: Yes - `docker-compose --profile sql-server --profile postgres up -d` runs both. Application uses whichever connection string is configured.

**Q: How do I access the database directly?**
A: SQLite: File explorer or SQLite client. SQL Server: SSMS/Azure Data Studio. PostgreSQL: pgAdmin/DBeaver.
See [DATABASE_SETUP.md](DATABASE_SETUP.md) for connection details.

**Q: Is this production-ready?**
A: Docker Compose is for development only. For production, use managed services (Azure SQL, AWS RDS, etc.).

---

## 🔧 Troubleshooting

| Problem | Solution |
|---------|----------|
| Connection refused | Wait 30s, check `docker ps`, verify connection string |
| Port already in use | Change port in docker-compose.yml or kill process |
| Container exits | Check logs: `docker-compose logs sqlserver` |
| Data disappeared | Use `down` not `down -v` to keep data |
| Can't connect after switching | Verify container is running: `docker ps` |

For detailed troubleshooting, see [DATABASE_SETUP.md](DATABASE_SETUP.md) or [DOCKER_COMPOSE_SETUP.md](DOCKER_COMPOSE_SETUP.md).

---

## 📂 Project Structure

```
SimpleBlog/
├── docker-compose.yml              ← Service definitions
├── .env.example                    ← Environment template
├── DATABASE_SETUP.md               ← Complete guide
├── DATABASE_SWITCH_GUIDE.md        ← Switching databases
├── DOCKER_QUICK_START.md           ← 30-second setup
├── DOCKER_COMPOSE_SETUP.md         ← Full reference
├── PERSISTENT_DB.md                ← SQLite details
└── docker/
    └── mssql/
        └── init/
            └── 01-init.sql         ← SQL Server init
```

---

## 🎯 Next Steps

1. **Choose your database:**
   - SQLite (default) - Nothing to do
   - SQL Server - Follow [DOCKER_QUICK_START.md](DOCKER_QUICK_START.md)
   - PostgreSQL - Follow [DOCKER_QUICK_START.md](DOCKER_QUICK_START.md)

2. **Configure (if not SQLite):**
   - Update connection string in `appsettings.Development.json`
   - See exact string in [DATABASE_SWITCH_GUIDE.md](DATABASE_SWITCH_GUIDE.md)

3. **Run application:**
   ```bash
   dotnet run --project SimpleBlog.AppHost
   ```

4. **Verify:**
   - Open https://localhost:7030
   - Check database connection in application logs

---

## 📖 Documentation Map

```
START HERE
    ↓
Choose database
    ↓
  SQLite        SQL Server       PostgreSQL
    ↓              ↓                 ↓
  Ready!    DOCKER_QUICK_START   DOCKER_QUICK_START
            or                   or
            DATABASE_SETUP       DATABASE_SETUP
                ↓                    ↓
            Same → DATABASE_SWITCH_GUIDE
```

---

## 🆘 Need Help?

1. **Quick answer:** [DATABASE_SWITCH_GUIDE.md](DATABASE_SWITCH_GUIDE.md) has common scenarios
2. **Setup issue:** [DOCKER_QUICK_START.md](DOCKER_QUICK_START.md) or [DATABASE_SETUP.md](DATABASE_SETUP.md)
3. **Docker details:** [DOCKER_COMPOSE_SETUP.md](DOCKER_COMPOSE_SETUP.md)
4. **Troubleshooting:** All guides have troubleshooting sections

---

## ✨ Summary

SimpleBlog supports **3 database backends** for flexibility:
- **SQLite** - Already configured, persistent, zero setup
- **SQL Server** - Production-like, Docker container, 1 minute setup
- **PostgreSQL** - Lightweight, Docker container, 1 minute setup

All persist data automatically. Switch anytime by updating the connection string.

**Ready?** Start with [DOCKER_QUICK_START.md](DOCKER_QUICK_START.md) or just run the app!

```bash
dotnet run --project SimpleBlog.AppHost
```

---

**Last Updated:** December 27, 2025  
**Status:** ✅ Complete and tested
