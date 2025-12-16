# =====================================================
# Script: Truncate App Tables (Exclude Extension-owned)
# Description: Truncates all user-created tables in public schema,
#              excluding tables owned by any extension (PostGIS, etc.)
#              and excluding __EFMigrationsHistory
# Usage: .\03_truncate_all_data.ps1
# =====================================================

# Connection String - Change this if needed
$DbHost = "localhost"
$DbPort = "5432"
$DbName = "backtrack-core-db"
$DbUser = "postgres"
$DbPassword = "postgres123"

Write-Host "WARNING: This will TRUNCATE ALL DATA from domain tables in database '$DbName' (public schema, excluding extension-owned tables)" -ForegroundColor Red
$confirmation = Read-Host "Continue? (yes/no)"

if ($confirmation -ne 'yes') {
    Write-Host "Cancelled" -ForegroundColor Yellow
    exit 0
}

Write-Host "Truncating data from domain tables..." -ForegroundColor Yellow

# SQL:
# - Select ordinary tables (relkind = 'r') in schema public
# - Exclude extension-owned objects via pg_depend deptype='e'
# - Exclude EF migrations history table
# - Print tables to be truncated (NOTICE)
# - Truncate them in one statement: RESTART IDENTITY CASCADE
$TruncateSQL = @"
DO `$`$
DECLARE
  r RECORD;
  table_list TEXT := '';
BEGIN
  RAISE NOTICE 'Collecting tables to truncate...';

  FOR r IN (
    SELECT c.relname AS tablename
    FROM pg_class c
    JOIN pg_namespace n ON n.oid = c.relnamespace
    LEFT JOIN pg_depend d
      ON d.objid = c.oid
     AND d.deptype = 'e'           -- extension ownership dependency
    WHERE n.nspname = 'public'
      AND c.relkind = 'r'          -- ordinary tables only
      AND d.objid IS NULL          -- NOT owned by an extension
      AND c.relname <> '__EFMigrationsHistory'
    ORDER BY c.relname
  ) LOOP
    RAISE NOTICE 'Will truncate: public.%', r.tablename;

    table_list := table_list
      || CASE WHEN table_list <> '' THEN ', ' ELSE '' END
      || format('public.%I', r.tablename);
  END LOOP;

  IF table_list <> '' THEN
    RAISE NOTICE 'Executing: TRUNCATE TABLE %', table_list;
    EXECUTE 'TRUNCATE TABLE ' || table_list || ' RESTART IDENTITY CASCADE';
    RAISE NOTICE 'Done truncating domain tables.';
  ELSE
    RAISE NOTICE 'No domain tables found to truncate (or all tables are extension-owned / excluded).';
  END IF;
END `$`$;
"@

# Check if psql is available
$psqlExists = Get-Command psql -ErrorAction SilentlyContinue

if ($psqlExists) {
    # Execute with psql
    $TempFile = Join-Path $env:TEMP "truncate_domain_tables.sql"
    [System.IO.File]::WriteAllText($TempFile, $TruncateSQL)

    $env:PGPASSWORD = $DbPassword
    psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -v ON_ERROR_STOP=1 -f $TempFile

    $exitCode = $LASTEXITCODE

    Remove-Item $TempFile -ErrorAction SilentlyContinue
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue

    if ($exitCode -eq 0) {
        Write-Host "Done!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "psql exited with code $exitCode" -ForegroundColor Red
        exit $exitCode
    }
}
else {
    # Save SQL file for manual execution
    $SqlFile = Join-Path $PSScriptRoot "truncate_domain_tables.sql"
    [System.IO.File]::WriteAllText($SqlFile, $TruncateSQL)

    Write-Host ""
    Write-Host "psql not found!" -ForegroundColor Red
    Write-Host "SQL file created: $SqlFile" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Option 1: Install PostgreSQL and run:" -ForegroundColor Cyan
    Write-Host "  psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -v ON_ERROR_STOP=1 -f `"$SqlFile`"" -ForegroundColor White
    Write-Host ""
    Write-Host "Option 2: Run the SQL manually in pgAdmin or your database tool" -ForegroundColor Cyan
    Write-Host ""
}
