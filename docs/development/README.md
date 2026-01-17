# Development Documentation

> **For:** Developers, Contributors  
> **Purpose:** Guide for developers working on SimpleBlog

---

## 📚 Available Documentation

### ✅ Completed Guides

- **[getting-started.md](./getting-started.md)** - First steps with the project (15 min setup)
- **[project-structure.md](./project-structure.md)** - Project structure and code organization
- **[database-guide.md](./database-guide.md)** - PostgreSQL setup, migrations, and troubleshooting
- **[git-workflow.md](./git-workflow.md)** - GitFlow strategy, branching, and commit conventions
- **[react-router-guide.md](./react-router-guide.md)** - React Router implementation and SPA routing
- **[admin-features.md](./admin-features.md)** - Admin panel features (themes, logo management)

### 🚧 Planned Documentation

- **coding-standards.md** - C# and JavaScript coding standards
- **testing.md** - Writing and running tests
- **aspire-development.md** - Working with .NET Aspire
- **api-endpoints.md** - Creating API endpoints
- **debugging-guide.md** - Application debugging
- **authentication.md** - JWT authentication implementation

---

## 🚀 Quick Start for New Developers

```powershell
# 1. Clone the repository
git clone https://github.com/MichalB136/SimpleBlog.git
cd SimpleBlog

# 2. Start the database
docker-compose up -d

# 3. Run the application
dotnet run --project SimpleBlog.AppHost

# 4. Open the URL from Aspire Dashboard in your browser
```

More details in [getting-started.md](./getting-started.md)

---

## 🛠️ Development Tools

| Tool | Version | Required | Description |
|------|---------|----------|-------------|
| .NET SDK | 9.0+ | ✅ Yes | Runtime and compiler |
| Docker Desktop | Latest | ✅ Yes | Containers (PostgreSQL) |
| Visual Studio 2022 | 17.9+ | ⚪ Recommended | IDE with Aspire support |
| VS Code | Latest | ⚪ Alternative | Lightweight IDE |
| Git | 2.40+ | ✅ Yes | Version control |

---

## 📖 Useful Links

- [Technical documentation](../technical/README.md)
- [Deployment guide](../deployment/README.md)
- [.NET Aspire docs](https://learn.microsoft.com/dotnet/aspire/)
- [EF Core docs](https://learn.microsoft.com/ef/core/)

---

## 🤝 Contributing

Before starting work:
1. Read [coding-standards.md](./coding-standards.md)
2. Familiarize yourself with [git-workflow.md](./git-workflow.md)
3. Create a branch according to convention: `feature/name` or `fix/name`
4. Add tests for new functionality
5. Ensure all tests pass
