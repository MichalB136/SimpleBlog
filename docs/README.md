# SimpleBlog Documentation

> ## Document Metadata
> 
> ### ✅ Required
> **Title:** SimpleBlog Documentation - Main Index  
> **Description:** Central entry point to all SimpleBlog project documentation divided into development, deployment and technical sections  
> **Audience:** all  
> **Topic:** documentation  
> **Last Update:** 2026-01-17
>
> ### 📌 Recommended
> **Difficulty:** beginner  
> **Estimated Time:** 5 min  
> **Version:** 1.0.0  
> **Status:** approved

---

## 📋 Overview

Comprehensive documentation for the SimpleBlog project divided into three main categories following standards: consistency, quality, maintainability, accessibility and visual communication.

---

## 📐 Documentation Standards

All documents in this project adhere to the official [documentation standards](./documentation-standards.md).

**Key Principles:**
- ✅ **Consistency** - Consistent structure and terminology
- ✅ **Quality** - Tested examples, up-to-date content
- ✅ **Maintainability** - Easy to update, modular
- ✅ **Accessibility** - Accessible for all levels
- ✅ **Visual Communication** - Diagrams, formatting, emoji

[➡️ View Full Standards](./documentation-standards.md)

## 📚 Main Documentation Sections

### 💻 [development](./development/README.md)
**For:** Developers, Contributors  
**Content:** 
- Local environment setup
- Coding standards
- Git workflow and testing
- Database and migrations work
- Debug and troubleshooting

[➡️ Go to Development Docs](./development/README.md)

---

### 🚀 [deployment](./deployment/README.md)
**For:** DevOps, System Administrators  
**Content:**
- Deployment to various platforms (Render, Azure, Docker)
- Production environment configuration
- CI/CD pipelines
- Monitoring and maintenance
- Security checklist

[➡️ Go to Deployment Docs](./deployment/README.md)

---

### 🏗️ [technical](./technical/README.md)
**For:** Architects, Senior Developers  
**Content:**
- System architecture
- Design patterns
- Database schema and relationships
- API specification
- Architecture Decision Records (ADR)

[➡️ Go to Technical Docs](./technical/README.md)

---

## 🎯 Quick Start Guide

### Local Development

For detailed setup instructions, see [Getting Started Guide](./development/getting-started.md).

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

Follow [Render Deployment Guide](./deployment/render-guide.md) for:
- Blueprint deployment (recommended)
- Manual deployment steps
- Environment configuration
- Troubleshooting

---

## 📂 Documentation Structure

```
docs/
├── README.md                           ← You are here
├── documentation-template.md           ← Template for new docs
├── documentation-standards.md          ← Standards guide
│
├── development/                        ← Development docs
│   ├── README.md
│   ├── getting-started.md
│   ├── project-structure.md
│   ├── database-guide.md               ← PostgreSQL guide
│   └── git-workflow.md                 ← Git workflow
│
├── deployment/                         ← Deployment docs
│   ├── README.md
│   └── render-guide.md                 ← Production deployment
│
└── technical/                          ← Technical docs
    ├── README.md
    └── architecture-overview.md
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

### Database Operations

See [Database Guide](./development/database-guide.md) for detailed instructions.

#### Create Migration

```bash
dotnet ef migrations add MigrationName --project SimpleBlog.ApiService
```

#### Apply Migrations

```bash
dotnet ef database update --project SimpleBlog.ApiService
```

#### Reset Database

```bash
# Find volume name in Aspire Dashboard
docker volume rm <postgres-volume-name>

# Restart application - database will be recreated
dotnet run --project SimpleBlog.AppHost
```

### Production Deployment

See [Render Deployment Guide](./deployment/render-guide.md) for complete instructions.

1. Push code to GitHub/GitLab
2. Create Blueprint in Render Dashboard
3. Connect repository
4. Deploy automatically

---

## 📝 Project Notes

- **PostgreSQL Only** - Project uses PostgreSQL exclusively (SQL Server removed)
- **Aspire Orchestration** - Aspire handles container orchestration (Docker Compose removed)
- **Consistent Database** - Production and local development use the same database engine
- **Multi-Context Design** - Three separate DbContext classes (ApplicationDbContext, BlogDbContext, ShopDbContext)

---

## 🔗 External Links

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)

---

**Last Updated:** January 4, 2026  
**Version:** 1.0
