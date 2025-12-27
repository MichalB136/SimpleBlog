-- SimpleBlog SQL Server Initialization Script
-- This script runs automatically when the SQL Server container starts

-- Create SimpleBlog database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SimpleBlogDb')
BEGIN
    CREATE DATABASE SimpleBlogDb;
    PRINT 'Database SimpleBlogDb created successfully.';
END
ELSE
BEGIN
    PRINT 'Database SimpleBlogDb already exists.';
END

-- Use the new database
USE SimpleBlogDb;

-- Create initial schema (optional - EF Core will handle migrations)
-- Tables will be created by Entity Framework Code First migrations

PRINT 'SimpleBlog database initialization completed.';
