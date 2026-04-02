#!/bin/sh
# =====================================================
# Script: Remove Last EF Core Migration
# Description: Removes the most recent migration
# Usage: ./04_remove_last_migration.sh
# WARNING: This will delete the last migration file!
# =====================================================

echo "========================================"
echo "EF Core - Remove Last Migration"
echo "========================================"
echo ""

# Resolve directories relative to this script
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
INFRASTRUCTURE_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
WEBAPI_DIR="$(cd "$INFRASTRUCTURE_DIR/../Backtrack.Core.WebApi" && pwd)"

echo "Infrastructure Project: $INFRASTRUCTURE_DIR"
echo "Startup Project:        $WEBAPI_DIR"
echo ""

# Check if dotnet ef is installed
if ! dotnet tool list -g | grep -q "dotnet-ef"; then
    echo "Error: dotnet ef tools not found!"
    echo "Install with: dotnet tool install --global dotnet-ef"
    exit 1
fi

# List current migrations
echo "Current migrations:"
MIGRATIONS_DIR="$INFRASTRUCTURE_DIR/Migrations"
if [ -d "$MIGRATIONS_DIR" ]; then
    find "$MIGRATIONS_DIR" -maxdepth 1 -name "*.cs" \
        ! -name "*Designer.cs" \
        ! -name "ApplicationDbContextModelSnapshot.cs" \
        | sort | while IFS= read -r f; do
        echo "  - $(basename "$f")"
    done
else
    echo "  No migrations found"
fi

echo ""
echo "WARNING: This will remove the last migration!"
printf 'Are you sure? (y/n): '
read -r confirmation

if [ "$confirmation" != "y" ]; then
    echo "Operation cancelled"
    exit 0
fi

echo ""
echo "Removing last migration..."
echo ""

cd "$INFRASTRUCTURE_DIR" || exit 1

if dotnet ef migrations remove \
    --startup-project "$WEBAPI_DIR" \
    --context ApplicationDbContext \
    --force \
    --verbose; then
    echo ""
    echo "========================================"
    echo "Last migration removed successfully!"
    echo "========================================"
else
    echo ""
    echo "========================================"
    echo "Migration removal failed!"
    echo "========================================"
    echo ""
    echo "Note: If the migration was already applied to the database,"
    echo "you need to rollback the database first:"
    echo "  dotnet ef database update <PreviousMigrationName>"
    exit 1
fi
