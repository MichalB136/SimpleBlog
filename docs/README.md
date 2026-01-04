# SimpleBlog Documentation

Welcome to SimpleBlog documentation! All guides are organized below.

## 📚 Documentation

### Getting Started
- **[DATABASES.md](DATABASES.md)** - PostgreSQL setup and configuration guide
- **[RENDER_DEPLOYMENT.md](RENDER_DEPLOYMENT.md)** - Production deployment on Render

### Main Documentation
- **[../README.md](../README.md)** - Project overview and quick start

---

## 🎯 Quick Guide

### Local Development

```bash
# Start the application (includes PostgreSQL)
dotnet run --project SimpleBlog.AppHost
```

Aspire automatically handles:
- PostgreSQL container
- Database creation
- Migrations
- Data seeding

### View Database

Open Aspire Dashboard (URL shown in console) to:
- See PostgreSQL connection string
- Monitor database logs
- View resource status

### Production Deployment

Follow [RENDER_DEPLOYMENT.md](RENDER_DEPLOYMENT.md) for:
- Blueprint deployment (recommended)
- Manual deployment steps
- Environment configuration
- Troubleshooting

---

## 📖 File Structure

```
docs/
├── README.md                      ← You are here
├── DATABASES.md                   ← PostgreSQL guide
└── RENDER_DEPLOYMENT.md           ← Production deployment
```

---

## 🔧 Technology Stack

- **.NET 9.0** with Aspire 13.1.0
- **PostgreSQL** - Database (local & production)
- **Entity Framework Core 9.0.10**
- **Npgsql 9.0.4** - PostgreSQL provider
- **Docker** - Managed by Aspire

---

## 🚀 Common Tasks

### Create Migration

```bash
dotnet ef migrations add MigrationName --project SimpleBlog.ApiService
```

### Apply Migrations

```bash
dotnet ef database update --project SimpleBlog.ApiService
```

### Reset Database

```bash
# Find volume name in Aspire Dashboard
docker volume rm <postgres-volume-name>

# Restart application - database will be recreated
dotnet run --project SimpleBlog.AppHost
```

### Deploy to Render

1. Push code to GitHub/GitLab
2. Create Blueprint in Render Dashboard
3. Connect repository
4. Deploy automatically

See [RENDER_DEPLOYMENT.md](RENDER_DEPLOYMENT.md) for details.

---

## 📝 Notes

- **SQL Server removed** - Project now uses PostgreSQL exclusively
- **Docker Compose removed** - Aspire handles container orchestration
- All database-related files moved to [DATABASES.md](DATABASES.md)
- Production and local development use the same database engine

---

## 🔗 External Links

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)

---

**Last Updated:** January 4, 2026  
**Version:** 1.0
