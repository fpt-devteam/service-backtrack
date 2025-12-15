# =====================================================
# Script: Truncate All Data from Database
# Description: Removes all data from all tables
# Usage: .\05_truncate_all_data.ps1
# =====================================================

# Connection String - Change this if needed
$DbHost = "localhost"
$DbPort = "5432"
$DbName = "backtrack-core-db"
$DbUser = "postgres"
$DbPassword = "postgres123"

Write-Host "WARNING: This will DELETE ALL data from database '$DbName'" -ForegroundColor Red
$confirmation = Read-Host "Continue? (yes/no)"

if ($confirmation -ne 'yes') {
    Write-Host "Cancelled" -ForegroundColor Yellow
    exit 0
}

Write-Host "Truncating all data..." -ForegroundColor Yellow

# SQL to truncate all tables
$TruncateSQL = @"
DO `$`$
DECLARE
   r RECORD;
   table_list TEXT := '';
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
        IF table_list != '' THEN table_list := table_list || ', '; END IF;
        table_list := table_list || 'public."' || r.tablename || '"';
    END LOOP;

    IF table_list != '' THEN
        EXECUTE 'TRUNCATE TABLE ' || table_list || ' RESTART IDENTITY CASCADE';
    END IF;
END `$`$;
"@

# Check if psql is available
$psqlExists = Get-Command psql -ErrorAction SilentlyContinue

if ($psqlExists) {
    # Execute with psql
    $TempFile = Join-Path $env:TEMP "truncate.sql"
    [System.IO.File]::WriteAllText($TempFile, $TruncateSQL)

    $env:PGPASSWORD = $DbPassword
    psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -f $TempFile

    Remove-Item $TempFile -ErrorAction SilentlyContinue
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue

    Write-Host "Done!" -ForegroundColor Green
}
else {
    # Save SQL file for manual execution
    $SqlFile = Join-Path $PSScriptRoot "truncate_all_data.sql"
    [System.IO.File]::WriteAllText($SqlFile, $TruncateSQL)

    Write-Host ""
    Write-Host "psql not found!" -ForegroundColor Red
    Write-Host "SQL file created: $SqlFile" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Option 1: Install PostgreSQL and run:" -ForegroundColor Cyan
    Write-Host "  psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -f `"$SqlFile`"" -ForegroundColor White
    Write-Host ""
    Write-Host "Option 2: Run the SQL manually in pgAdmin or your database tool" -ForegroundColor Cyan
    Write-Host ""
}
