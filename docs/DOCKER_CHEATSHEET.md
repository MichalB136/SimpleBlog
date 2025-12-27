# Docker Compose Quick Reference Card

## 📋 One-Page Cheat Sheet

### Start Database (Choose One)

```bash
# SQLite (No setup needed - already configured)
# → Just run the app below

# SQL Server (Docker)
docker-compose --profile sql-server up -d

# PostgreSQL (Docker)
docker-compose --profile postgres up -d
```

### Update Connection String (If Using Docker)

Edit: `SimpleBlog.ApiService/appsettings.Development.json`

**For SQL Server:**
```json
"Server=localhost,1433;User Id=sa;Password=Admin@12345;Database=SimpleBlogDb;TrustServerCertificate=True;"
```

**For PostgreSQL:**
```json
"Host=localhost;Port=5432;Username=simpleblog;Password=Admin@12345;Database=SimpleBlogDb;"
```

### Run Application

```bash
dotnet run --project SimpleBlog.AppHost
```

Then open: https://localhost:7030

---

## 🐳 Essential Commands

| Action | Command |
|--------|---------|
| **Start SQL Server** | `docker-compose --profile sql-server up -d` |
| **Start PostgreSQL** | `docker-compose --profile postgres up -d` |
| **Check status** | `docker-compose ps` |
| **View logs** | `docker-compose logs -f sqlserver` |
| **Restart** | `docker-compose restart sqlserver` |
| **Stop (keep data)** | `docker-compose stop` |
| **Remove (keep data)** | `docker-compose down` |
| **Remove all (delete data)** | `docker-compose down -v` |

---

## 📊 Database Comparison

| | **SQLite** | **SQL Server** | **PostgreSQL** |
|---|-----------|---|---|
| **Setup Time** | Instant | ~30s | ~30s |
| **Docker** | No | Yes | Yes |
| **Connection Edit** | No | Yes | Yes |
| **Port** | N/A | 1433 | 5432 |
| **Best For** | Solo dev | Production-like | Open source |

---

## 🔗 Connection Strings

**SQLite**
```
(No connection string - automatic)
```

**SQL Server**
```
Server=localhost,1433;User Id=sa;Password=Admin@12345;Database=SimpleBlogDb;TrustServerCertificate=True;
```

**PostgreSQL**
```
Host=localhost;Port=5432;Username=simpleblog;Password=Admin@12345;Database=SimpleBlogDb;
```

---

## ⚡ Common Tasks

### Check if Database is Ready
```bash
docker-compose ps
# Look for "healthy" status
```

### View What's Wrong
```bash
docker-compose logs -f sqlserver
```

### Port Already in Use?
Change `"1433:1433"` to `"1434:1433"` in docker-compose.yml

### Need to Delete Database?
```bash
docker-compose down -v
```
⚠️ This deletes all data!

---

## 📚 Documentation Guide

| File | Purpose |
|------|---------|
| **DATABASES.md** | Main index - start here |
| **DOCKER_QUICK_START.md** | 30-second setup |
| **DATABASE_SWITCH_GUIDE.md** | Switch between databases |
| **DOCKER_COMPOSE_SETUP.md** | Full Docker reference |
| **DATABASE_SETUP.md** | Complete guide |

---

## ✅ Validation

- ✓ Docker Compose syntax valid
- ✓ SQL Server container configured
- ✓ PostgreSQL container configured
- ✓ All documentation created
- ✓ Ready for production-like development

---

## 🎯 Next Steps

1. **Choose database:** SQLite (instant) or Docker (1 min)
2. **Update connection string:** (if using Docker)
3. **Run application:** `dotnet run --project SimpleBlog.AppHost`
4. **Open:** https://localhost:7030

---

## 🆘 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| Connection refused | Wait 30s, run `docker ps` |
| Port in use | Change port in docker-compose.yml |
| Container won't start | Run `docker-compose logs` to see error |
| Data disappeared | Use `docker-compose down` not `down -v` |

---

**Ready?** Start with [DATABASES.md](DATABASES.md) or [DOCKER_QUICK_START.md](DOCKER_QUICK_START.md)
