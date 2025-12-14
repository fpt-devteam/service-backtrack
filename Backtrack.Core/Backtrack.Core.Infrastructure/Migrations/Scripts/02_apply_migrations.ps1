# =====================================================
# Script: Apply Migrations to Database
# Description: Updates database with pending migrations
# Usage: .\04_apply_migrations.ps1
# =====================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EF Core - Apply Migrations" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get script directory and navigate to Infrastructure project
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$InfrastructureDir = Split-Path -Parent (Split-Path -Parent $ScriptDir)
$WebApiDir = Join-Path (Split-Path -Parent $InfrastructureDir) "Backtrack.Core.WebApi"

Write-Host "Infrastructure Project: $InfrastructureDir" -ForegroundColor Yellow
Write-Host "Startup Project: $WebApiDir" -ForegroundColor Yellow
Write-Host ""

# Check if dotnet ef is installed
$efInstalled = dotnet tool list -g | Select-String "dotnet-ef"

if (-not $efInstalled) {
    Write-Host "Error: dotnet ef tools not found!" -ForegroundColor Red
    Write-Host "Install with: dotnet tool install --global dotnet-ef" -ForegroundColor Yellow
    exit 1
}

# Navigate to Infrastructure directory
Set-Location $InfrastructureDir

Write-Host "Applying migrations to database..." -ForegroundColor Cyan
Write-Host ""

try {
    dotnet ef database update `
        --startup-project $WebApiDir `
        --context ApplicationDbContext `
        --verbose

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Migrations applied successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Migration application failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
