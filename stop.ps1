#!/usr/bin/env pwsh
# SimpleBlog - Stop and Clean Script

Write-Host "🛑 Stopping SimpleBlog" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker containers are running
$containers = docker ps --filter "name=simpleblog" --format "{{.Names}}" 2>$null

if ($containers) {
    Write-Host "🐳 Stopping Docker containers..." -ForegroundColor Yellow
    docker-compose stop
    
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Cyan
    Write-Host "  1. Keep data (just stopped)" -ForegroundColor Green
    Write-Host "  2. Remove containers (keep volumes)" -ForegroundColor Yellow
    Write-Host "  3. Remove everything including data" -ForegroundColor Red
    Write-Host ""
    
    $choice = Read-Host "Choose option (1-3)"
    
    switch ($choice) {
        "2" {
            Write-Host "🗑️  Removing containers..." -ForegroundColor Yellow
            docker-compose down
            Write-Host "✅ Containers removed, data preserved" -ForegroundColor Green
        }
        "3" {
            Write-Host "🗑️  Removing everything..." -ForegroundColor Yellow
            docker-compose down -v
            Write-Host "✅ All containers and data removed" -ForegroundColor Green
        }
        default {
            Write-Host "✅ Containers stopped, data preserved" -ForegroundColor Green
        }
    }
}
else {
    Write-Host "ℹ️  No SimpleBlog Docker containers running" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✅ Done!" -ForegroundColor Green
