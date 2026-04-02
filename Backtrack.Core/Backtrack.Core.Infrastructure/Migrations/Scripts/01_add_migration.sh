#!/bin/sh
# =====================================================
# Script: Add New EF Core Migration
# Description: Creates a new migration from DbContext
# Usage: ./01_add_migration.sh [MigrationName]
# =====================================================

MIGRATION_NAME="${1:-InitialCreate}"

echo "========================================"
echo "EF Core - Add Migration"
echo "========================================"
echo ""

# Resolve directories relative to this script
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
INFRASTRUCTURE_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
WEBAPI_DIR="$(cd "$INFRASTRUCTURE_DIR/../Backtrack.Core.WebApi" && pwd)"

echo "Infrastructure Project: $INFRASTRUCTURE_DIR"
echo "Startup Project:        $WEBAPI_DIR"
echo "Migration Name:         $MIGRATION_NAME"
echo ""

# Check if dotnet ef is installed
echo "Checking dotnet ef tools..."
if ! dotnet tool list -g | grep -q "dotnet-ef"; then
    echo "dotnet ef tools not found. Installing..."
    dotnet tool install --global dotnet-ef
    echo "dotnet ef tools installed successfully!"
else
    echo "dotnet ef tools found!"
fi

echo ""
echo "Creating migration: $MIGRATION_NAME"
echo ""

cd "$INFRASTRUCTURE_DIR" || exit 1

if dotnet ef migrations add "$MIGRATION_NAME" \
    --startup-project "$WEBAPI_DIR" \
    --context ApplicationDbContext \
    --output-dir Migrations \
    --verbose; then
    echo ""
    echo "========================================"
    echo "Migration created successfully!"
    echo "========================================"
    echo ""
    echo "Next steps:"
    echo "1. Review the migration file in Migrations folder"
    echo "2. Run: dotnet ef database update"
    echo "   Or use: ./02_apply_migrations.sh"
else
    echo ""
    echo "========================================"
    echo "Migration creation failed!"
    echo "========================================"
    exit 1
fi
