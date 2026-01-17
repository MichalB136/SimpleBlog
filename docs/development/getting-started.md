# Getting Started

> ## Document Metadata
> 
> ### ✅ Required
> **Title:** Getting Started  
> **Description:** Comprehensive guide to first-time SimpleBlog launch - from installation to first code change  
> **Audience:** developer  
> **Topic:** development  
> **Last Update:** 2026-01-17
>
> ### 📌 Recommended
> **Parent Document:** [README.md](./README.md)  
> **Difficulty:** beginner  
> **Estimated Time:** 15 min  
> **Version:** 1.0.0  
> **Status:** approved
>
> ### 🏷️ Optional
> **Prerequisites:** Basic knowledge of .NET, Docker, Git  
> **Tags:** `setup`, `quickstart`, `beginner`, `first-run`

---

## 📋 Overview

This document will help you set up the development environment and start working on SimpleBlog in less than 15 minutes.

---

## ✅ Prerequisites

- [ ] [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed
- [ ] [Docker Desktop](https://www.docker.com/products/docker-desktop/) running
- [ ] [Git](https://git-scm.com/) installed
- [ ] [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

**Check Installation:**
```powershell
# Check .NET
dotnet --version
# Should display: 9.0.x

# Check Docker
docker --version
# Should display: Docker version 24.x.x

# Check Git
git --version
# Should display: git version 2.x.x
```

---

## 🚀 Quick Start

### Step 1: Clone the Repository

```powershell
# Clone the project
git clone https://github.com/MichalB136/SimpleBlog.git

# Navigate to directory
cd SimpleBlog
```

### Step 2: Install .NET Aspire Workload

```powershell
# Update workloads
dotnet workload update

# Install Aspire
dotnet workload install aspire
```

**Expected Result:**
```
Successfully installed workload(s) aspire.
```

### Step 3: Run PostgreSQL Database

```powershell
# Start PostgreSQL in background
docker-compose up -d

# Check status
docker-compose ps
```

**Expected Result:**
```
NAME                    STATUS
simpleblog-postgres-1   Up About a minute
simpleblog-pgadmin-1    Up About a minute
```

### Step 4: Run the Application

```powershell
# Option 1: Use script
.\start.ps1

# Option 2: Manually
dotnet run --project SimpleBlog.AppHost
```

**What Happens:**
- ✅ Application compiles
- ✅ Aspire starts all services
- ✅ Database migrations are automatically applied
- ✅ Test data is seeded
- ✅ Aspire Dashboard opens in browser

### Step 5: Access the Application

After starting, you'll see in the console:

```
Aspire Dashboard: http://localhost:15275
```

In Aspire Dashboard you'll find:
- **webfrontend** - Web application (click to open)
- **apiservice** - REST API
- **postgres** - PostgreSQL database

---

## 🔧 Environment Verification

### Check if Everything Works

```powershell
# Test API health endpoint
curl http://localhost:5000/health

# Expected result: Healthy
```

### Check Database

1. Open Aspire Dashboard
2. Click on **postgres** in Resources section
3. See connection string and status
4. Alternatively use pgAdmin: http://localhost:5050
   - Email: `admin@simpleblog.com`
   - Password: `admin`

---

## 💻 First Changes

### Edit Backend

```powershell
# Open in Visual Studio
start SimpleBlog.sln

# Or in VS Code
code .
```

**Example:** Add a new endpoint in `SimpleBlog.ApiService/Endpoints/PostEndpoints.cs`

```csharp
// Good: Add a new endpoint
group.MapGet("/featured", async (IPostRepository repo) =>
{
    var posts = await repo.GetFeaturedPostsAsync();
    return Results.Ok(posts);
})
.WithName("GetFeaturedPosts")
.WithOpenApi();
```

### Edit Frontend

```powershell
# Navigate to frontend directory
cd SimpleBlog.Web/client

# Install dependencies (first time)
npm install

# Run dev server with hot reload
npm run dev
```

**Example:** Edit component in `SimpleBlog.Web/client/src/components/PostList.tsx`

---

## ⚠️ Common Issues

### Issue: "Docker is not running"

**Solution:**
```powershell
# Start Docker Desktop
# Wait for Docker Engine to start (green icon in tray)

# Check again
docker ps
```

### Issue: "Port 5432 already in use"

**Cause:** Another PostgreSQL already running on port 5432

**Solution:**
```powershell
# Option 1: Stop local PostgreSQL
# Windows Services > PostgreSQL > Stop

# Option 2: Change port in docker-compose.yml
# Edit: "5433:5432" instead of "5432:5432"
```

### Issue: "Aspire workload not found"

**Solution:**
```powershell
# Update .NET SDK to latest version
winget upgrade Microsoft.DotNet.SDK.9

# Reinstall workload
dotnet workload install aspire --skip-sign-check
```

---

## 📚 Next Steps

After successful launch:

1. 📖 Read [project-structure.md](./project-structure.md) - understand code organization
2. 📝 Familiarize yourself with [coding-standards.md](./coding-standards.md) - coding standards
3. 🧪 See [testing.md](./testing.md) - how to write tests
4. 🔀 Check [git-workflow.md](./git-workflow.md) - project workflow

---

## 🔗 Useful Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [EF Core Documentation](https://learn.microsoft.com/ef/core/)
- [React Documentation](https://react.dev/)
- [PostgreSQL Tutorial](https://www.postgresql.org/docs/current/tutorial.html)

---

## 💡 Tips

> **💡 Tip:** Use `.\start.ps1 -Clean` to do a clean build before starting

> **💡 Tip:** Aspire Dashboard automatically refreshes logs - no need to restart

> **💡 Tip:** C# code changes require restart, but frontend has hot reload

> **✅ Note:** First run may take longer due to Docker image downloads
