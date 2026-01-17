<#
.SYNOPSIS
    Starts the SimpleBlog application with PostgreSQL database.

.DESCRIPTION
    This script checks Docker availability, starts PostgreSQL container if needed,
    and launches the SimpleBlog application using .NET Aspire AppHost.

.PARAMETER Clean
    Performs a clean build before starting the application.

.EXAMPLE
    .\Start-SimpleBlog.ps1
    Starts SimpleBlog with existing build.

.EXAMPLE
    .\Start-SimpleBlog.ps1 -Clean
    Cleans solution and starts SimpleBlog with fresh build.

.NOTES
    Requires:
    - Docker Desktop installed and running
    - .NET 9.0 SDK
    - SimpleBlog.sln in parent directory

.LINK
    https://github.com/MichalB136/SimpleBlog
#>

[CmdletBinding()]
param(
    [Parameter(HelpMessage = "Perform clean build before starting")]
    [switch]$Clean
)

#Requires -Version 7.0

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Constants
$Script:SolutionPath = Join-Path $PSScriptRoot '..' 'SimpleBlog.sln'
$Script:AppHostProject = Join-Path $PSScriptRoot '..' 'SimpleBlog.AppHost' 'SimpleBlog.AppHost.csproj'
$Script:DockerComposeFile = Join-Path $PSScriptRoot '..' 'docker-compose.yml'

function Write-Header {
    [CmdletBinding()]
    param()
    
    Write-Host '🚀 SimpleBlog Quick Start' -ForegroundColor Cyan
    Write-Host '=========================' -ForegroundColor Cyan
    Write-Host ''
}

function Test-DockerAvailability {
    <#
    .SYNOPSIS
        Verifies Docker Desktop is installed and running.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param()
    
    Write-Host '🔍 Checking Docker...' -ForegroundColor Yellow
    
    try {
        $null = docker info 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ Docker is running' -ForegroundColor Green
            Write-Host ''
            return $true
        }
    }
    catch {
        # Docker command not found
    }
    
    Write-Host '❌ Docker is not running!' -ForegroundColor Red
    Write-Host ''
    Write-Host '   SimpleBlog requires Docker Desktop to run PostgreSQL.' -ForegroundColor Yellow
    Write-Host '   Download: https://www.docker.com/products/docker-desktop/' -ForegroundColor Cyan
    Write-Host ''
    Write-Host '   After installing Docker Desktop:' -ForegroundColor White
    Write-Host '   1. Start Docker Desktop' -ForegroundColor White
    Write-Host '   2. Wait for it to fully start (check system tray)' -ForegroundColor White
    Write-Host '   3. Run this script again' -ForegroundColor White
    Write-Host ''
    
    return $false
}

function Test-PostgreSQLContainer {
    <#
    .SYNOPSIS
        Checks if PostgreSQL container is running.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param()
    
    Write-Host '🔍 Checking PostgreSQL container...' -ForegroundColor Yellow
    
    $containerName = docker ps --filter 'name=simpleblog-postgres' --filter 'status=running' --format '{{.Names}}' 2>$null
    
    if ($containerName) {
        Write-Host "✅ PostgreSQL is running (container: $containerName)" -ForegroundColor Green
        Write-Host ''
        return $true
    }
    
    return $false
}

function Start-PostgreSQLContainer {
    <#
    .SYNOPSIS
        Starts PostgreSQL container using docker-compose.
    #>
    [CmdletBinding()]
    param()
    
    Write-Host '⚠️  PostgreSQL container is not running!' -ForegroundColor Yellow
    Write-Host ''
    Write-Host '   Starting PostgreSQL via docker-compose...' -ForegroundColor Cyan
    Write-Host ''
    
    try {
        Push-Location (Split-Path $Script:DockerComposeFile -Parent)
        docker-compose up -d
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ PostgreSQL started successfully' -ForegroundColor Green
            Write-Host ''
            
            # Wait for PostgreSQL to be ready
            Write-Host '⏳ Waiting for PostgreSQL to be ready...' -ForegroundColor Yellow
            Start-Sleep -Seconds 5
            Write-Host '✅ PostgreSQL is ready' -ForegroundColor Green
            Write-Host ''
        }
        else {
            throw 'docker-compose failed to start PostgreSQL'
        }
    }
    catch {
        Write-Error "Failed to start PostgreSQL: $_"
        return
    }
    finally {
        Pop-Location
    }
}

function Invoke-CleanBuild {
    <#
    .SYNOPSIS
        Cleans the solution build artifacts.
    #>
    [CmdletBinding()]
    param()
    
    Write-Host '🧹 Cleaning solution...' -ForegroundColor Yellow
    
    try {
        dotnet clean $Script:SolutionPath --nologo --verbosity quiet
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ Solution cleaned' -ForegroundColor Green
            Write-Host ''
        }
        else {
            throw 'dotnet clean failed'
        }
    }
    catch {
        Write-Error "Failed to clean solution: $_"
        return
    }
}

function Start-Application {
    <#
    .SYNOPSIS
        Starts the SimpleBlog application using AppHost.
    #>
    [CmdletBinding()]
    param()
    
    Write-Host '🌐 Starting SimpleBlog...' -ForegroundColor Cyan
    Write-Host ''
    Write-Host '   📊 Database: PostgreSQL (docker-compose)' -ForegroundColor Green
    Write-Host '   🎯 Orchestration: .NET Aspire AppHost' -ForegroundColor Green
    Write-Host ''
    Write-Host '⏳ Building and starting application...' -ForegroundColor Yellow
    Write-Host '   (This may take a minute on first run)' -ForegroundColor Gray
    Write-Host ''
    Write-Host '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━' -ForegroundColor Gray
    Write-Host ''
    
    try {
        dotnet run --project $Script:AppHostProject
    }
    catch {
        Write-Error "Failed to start application: $_"
    }
}

# Main execution
try {
    Write-Header
    
    # Verify Docker
    if (-not (Test-DockerAvailability)) {
        exit 1
    }
    
    # Check/Start PostgreSQL
    if (-not (Test-PostgreSQLContainer)) {
        Start-PostgreSQLContainer
    }
    
    # Clean build if requested
    if ($Clean) {
        Invoke-CleanBuild
    }
    
    # Start application
    Start-Application
}
catch {
    Write-Error "An error occurred: $_"
    exit 1
}
