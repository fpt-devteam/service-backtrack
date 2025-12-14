# =====================================================
# Script: Remove Last EF Core Migration
# Description: Removes the most recent migration
# Usage: .\02_remove_last_migration.ps1
# WARNING: This will delete the last migration file!
# =====================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EF Core - Remove Last Migration" -ForegroundColor Cyan
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

# List current migrations
Write-Host "Current migrations:" -ForegroundColor Cyan
$MigrationsDir = Join-Path $InfrastructureDir "Migrations"
if (Test-Path $MigrationsDir) {
    Get-ChildItem $MigrationsDir -Filter "*.cs" |
        Where-Object { $_.Name -notlike "*Designer.cs" -and $_.Name -ne "ApplicationDbContextModelSnapshot.cs" } |
        ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor White }
}
else {
    Write-Host "  No migrations found" -ForegroundColor Gray
}

Write-Host ""
Write-Host "WARNING: This will remove the last migration!" -ForegroundColor Red
$confirmation = Read-Host "Are you sure? (y/n)"

if ($confirmation -ne 'y') {
    Write-Host "Operation cancelled" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Removing last migration..." -ForegroundColor Cyan
Write-Host ""

# Navigate to Infrastructure directory
Set-Location $InfrastructureDir

# Run dotnet ef migrations remove
try {
    dotnet ef migrations remove `
        --startup-project $WebApiDir `
        --context ApplicationDbContext `
        --force `
        --verbose

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Last migration removed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Migration removal failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Note: If the migration was already applied to the database," -ForegroundColor Yellow
    Write-Host "you need to rollback the database first:" -ForegroundColor Yellow
    Write-Host "  dotnet ef database update <PreviousMigrationName>" -ForegroundColor White
    exit 1
}
