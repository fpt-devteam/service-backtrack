# EF Core Migration Scripts

PowerShell scripts to manage Entity Framework Core migrations for Backtrack.Core project.

## Scripts Overview

### 1. `01_add_migration.ps1` - Create New Migration
Creates a new EF Core migration from your DbContext and entity configurations.

**Usage:**
```powershell
.\01_add_migration.ps1 [MigrationName]
```

**Example:**
```powershell
.\01_add_migration.ps1 InitialCreate
.\01_add_migration.ps1 AddUserTable
```

**What it does:**
- Checks if `dotnet-ef` tools are installed
- Runs `dotnet ef migrations add` with proper parameters
- Creates migration files in `Migrations` folder
- Shows next steps after creation

---

### 2. `02_remove_last_migration.ps1` - Remove Last Migration
Removes the most recently created migration (not yet applied).

**Usage:**
```powershell
.\02_remove_last_migration.ps1
```

**What it does:**
- Lists current migrations
- Asks for confirmation
- Runs `dotnet ef migrations remove`
- Deletes the last migration files

**⚠️ Important:**
- Only works if migration hasn't been applied to database
- If migration was applied, rollback database first:
  ```powershell
  dotnet ef database update PreviousMigrationName
  ```

---

### 3. `03_reset_database_and_migrations.ps1` - Complete Reset
Drops ALL database objects and deletes ALL migration files.

**Usage:**
```powershell
.\03_reset_database_and_migrations.ps1
```

**What it does:**
1. Prompts for database connection details
2. Drops ALL tables, sequences, views, functions in database
3. Deletes ALL migration files from Migrations folder
4. Cleans up SQL script files

**⚠️ DANGER:**
- Deletes ALL data in the database
- Deletes ALL migration history
- Cannot be undone
- Requires typing 'DELETE EVERYTHING' to confirm

---

### 4. `04_apply_migrations.ps1` - Apply Migrations
Applies pending migrations to the database.

**Usage:**
```powershell
.\04_apply_migrations.ps1
```

**What it does:**
- Runs `dotnet ef database update`
- Applies all pending migrations
- Updates database schema

---

### 5. `05_truncate_all_data.ps1` - Truncate All Data
Removes all data from all tables while preserving schema and migrations.

**Usage:**
```powershell
.\05_truncate_all_data.ps1
```

**What it does:**
1. Prompts for database connection details
2. Truncates ALL data from ALL tables in public schema
3. Resets all auto-increment sequences to 1
4. Verifies row counts after truncation
5. Keeps table structure intact
6. Preserves migration history

**⚠️ WARNING:**
- Deletes ALL data from ALL tables
- Cannot be undone
- Keeps database schema intact
- Requires typing 'TRUNCATE ALL DATA' to confirm

**When to use:**
- Clean database for fresh data import
- Reset test/development database without recreating schema
- Clear all data but maintain structure

**Difference from script 03:**
- Script 03: Drops tables + deletes migrations (complete reset)
- Script 05: Empties tables + keeps migrations (data reset only)

---

## Quick Start Workflow

### First Time Setup
```powershell
# 1. Create initial migration
.\01_add_migration.ps1 InitialCreate

# 2. Apply to database
.\04_apply_migrations.ps1
```

### Making Schema Changes
```powershell
# 1. Update your entity classes or DbContext
# 2. Create new migration
.\01_add_migration.ps1 AddNewFeature

# 3. Apply to database
.\04_apply_migrations.ps1
```

### Fix Mistakes
```powershell
# If migration not yet applied:
.\02_remove_last_migration.ps1

# If migration was already applied:
dotnet ef database update PreviousMigrationName
.\02_remove_last_migration.ps1
```

### Complete Reset (Development Only!)
```powershell
# WARNING: Deletes everything!
.\03_reset_database_and_migrations.ps1

# Then create fresh migration
.\01_add_migration.ps1 InitialCreate
.\04_apply_migrations.ps1
```

### Clear Data Only (Keep Schema)
```powershell
# Remove all data but keep tables and migrations
.\05_truncate_all_data.ps1

# Database is now empty but ready for new data
# No need to reapply migrations!
```

---

## Prerequisites

### Required Tools
```powershell
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef

# Verify installation
dotnet ef --version
```

### PostgreSQL Tools (for script 03)
- Install PostgreSQL client tools (includes `psql`)
- Or the script will attempt to use EF Core commands

---

## Configuration

Scripts automatically detect:
- Infrastructure project: `Backtrack.Core.Infrastructure`
- Startup project: `Backtrack.Core.WebApi`
- DbContext: `ApplicationDbContext`
- Output directory: `Migrations`

### Custom Connection String
For script 03, you can provide connection string:
```powershell
.\03_reset_database_and_migrations.ps1 -ConnectionString "Host=localhost;Port=5432;Database=mydb;Username=user;Password=pass"
```

Or it will prompt interactively.

---

## Troubleshooting

### "dotnet ef not found"
```powershell
dotnet tool install --global dotnet-ef
```

### "Build failed"
Build the project first:
```powershell
cd ..\Backtrack.Core.WebApi
dotnet build
```

### "Cannot remove migration"
Migration was already applied. Rollback first:
```powershell
# List migrations
dotnet ef migrations list

# Rollback to previous
dotnet ef database update PreviousMigrationName
```

### "Connection string not found"
Check `appsettings.Development.json` in WebApi project:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=backtrack_db;Username=postgres;Password=yourpassword"
  }
}
```

---

## Manual Commands

If you prefer manual commands:

```powershell
# Navigate to Infrastructure
cd Backtrack.Core.Infrastructure

# Add migration
dotnet ef migrations add MigrationName --startup-project ..\Backtrack.Core.WebApi

# Remove last migration
dotnet ef migrations remove --startup-project ..\Backtrack.Core.WebApi

# Apply migrations
dotnet ef database update --startup-project ..\Backtrack.Core.WebApi

# List migrations
dotnet ef migrations list --startup-project ..\Backtrack.Core.WebApi

# Generate SQL script
dotnet ef migrations script --startup-project ..\Backtrack.Core.WebApi --output migration.sql
```

---

## Best Practices

1. **Always create migrations for schema changes**
   - Don't modify database manually
   - Let EF Core track changes

2. **Use descriptive migration names**
   - Good: `AddUserEmailIndex`, `UpdateUserStatusColumn`
   - Bad: `Migration1`, `Fix`, `Update`

3. **Review generated migrations**
   - Check the generated code
   - Ensure it matches your intent
   - Add custom SQL if needed

4. **Test migrations**
   - Test in development first
   - Then staging
   - Finally production

5. **Backup before production migrations**
   ```powershell
   pg_dump -h host -U user dbname > backup.sql
   ```

6. **Never use script 03 in production!**
   - Only for development/testing
   - Use proper migration rollback instead

---

## Environment-Specific Usage

### Development
```powershell
# Safe to use all scripts
.\01_add_migration.ps1 FeatureName
.\04_apply_migrations.ps1

# Clear data only (keeps schema)
.\05_truncate_all_data.ps1

# Full reset when needed (drops everything)
.\03_reset_database_and_migrations.ps1
```

### Staging
```powershell
# Only add and apply migrations
.\01_add_migration.ps1 FeatureName
.\04_apply_migrations.ps1

# Never reset!
```

### Production
```powershell
# Only apply migrations (created in dev)
.\04_apply_migrations.ps1

# Or use SQL script for review:
dotnet ef migrations script --idempotent --output prod_migration.sql
# Review SQL, then apply manually
```

---

## Generated Files

After running `01_add_migration.ps1 InitialCreate`:

```
Migrations/
├── 20251214120000_InitialCreate.cs          # Up/Down methods
├── 20251214120000_InitialCreate.Designer.cs  # Migration metadata
└── ApplicationDbContextModelSnapshot.cs      # Current model state
```

---

## Support

For issues:
1. Check build errors first
2. Verify connection string
3. Ensure PostgreSQL is running
4. Check EF Core tools version
5. Review migration files for errors

---

**Last Updated:** 2025-12-14
**EF Core Version:** 9.0.4
**Target Framework:** .NET 8.0
