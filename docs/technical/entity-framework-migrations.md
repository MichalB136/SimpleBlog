# Entity Framework Core Migrations Guide

> ## Document Metadata
> 
> ### ✅ Required
> **Title:** Entity Framework Core Migrations Guide  
> **Description:** Technical guide for managing database migrations in SimpleBlog using EF Core with PostgreSQL. Covers creation, application, rollback, and troubleshooting.  
> **Audience:** developer, devops  
> **Topic:** technical  
> **Last Update:** 2026-01-18
>
> ### 📌 Recommended
> **Parent Document:** [architecture-overview.md](./architecture-overview.md)  
> **Difficulty:** intermediate  
> **Estimated Time:** 20 min  
> **Version:** 1.0.0  
> **Status:** approved
>
> ### 🏷️ Optional
> **Prerequisites:** .NET 9, PostgreSQL, EF Core knowledge  
> **Related Docs:** [../development/database-guide.md](../development/database-guide.md)  
> **Tags:** `migrations`, `ef-core`, `database`, `postgresql`

---

## 📋 Overview

Entity Framework Core (EF Core) manages SimpleBlog's database schema changes through migrations. This guide explains how to create, apply, and troubleshoot migrations in the SimpleBlog project which uses multiple DbContexts (ApplicationDbContext, BlogDbContext, ShopDbContext).

---

## 🎯 Document Purpose

This document helps developers:
- Create new migrations when schema changes
- Apply migrations to development, staging, and production databases
- Troubleshoot common migration issues
- Rollback failed migrations safely
- Understand the migration structure specific to SimpleBlog

---

## ✅ Prerequisites

- [x] .NET 9 SDK installed - [Download](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [x] PostgreSQL 16+ running locally - [Installation Guide](https://www.postgresql.org/download/)
- [x] SimpleBlog repository cloned - [GitHub](https://github.com/MichalB136/SimpleBlog)
- [x] Entity Framework CLI tools: `dotnet ef`
- [x] Connection string configured in `appsettings.json`

**Verify Prerequisites:**
```bash
# Check .NET version
dotnet --version

# Check EF Core CLI
dotnet ef --version

# Verify PostgreSQL is running
psql --version
```

---

## 🚀 Quick Start: Creating and Applying Migrations

### Step 1: Make Schema Changes

Edit entity models in the appropriate service project:
- **Blog content:** `SimpleBlog.Blog.Services/Entities.cs`
- **Shop content:** `SimpleBlog.Shop.Services/Entities.cs`
- **Authentication:** `SimpleBlog.ApiService/Identity/Entities.cs`

Example - Adding a field to Post:
```csharp
public class PostEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Category { get; set; } = null!;  // ← NEW FIELD
    public DateTimeOffset CreatedAt { get; set; }
}
```

### Step 2: Build the Project

```bash
cd c:\Code\SimpleBlog
dotnet build SimpleBlog.sln
```

**Expected Result:**
```
Build succeeded.
```

### Step 3: Create Migration

Use `dotnet ef migrations add` with the correct context:

```bash
# For BlogDbContext migrations
dotnet ef migrations add AddCategoryToBlog \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService \
  --output-dir Data/Migrations/Blog

# For ShopDbContext migrations
dotnet ef migrations add AddCategoryToShop \
  --context ShopDbContext \
  --project SimpleBlog.Shop.Services \
  --startup-project SimpleBlog.ApiService \
  --output-dir Data/Migrations/Shop

# For ApplicationDbContext (identity)
dotnet ef migrations add AddCustomField \
  --context ApplicationDbContext \
  --project SimpleBlog.ApiService \
  --output-dir Data/Migrations
```

**Expected Result:**
```
Done. To undo this action, use 'ef migrations remove'
```

**What was created:**
```
SimpleBlog.Blog.Services/
└── Data/Migrations/Blog/
    ├── 20260118_AddCategoryToBlog.cs          ← Main migration
    └── 20260118_AddCategoryToBlog.Designer.cs ← Metadata
```

### Step 4: Review Migration Code

Open the generated migration file and **verify it's correct**:

```csharp
public partial class AddCategoryToBlog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ✅ Check: Does this add/modify/delete the intended column?
        migrationBuilder.AddColumn<string>(
            name: "Category",
            table: "Posts",
            type: "text",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // ✅ Check: Does this correctly rollback?
        migrationBuilder.DropColumn(
            name: "Category",
            table: "Posts");
    }
}
```

### Step 5: Apply Migration

Run migrations **before launching the application**:

```bash
# Apply BlogDbContext migrations
dotnet ef database update \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService

# Apply ShopDbContext migrations
dotnet ef database update \
  --context ShopDbContext \
  --project SimpleBlog.Shop.Services \
  --startup-project SimpleBlog.ApiService

# Apply ApplicationDbContext migrations
dotnet ef database update \
  --context ApplicationDbContext \
  --project SimpleBlog.ApiService
```

**Expected Result:**
```
Applying migration '20260118_AddCategoryToBlog'
Done.
```

### Step 6: Launch Application

```bash
dotnet run --project SimpleBlog.AppHost
```

The migrations will also apply automatically on startup through the `MigrateDatabaseAsync()` method in `Program.cs`.

---

## ⚙️ Migration Configuration

### DbContext Structure

SimpleBlog uses **three separate DbContexts**:

```
SimpleBlog.sln
├── SimpleBlog.ApiService/
│   ├── Identity/Entities.cs
│   └── Program.cs (ApplicationDbContext)
│
├── SimpleBlog.Blog.Services/
│   ├── Entities.cs (Post, Comment, AboutMe, SiteSettings)
│   ├── BlogDbContext.cs
│   └── Data/Migrations/Blog/
│
└── SimpleBlog.Shop.Services/
    ├── Entities.cs (Product, Order)
    ├── ShopDbContext.cs
    └── Data/Migrations/Shop/
```

### Migration Folders

Each context has its own migration folder:

| Context | Folder | Purpose |
|---------|--------|---------|
| ApplicationDbContext | `SimpleBlog.ApiService/Data/Migrations` | User authentication, identity |
| BlogDbContext | `SimpleBlog.Blog.Services/Data/Migrations/Blog` | Posts, comments, blog settings |
| ShopDbContext | `SimpleBlog.Shop.Services/Data/Migrations/Shop` | Products, orders, shop data |

### Connection String Configuration

**Development** (`appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=simpleblog;Username=simpleblog_user;Password=simpleblog_password"
  }
}
```

**All three contexts share the same database**, but migrations are applied per-context.

---

## 🔧 Common Migration Tasks

### Undo Last Migration

If you created a migration but haven't applied it yet:

```bash
# Remove last migration (unapplied only)
dotnet ef migrations remove \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService
```

### Rollback to Previous Migration

If applied migration has issues:

```bash
# Rollback to specific migration
dotnet ef database update 20260101_InitialCreate \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService

# Verify rolled back
dotnet ef migrations list \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService
```

### List All Migrations

```bash
dotnet ef migrations list \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService
```

**Expected Output:**
```
Build succeeded.
20250101_InitialCreate
20250110_AddImageUrls
20250118_AddTagsSupport
```

### Generate SQL Script

Review SQL before applying:

```bash
dotnet ef migrations script \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService \
  --output migrations.sql
```

---

## 🐛 Troubleshooting

### Error: "The name 'DbSet' does not exist"

**Problem:** Missing DbSet in DbContext

**Solution:**
```csharp
// SimpleBlog.Blog.Services/BlogDbContext.cs
public DbSet<TagEntity> Tags { get; set; } = null!;
public DbSet<PostTagEntity> PostTags { get; set; } = null!;
```

### Error: "Your startup project 'X' doesn't reference Microsoft.EntityFrameworkCore.Design"

**Problem:** Design tools not available in startup project

**Solution:**
```bash
# ALWAYS use SimpleBlog.ApiService as startup project
dotnet ef migrations add MyMigration \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService  # ← KEY
```

### Error: "Could not find DbContext with name 'BlogDbContext'"

**Problem:** Wrong context name

**Solution:** Verify context name in OnConfiguring method:
```csharp
public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) 
        : base(options) { }  // ← Must match --context parameter
}
```

### Error: "Cannot drop column because it's part of an index"

**Problem:** Column has constraints

**Solution:** Manually edit migration to drop index first:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropIndex(name: "idx_Posts_Category", table: "Posts");
    migrationBuilder.DropColumn(name: "Category", table: "Posts");
}
```

### Error: "The database already exists"

**Problem:** Database state mismatch

**Solution:**
```bash
# Check current schema
dotnet ef migrations script \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService

# Force update
dotnet ef database update \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService
```

### Tests Failing After Migration

**Problem:** Test data incompatible with schema

**Solution:** Update test fixtures in `SimpleBlog.Tests/`:
```csharp
// Update entity construction with new required fields
var post = new Post(
    Guid.NewGuid(),
    "Title",
    "Content",
    "Author",
    DateTimeOffset.UtcNow,
    new List<Comment>(),
    new List<string>(),
    false,
    new List<Tag>()  // ← NEW REQUIRED FIELD
);
```

---

## 📊 Migration Best Practices

### ✅ DO

- **Create a new migration** for each logical schema change
- **Test migrations locally** before pushing to repository
- **Review generated migration code** before applying to production
- **Use descriptive migration names:** `AddTagsSupport`, not `Change1`
- **Keep migrations atomic:** One concept per migration
- **Document breaking changes** in migration comments

```csharp
public partial class AddCategoryField : Migration
{
    /// <summary>
    /// Adds Category column to Posts table.
    /// WARNING: Existing posts will have empty category (default: "Uncategorized")
    /// </summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Category",
            table: "Posts",
            nullable: false,
            defaultValue: "Uncategorized");
    }
}
```

### ❌ DON'T

- Don't modify migrations after they're committed (create a new one instead)
- Don't delete migration files manually (use `ef migrations remove`)
- Don't share one migration file across multiple DbContexts
- Don't apply migrations without testing
- Don't ignore migration failures

---

## 🚀 Deployment: Production Migrations

### Pre-Deployment Checklist

- [ ] All migrations created and tested locally
- [ ] Tests passing (120/120 tests)
- [ ] SQL scripts reviewed for breaking changes
- [ ] Database backup created
- [ ] Rollback plan documented

### Deployment Steps

**1. Generate Migration Script**
```bash
dotnet ef migrations script \
  --context BlogDbContext \
  --project SimpleBlog.Blog.Services \
  --startup-project SimpleBlog.ApiService \
  --output blog-migrations.sql
```

**2. Review SQL Script**
```sql
-- blog-migrations.sql
-- Check for:
-- - Data loss operations (DROP COLUMN)
-- - Performance impacts (new indexes)
-- - Constraint conflicts
```

**3. Create Database Backup** (Ask DevOps)
```bash
# PostgreSQL backup
pg_dump simpleblog > simpleblog_backup.sql
```

**4. Apply Migrations**
```bash
# Option A: Automatic (on app startup)
dotnet run --project SimpleBlog.ApiService

# Option B: Manual
dotnet ef database update --context BlogDbContext --project SimpleBlog.Blog.Services --startup-project SimpleBlog.ApiService
```

**5. Verify Application**
```bash
# Check API health
curl https://api.simpleblog.com/health

# Run smoke tests
dotnet test SimpleBlog.Tests
```

### Rollback Procedure

If migration causes issues:

```bash
# 1. Identify last good migration
dotnet ef migrations list --context BlogDbContext --project SimpleBlog.Blog.Services --startup-project SimpleBlog.ApiService

# 2. Rollback to specific migration
dotnet ef database update 20250110_AddImageUrls --context BlogDbContext --project SimpleBlog.Blog.Services --startup-project SimpleBlog.ApiService

# 3. Restore from backup if needed
psql simpleblog < simpleblog_backup.sql

# 4. Verify application
dotnet test SimpleBlog.Tests
```

---

## 📚 Related Documentation

- [Architecture Overview](./architecture-overview.md) - Database design
- [Database Guide](../development/database-guide.md) - Development database setup
- [Deployment Guide](../deployment/production-deployment.md) - Production deployment
- [EF Core Official Docs](https://learn.microsoft.com/en-us/ef/core/) - Microsoft documentation

---

## ✅ Checklist: Creating a New Migration

Use this checklist when adding a new migration:

- [ ] Modified entity model in appropriate `Entities.cs`
- [ ] Built solution successfully (`dotnet build`)
- [ ] Created migration with descriptive name
- [ ] Reviewed generated migration SQL
- [ ] Added null checks or default values where needed
- [ ] Updated test fixtures if schema changed
- [ ] Ran all tests locally (120/120 passing)
- [ ] Committed both migration files (main + Designer)
- [ ] Pushed to feature branch for code review
- [ ] Documented breaking changes if any
