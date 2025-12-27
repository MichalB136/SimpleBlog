# Persistent Local Database Implementation - SimpleBlog

## Overview
The SimpleBlog application now uses a persistent local SQLite database instead of recreating it on each application restart.

## Key Changes

### 1. Database Initialization Strategy
**Location:** [SimpleBlog.ApiService/Program.cs](SimpleBlog.ApiService/Program.cs) (Lines 117-131)

**Before:**
```csharp
db.Database.Migrate();  // Requires migrations, can be destructive
```

**After:**
```csharp
db.Database.EnsureCreated();  // Creates if missing, does nothing if exists
```

### 2. Conditional Data Seeding
**Location:** [SimpleBlog.ApiService/Program.cs](SimpleBlog.ApiService/Program.cs) (Lines 134-136)

```csharp
// Seed initial data if database is empty
if (!db.Posts.Any())
{
    // Add 21 products, 7 posts, 4 comments (only on first run)
}
```

## How It Works

### First Application Start
1. Application launches and creates a database scope
2. `EnsureCreated()` is called - **creates** `simpleblog.db` file
3. Conditional seeding checks if database has posts: `if (!db.Posts.Any())`
   - ✓ Database is empty, so seeding runs
   - Adds 21 products, 7 blog posts, 4 comments
4. Application is ready with initial data

### Second and Subsequent Starts
1. Application launches and creates a database scope
2. `EnsureCreated()` is called - **does nothing** (database exists)
3. Conditional seeding checks if database has posts: `if (!db.Posts.Any())`
   - ✗ Database already has posts, so seeding is skipped
4. Application loads existing data without duplication
5. Any data added via UI persists across restarts

### User-Added Data
- New products added via "Shop" → "Add Product" persist
- New blog posts added via "Blog" → "Add Post" persist
- New comments on blog posts persist
- All changes survive application restart

## Database Files

The SQLite database creates three files in `c:\Code\SimpleBlog\`:

1. **simpleblog.db** - Main database file (contains all data)
2. **simpleblog.db-wal** - Write-Ahead Log (transaction log for durability)
3. **simpleblog.db-shm** - Shared Memory file (temporary, for concurrency)

## Advantages Over Previous Approach

| Aspect | Before (Migrate) | After (EnsureCreated) |
|--------|------------------|----------------------|
| First Run | Requires migration setup | Automatic database creation |
| Subsequent Runs | Database recreated | Database persists |
| Data Preservation | ✗ Lost on each restart | ✓ Fully preserved |
| Conditional Seeding | ✗ Manual checks needed | ✓ Automatic with simple check |
| Development Speed | ⚠️ Slower (manual migrations) | ✓ Faster (no migration steps) |
| Production Ready | ✗ Not recommended | ⚠️ Development only (upgrade for prod) |

## Testing the Implementation

Run the automated test script:
```powershell
cd c:\Code\SimpleBlog
.\test-persistent-db.ps1
```

**Manual Testing:**
1. Start application: `cd c:\Code\SimpleBlog\SimpleBlog.AppHost; dotnet run`
2. Add a new product or blog post via the web UI
3. Stop the application (Ctrl+C)
4. Restart the application
5. Verify the new data is still present

## Configuration

### Connection String
- **Database:** SQLite (file-based, local)
- **Location:** `c:\Code\SimpleBlog\simpleblog.db`
- **Defined in:** [SimpleBlog.ApiService/Program.cs](SimpleBlog.ApiService/Program.cs)

### Application Startup Flow
```
SimpleBlog.AppHost (Orchestrator)
    └─> SimpleBlog.ApiService (API Backend)
        └─> ApplicationDbContext
            └─> simpleblog.db (Persistent local file)
    └─> SimpleBlog.Web (Frontend)
        └─> Connects to API Service
```

## Production Considerations

⚠️ **Important:** `EnsureCreated()` + conditional seeding is designed for **development only**.

For production deployment:

### Option 1: Switch to EF Core Migrations
```bash
cd SimpleBlog.ApiService
dotnet ef migrations add InitialCreate
# Then use db.Database.Migrate() instead of EnsureCreated()
```

### Option 2: Use Managed Database Services
- Azure SQL Database
- AWS RDS
- Cloud-hosted PostgreSQL

### Option 3: Docker Volume for SQLite
```yaml
services:
  api:
    volumes:
      - db-volume:/app/data  # Persist database in Docker volume
    environment:
      - CONNECTION_STRING=Data Source=/app/data/simpleblog.db
```

## Troubleshooting

### Database File Locked
**Error:** "The process cannot access the file... it is being used by another process."
**Solution:** Kill the application process and retry build:
```powershell
taskkill /F /IM dotnet.exe
```

### Data Not Persisting
**Check:** Verify `simpleblog.db` exists in `c:\Code\SimpleBlog\`
```powershell
Get-ChildItem c:\Code\SimpleBlog\*.db*
```

### Conditional Seeding Not Working
**Check:** Ensure the condition is correct in Program.cs:
```csharp
if (!db.Posts.Any())  // Only seed if no posts exist
```

## Files Modified

1. **[SimpleBlog.ApiService/Program.cs](SimpleBlog.ApiService/Program.cs)**
   - Changed from `Migrate()` to `EnsureCreated()`
   - Added conditional seeding check
   - Lines: 117-136

## Summary

✅ **Persistent local database implemented successfully**
- Database file: `c:\Code\SimpleBlog\simpleblog.db`
- Seeding runs only once on first application start
- All data persists between application restarts
- Full backward compatibility maintained
- No migration files required for development

**Next Steps:**
1. Test the persistent database with `.\test-persistent-db.ps1`
2. Add products/posts through the UI and verify they persist across restarts
3. Consider migration to EF Core migrations for production deployment
