# EF Core Migration Quick Guide

## One-Time PowerShell Setup

Before running any scripts, enable script execution:

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
```

## Quick Start - Copy & Paste

### Navigate to Scripts Folder

```powershell
cd C:\Users\NHATTHANG\FptUniversity\SEP490\service-backtrack\Backtrack.Core\Backtrack.Core.Infrastructure\Migrations\Scripts
```

## Common Scenarios

### 1. Add New Migration

```powershell
# Create migration with descriptive name
.\01_add_migration.ps1 AddUserDisplayName

# Apply to database
.\02_apply_migrations.ps1
```

### 2. Undo Last Migration

```powershell
# Remove last migration (before applying to DB)
.\04_remove_last_migration.ps1
```

### 3. Reset Everything (Clean Slate)

```powershell
# WARNING: Drops all database objects and deletes migration files
.\03_reset_database_and_migrations.ps1
```

## Database Connection

Update connection string in `Backtrack.Core.WebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=backtrack;Username=postgres;Password=yourpassword"
  }
}
```

## Troubleshooting

**Error: "Running scripts is disabled on this system"**
- Run: `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned`

**Error: "dotnet ef not found"**
- Scripts auto-install it, or run: `dotnet tool install --global dotnet-ef`

**Error: "No DbContext was found"**
- Ensure you're in the Infrastructure project directory
- Check that ApplicationDbContext exists

**Migration fails to apply**
- Check database connection string
- Verify PostgreSQL is running
- Review migration file for errors
