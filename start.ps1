#!/usr/bin/env pwsh
# SimpleBlog - Quick Start Script

param(
    [Parameter()]
    [switch]$Clean
)

Write-Host "🚀 SimpleBlog Quick Start" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

# Clean build if requested
if ($Clean) {
    Write-Host "🧹 Cleaning solution..." -ForegroundColor Yellow
    dotnet clean SimpleBlog.sln
    Write-Host ""
}

Write-Host "📊 Database: PostgreSQL (docker-compose)" -ForegroundColor Green
Write-Host ""

# Check if Docker is running
Write-Host "🔍 Checking Docker..." -ForegroundColor Yellow
$dockerRunning = docker info 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker is not running!" -ForegroundColor Red
    Write-Host ""
    Write-Host "   SimpleBlog requires Docker Desktop to run PostgreSQL." -ForegroundColor Yellow
    Write-Host "   Download: https://www.docker.com/products/docker-desktop/" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   After installing Docker Desktop:" -ForegroundColor White
    Write-Host "   1. Start Docker Desktop" -ForegroundColor White
    Write-Host "   2. Wait for it to fully start (check system tray)" -ForegroundColor White
    Write-Host "   3. Run this script again" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "✅ Docker is running" -ForegroundColor Green
Write-Host ""

# Check if PostgreSQL is running
Write-Host "🔍 Checking PostgreSQL container..." -ForegroundColor Yellow
$postgresRunning = docker ps --filter "name=simpleblog-postgres" --filter "status=running" --format "{{.Names}}" 2>$null

if (-not $postgresRunning) {
    Write-Host "⚠️  PostgreSQL container is not running!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Starting PostgreSQL via docker-compose..." -ForegroundColor Cyan
    docker-compose up -d
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to start PostgreSQL!" -ForegroundColor Red
        Write-Host "   Check docker-compose.yml configuration" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "✅ PostgreSQL started" -ForegroundColor Green
    Write-Host "⏳ Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
} else {
    Write-Host "✅ PostgreSQL is already running" -ForegroundColor Green
}

Write-Host ""
Write-Host "📊 Database Connection:" -ForegroundColor Cyan
Write-Host "   Host: localhost:5432" -ForegroundColor White
Write-Host "   Database: simpleblog" -ForegroundColor White
Write-Host "   User: simpleblog_user" -ForegroundColor White
Write-Host "   Password: simpleblog_dev_password_123" -ForegroundColor White
Write-Host ""
Write-Host "🔧 pgAdmin Access:" -ForegroundColor Cyan
Write-Host "   URL: http://localhost:5050" -ForegroundColor White
Write-Host "   Email: admin@simpleblog.local" -ForegroundColor White
Write-Host "   Password: admin" -ForegroundColor White
Write-Host ""

# Build solution
Write-Host "🔨 Building solution..." -ForegroundColor Yellow
dotnet build SimpleBlog.sln

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful" -ForegroundColor Green
Write-Host ""

# Start application
Write-Host "🚀 Starting SimpleBlog..." -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Application will:" -ForegroundColor Yellow
Write-Host "   - Apply database migrations automatically" -ForegroundColor White
Write-Host "   - Create/update database schema" -ForegroundColor White
Write-Host "   - Start API and Web services" -ForegroundColor White
Write-Host ""
Write-Host "📍 Aspire Dashboard will open automatically" -ForegroundColor Yellow
Write-Host "📍 Press Ctrl+C to stop the application" -ForegroundColor Yellow
Write-Host ""

dotnet run --project SimpleBlog.AppHost
