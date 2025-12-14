# =====================================================
# Script: Reset Database and Migrations
# Description: Drops all database objects and deletes all migration files
# Usage: .\03_reset_database_and_migrations.ps1
# WARNING: This will DELETE ALL data and migration files!
# =====================================================

param(
    [string]$ConnectionString = ""
)

Write-Host "========================================" -ForegroundColor Red
Write-Host "RESET DATABASE AND MIGRATIONS" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""
Write-Host "WARNING: This will:" -ForegroundColor Red
Write-Host "  1. Drop ALL tables, sequences, views, and functions in the database" -ForegroundColor Red
Write-Host "  2. Delete ALL migration files from Migrations folder" -ForegroundColor Red
Write-Host "  3. Delete the ModelSnapshot file" -ForegroundColor Red
Write-Host ""
Write-Host "THIS CANNOT BE UNDONE!" -ForegroundColor Red
Write-Host ""

# Get confirmation
$confirmation = Read-Host "Type 'DELETE EVERYTHING' to confirm"

if ($confirmation -ne 'DELETE EVERYTHING') {
    Write-Host ""
    Write-Host "Operation cancelled - you typed: '$confirmation'" -ForegroundColor Yellow
    exit 0
}

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$InfrastructureDir = Split-Path -Parent (Split-Path -Parent $ScriptDir)
$WebApiDir = Join-Path (Split-Path -Parent $InfrastructureDir) "Backtrack.Core.WebApi"
$MigrationsDir = Join-Path $InfrastructureDir "Migrations"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "STEP 1: Get Database Connection String" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get connection string
if ([string]::IsNullOrEmpty($ConnectionString)) {
    Write-Host "Enter connection string components:" -ForegroundColor Yellow
    $Host = Read-Host "Database Host (default: localhost)"
    if ([string]::IsNullOrEmpty($Host)) { $Host = "localhost" }

    $Port = Read-Host "Database Port (default: 5432)"
    if ([string]::IsNullOrEmpty($Port)) { $Port = "5432" }

    $Database = Read-Host "Database Name (default: backtrack_db)"
    if ([string]::IsNullOrEmpty($Database)) { $Database = "backtrack_db" }

    $User = Read-Host "Database User (default: postgres)"
    if ([string]::IsNullOrEmpty($User)) { $User = "postgres" }

    $Password = Read-Host "Database Password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
    $PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

    $ConnectionString = "Host=$Host;Port=$Port;Database=$Database;Username=$User;Password=$PlainPassword"
}

Write-Host ""
Write-Host "Connection: Host=$Host, Database=$Database" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "STEP 2: Drop All Database Objects" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# SQL script to drop everything
$DropAllSQL = @"
DO
`$`$
DECLARE
   r RECORD;
BEGIN
    RAISE NOTICE 'Starting database cleanup...';

    -- Drop all tables in public schema
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
        EXECUTE 'DROP TABLE IF EXISTS public."' || r.tablename || '" CASCADE';
        RAISE NOTICE 'Dropped table: %', r.tablename;
    END LOOP;

    -- Drop all sequences in public schema
    FOR r IN (SELECT sequence_name FROM information_schema.sequences WHERE sequence_schema = 'public') LOOP
        EXECUTE 'DROP SEQUENCE IF EXISTS public."' || r.sequence_name || '" CASCADE';
        RAISE NOTICE 'Dropped sequence: %', r.sequence_name;
    END LOOP;

    -- Drop all views in public schema
    FOR r IN (SELECT table_name FROM information_schema.views WHERE table_schema = 'public') LOOP
        EXECUTE 'DROP VIEW IF EXISTS public."' || r.table_name || '" CASCADE';
        RAISE NOTICE 'Dropped view: %', r.table_name;
    END LOOP;

    -- Drop all functions in public schema
    FOR r IN (
        SELECT routine_name, routine_type
        FROM information_schema.routines
        WHERE routine_schema = 'public'
    ) LOOP
        EXECUTE 'DROP ' || r.routine_type || ' IF EXISTS public."' || r.routine_name || '" CASCADE';
        RAISE NOTICE 'Dropped %: %', r.routine_type, r.routine_name;
    END LOOP;

    RAISE NOTICE 'Database cleanup completed!';
END;
`$`$;
"@

# Check if psql is available
$psqlExists = Get-Command psql -ErrorAction SilentlyContinue

if ($psqlExists) {
    Write-Host "Using psql to drop database objects..." -ForegroundColor Yellow

    # Save SQL to temp file
    $TempSqlFile = Join-Path $env:TEMP "drop_all.sql"
    $DropAllSQL | Out-File -FilePath $TempSqlFile -Encoding UTF8

    try {
        # Execute SQL
        $env:PGPASSWORD = $PlainPassword
        psql -h $Host -p $Port -U $User -d $Database -f $TempSqlFile

        Write-Host ""
        Write-Host "Database objects dropped successfully!" -ForegroundColor Green
    }
    catch {
        Write-Host ""
        Write-Host "Error dropping database objects: $_" -ForegroundColor Red
        Write-Host "You may need to drop the database manually." -ForegroundColor Yellow
    }
    finally {
        Remove-Item $TempSqlFile -ErrorAction SilentlyContinue
        Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
    }
}
else {
    Write-Host "psql not found. Attempting to use EF Core database drop..." -ForegroundColor Yellow

    Set-Location $InfrastructureDir

    try {
        dotnet ef database drop --force --startup-project $WebApiDir --context ApplicationDbContext
        Write-Host ""
        Write-Host "Database dropped successfully!" -ForegroundColor Green
    }
    catch {
        Write-Host ""
        Write-Host "Could not drop database automatically." -ForegroundColor Yellow
        Write-Host "Please run this SQL manually:" -ForegroundColor Yellow
        Write-Host ""
        Write-Host $DropAllSQL -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "STEP 3: Delete All Migration Files" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $MigrationsDir) {
    $MigrationFiles = Get-ChildItem $MigrationsDir -Filter "*.cs"

    if ($MigrationFiles.Count -gt 0) {
        Write-Host "Found $($MigrationFiles.Count) migration files:" -ForegroundColor Yellow
        $MigrationFiles | ForEach-Object {
            Write-Host "  - $($_.Name)" -ForegroundColor Gray
        }

        Write-Host ""
        Write-Host "Deleting migration files..." -ForegroundColor Yellow

        foreach ($file in $MigrationFiles) {
            Remove-Item $file.FullName -Force
            Write-Host "  Deleted: $($file.Name)" -ForegroundColor Green
        }

        Write-Host ""
        Write-Host "All migration files deleted!" -ForegroundColor Green
    }
    else {
        Write-Host "No migration files found to delete" -ForegroundColor Gray
    }

    # Also delete the Scripts folder SQL files if they exist
    $SqlScriptsDir = Join-Path $MigrationsDir "Scripts"
    if (Test-Path $SqlScriptsDir) {
        $SqlFiles = Get-ChildItem $SqlScriptsDir -Filter "*.sql"
        if ($SqlFiles.Count -gt 0) {
            Write-Host ""
            Write-Host "Cleaning up SQL scripts..." -ForegroundColor Yellow
            $SqlFiles | Where-Object { $_.Name -like "0*" } | ForEach-Object {
                Remove-Item $_.FullName -Force
                Write-Host "  Deleted: Scripts\$($_.Name)" -ForegroundColor Green
            }
        }
    }
}
else {
    Write-Host "Migrations directory not found" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "RESET COMPLETED!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Create a new migration:" -ForegroundColor White
Write-Host "   .\01_add_migration.ps1 InitialCreate" -ForegroundColor White
Write-Host ""
Write-Host "2. Apply migration to database:" -ForegroundColor White
Write-Host "   cd .." -ForegroundColor White
Write-Host "   dotnet ef database update --startup-project ..\Backtrack.Core.WebApi" -ForegroundColor White
Write-Host ""
