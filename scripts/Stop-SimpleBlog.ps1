<#
.SYNOPSIS
    Stops SimpleBlog Docker containers with optional cleanup.

.DESCRIPTION
    This script stops SimpleBlog PostgreSQL containers and optionally removes
    containers and volumes based on user selection.

.PARAMETER RemoveContainers
    Removes containers but keeps volumes (data preserved).

.PARAMETER RemoveAll
    Removes containers and volumes (all data deleted).

.PARAMETER Force
    Skips confirmation prompt for RemoveAll operation.

.EXAMPLE
    .\Stop-SimpleBlog.ps1
    Stops containers, keeps everything.

.EXAMPLE
    .\Stop-SimpleBlog.ps1 -RemoveContainers
    Stops and removes containers, keeps data.

.EXAMPLE
    .\Stop-SimpleBlog.ps1 -RemoveAll -Force
    Removes everything without confirmation.

.NOTES
    Requires Docker Desktop installed and running.

.LINK
    https://github.com/MichalB136/SimpleBlog
#>

[CmdletBinding(DefaultParameterSetName = 'Stop')]
param(
    [Parameter(ParameterSetName = 'RemoveContainers', HelpMessage = 'Remove containers but keep volumes')]
    [switch]$RemoveContainers,
    
    [Parameter(ParameterSetName = 'RemoveAll', HelpMessage = 'Remove containers and volumes (data deleted)')]
    [switch]$RemoveAll,
    
    [Parameter(ParameterSetName = 'RemoveAll', HelpMessage = 'Skip confirmation prompt')]
    [switch]$Force
)

#Requires -Version 7.0

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Constants
$Script:DockerComposeFile = Join-Path $PSScriptRoot '..' 'docker-compose.yml'

function Write-Header {
    [CmdletBinding()]
    param()
    
    Write-Host '🛑 Stopping SimpleBlog' -ForegroundColor Cyan
    Write-Host '=====================' -ForegroundColor Cyan
    Write-Host ''
}

function Get-RunningContainers {
    <#
    .SYNOPSIS
        Gets list of running SimpleBlog containers.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()
    
    try {
        $containers = docker ps --filter 'name=simpleblog' --format '{{.Names}}' 2>$null
        
        if ($LASTEXITCODE -eq 0 -and $containers) {
            return $containers -split "`n" | Where-Object { $_ }
        }
        
        return @()
    }
    catch {
        Write-Warning "Failed to query Docker containers: $_"
        return @()
    }
}

function Stop-Containers {
    <#
    .SYNOPSIS
        Stops Docker containers using docker-compose.
    #>
    [CmdletBinding()]
    param()
    
    Write-Host '🐳 Stopping Docker containers...' -ForegroundColor Yellow
    
    try {
        Push-Location (Split-Path $Script:DockerComposeFile -Parent)
        docker-compose stop
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ Containers stopped, data preserved' -ForegroundColor Green
        }
        else {
            throw 'docker-compose stop failed'
        }
    }
    catch {
        Write-Error "Failed to stop containers: $_"
    }
    finally {
        Pop-Location
    }
}

function Remove-Containers {
    <#
    .SYNOPSIS
        Removes Docker containers but keeps volumes.
    #>
    [CmdletBinding()]
    param()
    
    Write-Host '🗑️  Removing containers...' -ForegroundColor Yellow
    
    try {
        Push-Location (Split-Path $Script:DockerComposeFile -Parent)
        docker-compose down
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ Containers removed, data preserved' -ForegroundColor Green
        }
        else {
            throw 'docker-compose down failed'
        }
    }
    catch {
        Write-Error "Failed to remove containers: $_"
    }
    finally {
        Pop-Location
    }
}

function Remove-ContainersAndVolumes {
    <#
    .SYNOPSIS
        Removes Docker containers and volumes (all data deleted).
    #>
    [CmdletBinding()]
    param(
        [switch]$Force
    )
    
    if (-not $Force) {
        Write-Host '⚠️  WARNING: This will delete ALL database data!' -ForegroundColor Red
        Write-Host ''
        $confirmation = Read-Host 'Type "DELETE" to confirm'
        
        if ($confirmation -ne 'DELETE') {
            Write-Host 'Operation cancelled' -ForegroundColor Yellow
            return
        }
    }
    
    Write-Host '🗑️  Removing everything...' -ForegroundColor Yellow
    
    try {
        Push-Location (Split-Path $Script:DockerComposeFile -Parent)
        docker-compose down -v
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ All containers and data removed' -ForegroundColor Green
        }
        else {
            throw 'docker-compose down -v failed'
        }
    }
    catch {
        Write-Error "Failed to remove containers and volumes: $_"
    }
    finally {
        Pop-Location
    }
}

function Show-InteractiveMenu {
    <#
    .SYNOPSIS
        Shows interactive menu for cleanup options.
    #>
    [CmdletBinding()]
    param()
    
    Write-Host 'Options:' -ForegroundColor Cyan
    Write-Host '  1. Keep data (just stopped)' -ForegroundColor Green
    Write-Host '  2. Remove containers (keep volumes)' -ForegroundColor Yellow
    Write-Host '  3. Remove everything including data' -ForegroundColor Red
    Write-Host ''
    
    $choice = Read-Host 'Choose option (1-3)'
    
    switch ($choice) {
        '2' {
            Remove-Containers
        }
        '3' {
            Remove-ContainersAndVolumes -Force:$false
        }
        default {
            Write-Host '✅ Containers stopped, data preserved' -ForegroundColor Green
        }
    }
}

# Main execution
try {
    Write-Header
    
    $containers = Get-RunningContainers
    
    if ($containers.Count -eq 0) {
        Write-Host 'ℹ️  No SimpleBlog Docker containers running' -ForegroundColor Yellow
        Write-Host ''
        exit 0
    }
    
    Write-Host "Found $($containers.Count) running container(s):" -ForegroundColor Cyan
    foreach ($container in $containers) {
        Write-Host "  • $container" -ForegroundColor Gray
    }
    Write-Host ''
    
    # Execute based on parameters
    switch ($PSCmdlet.ParameterSetName) {
        'RemoveContainers' {
            Stop-Containers
            Remove-Containers
        }
        'RemoveAll' {
            Stop-Containers
            Remove-ContainersAndVolumes -Force:$Force
        }
        default {
            Stop-Containers
            Write-Host ''
            Show-InteractiveMenu
        }
    }
    
    Write-Host ''
    Write-Host '✅ Done!' -ForegroundColor Green
}
catch {
    Write-Error "An error occurred: $_"
    exit 1
}
