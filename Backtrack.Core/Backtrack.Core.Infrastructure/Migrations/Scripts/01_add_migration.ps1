# =====================================================
# Script: Add New EF Core Migration
# Description: Creates a new migration from DbContext
# Usage: .\01_add_migration.ps1 [MigrationName]
# =====================================================

param(
    [string]$MigrationName = "InitialCreate"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EF Core - Add Migration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get script directory and navigate to Infrastructure project
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$InfrastructureDir = Split-Path -Parent (Split-Path -Parent $ScriptDir)
$WebApiDir = Join-Path (Split-Path -Parent $InfrastructureDir) "Backtrack.Core.WebApi"

Write-Host "Infrastructure Project: $InfrastructureDir" -ForegroundColor Yellow
Write-Host "Startup Project: $WebApiDir" -ForegroundColor Yellow
Write-Host "Migration Name: $MigrationName" -ForegroundColor Yellow
Write-Host ""

# Check if dotnet ef is installed
Write-Host "Checking dotnet ef tools..." -ForegroundColor Cyan
$efInstalled = dotnet tool list -g | Select-String "dotnet-ef"

if (-not $efInstalled) {
    Write-Host "dotnet ef tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "dotnet ef tools installed successfully!" -ForegroundColor Green
}
else {
    Write-Host "dotnet ef tools found!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Creating migration: $MigrationName" -ForegroundColor Cyan
Write-Host ""

# Navigate to Infrastructure directory
Set-Location $InfrastructureDir

# Run dotnet ef migrations add
try {
    dotnet ef migrations add $MigrationName `
        --startup-project $WebApiDir `
        --context ApplicationDbContext `
        --output-dir Migrations `
        --verbose

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Migration created successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Review the migration file in Migrations folder" -ForegroundColor White
    Write-Host "2. Run: dotnet ef database update" -ForegroundColor White
    Write-Host "   Or use: .\02_apply_migrations.ps1" -ForegroundColor White
}
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Migration creation failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
