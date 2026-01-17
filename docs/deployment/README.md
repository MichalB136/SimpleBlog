# Deployment Documentation

> **For:** DevOps, System Administrators  
> **Purpose:** Deploying SimpleBlog to various platforms

---

## 📚 Available Documentation

### ✅ Completed Guides

- **[render-guide.md](./render-guide.md)** - Complete Render.com deployment guide (Blueprint & Manual)

### 🚧 Planned Documentation

- **deployment-overview.md** - Deployment options overview
- **azure-deployment.md** - Deployment to Azure
- **docker-deployment.md** - Deployment via Docker Compose
- **postgresql-production.md** - PostgreSQL configuration in production
- **environment-variables.md** - Environment variables
- **health-checks.md** - Health checks and liveness probes
- **github-actions.md** - Automated deployments via GitHub Actions
- **security-checklist.md** - Security checklist

---

## 🚀 Quick Deploy - Render.com

Complete guide: [render-guide.md](./render-guide.md)

```bash
# 1. Prepare repository
git push origin main

# 2. In Render Dashboard
# - New > Blueprint
# - Connect repository
# - Deploy from render.yaml

# 3. Configure environment variables
# - JWT_KEY (min 32 characters)
# - ASPNETCORE_ENVIRONMENT=Production

# 4. Automatic deploy
```

---

## 🎯 Platform Comparison

| Platform | Complexity | Cost | Recommendation |
|----------|------------|------|----------------|
| **Render** | ⭐⭐ Low | $7-21/m | ✅ Prototypes, small projects |
| **Azure** | ⭐⭐⭐⭐ High | $50+/m | ✅ Enterprise, scaling |
| **Docker** | ⭐⭐⭐ Medium | VPS cost | ✅ Self-hosted, control |

---

## ✅ Production Readiness Checklist

- [ ] Environment variables configured securely
- [ ] JWT secret key generated (min 32 characters)
- [ ] CORS restricted to specific domains
- [ ] PostgreSQL with automatic backups
- [ ] Health checks configured
- [ ] Centralized logging (Seq, Application Insights)
- [ ] SSL/TLS certificates active
- [ ] Monitoring and alerts configured
- [ ] Database migrations tested
- [ ] Rollback strategy defined

---

## 🔧 Deployment Tools

| Tool | Purpose |
|------|----------|
| Docker | Application containerization |
| Render | Platform-as-a-Service hosting |
| Azure CLI | Azure resource management |
| GitHub Actions | CI/CD pipeline |
| Terraform | Infrastructure as Code (optional) |

---

## 📖 Useful Links

- [Technical documentation](../technical/README.md)
- [Development guide](../development/README.md)
- [Render Documentation](https://render.com/docs)
- [Azure App Service docs](https://learn.microsoft.com/azure/app-service/)
