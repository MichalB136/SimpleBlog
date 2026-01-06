-- Minimal PostgreSQL setup for SimpleBlog
-- Application (Entity Framework Core) handles all schema creation and migrations

-- Create useful extensions only
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- For text search optimization

-- Note: All tables, indexes, and data are managed by the application via EF Core
-- See SimpleBlog.ApiService/Program.cs for database initialization
