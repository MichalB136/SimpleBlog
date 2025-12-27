# SimpleBlog Documentation

Welcome to SimpleBlog documentation! All guides are organized below.

## 📚 Quick Navigation

### Getting Started
- **[DATABASES.md](DATABASES.md)** - Main database index and navigation guide
- **[DOCKER_CHEATSHEET.md](DOCKER_CHEATSHEET.md)** - One-page quick reference card
- **[DOCKER_QUICK_START.md](DOCKER_QUICK_START.md)** - 30-second setup guide

### Database Configuration
- **[DATABASE_SETUP.md](DATABASE_SETUP.md)** - Complete database configuration guide
- **[DATABASE_SWITCH_GUIDE.md](DATABASE_SWITCH_GUIDE.md)** - How to switch between databases
- **[PERSISTENT_DB.md](PERSISTENT_DB.md)** - SQLite persistence implementation

### Docker Reference
- **[DOCKER_COMPOSE_SETUP.md](DOCKER_COMPOSE_SETUP.md)** - Full Docker Compose reference
- **[DOCKER_SETUP_SUMMARY.md](DOCKER_SETUP_SUMMARY.md)** - Docker setup overview

---

## 🎯 By Use Case

### I want to get started quickly
→ Read [DOCKER_QUICK_START.md](DOCKER_QUICK_START.md) (5 minutes)

### I need to understand database options
→ Read [DATABASES.md](DATABASES.md) (5 minutes)

### I want to switch databases
→ Read [DATABASE_SWITCH_GUIDE.md](DATABASE_SWITCH_GUIDE.md) (5 minutes)

### I need a quick reference
→ Read [DOCKER_CHEATSHEET.md](DOCKER_CHEATSHEET.md) (1 page)

### I need full Docker documentation
→ Read [DOCKER_COMPOSE_SETUP.md](DOCKER_COMPOSE_SETUP.md) (20 minutes)

---

## 📖 Documentation Structure

```
docs/
├── README.md                      ← You are here
├── DATABASES.md                   ← Main database guide
├── DOCKER_CHEATSHEET.md           ← Quick reference
├── DOCKER_QUICK_START.md          ← 30-second setup
├── DATABASE_SETUP.md              ← Complete configuration
├── DATABASE_SWITCH_GUIDE.md       ← Switch databases
├── DOCKER_COMPOSE_SETUP.md        ← Full Docker reference
├── DOCKER_SETUP_SUMMARY.md        ← Setup overview
└── PERSISTENT_DB.md               ← SQLite details
```

---

## 🚀 Quick Start

### Option 1: SQLite (Default - No Setup)
```bash
dotnet run --project SimpleBlog.AppHost
```

### Option 2: SQL Server (Docker)
```bash
docker-compose --profile sql-server up -d
# Update connection string in appsettings.Development.json
dotnet run --project SimpleBlog.AppHost
```

### Option 3: PostgreSQL (Docker)
```bash
docker-compose --profile postgres up -d
# Update connection string in appsettings.Development.json
dotnet run --project SimpleBlog.AppHost
```

---

## 🔗 External Links

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [SQL Server Docker Hub](https://hub.docker.com/_/microsoft-mssql-server)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)

---

**Last Updated:** December 27, 2025  
**Version:** 1.0
