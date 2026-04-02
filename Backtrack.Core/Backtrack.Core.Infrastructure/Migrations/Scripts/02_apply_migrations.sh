#!/bin/sh
# =====================================================
# Script: Apply Migrations to Database
# Description: Updates database with pending migrations
# Usage: ./02_apply_migrations.sh
# =====================================================

echo "========================================"
echo "EF Core - Apply Migrations"
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

cd "$INFRASTRUCTURE_DIR" || exit 1

echo "Applying migrations to database..."
echo ""

if dotnet ef database update \
    --startup-project "$WEBAPI_DIR" \
    --context ApplicationDbContext \
    --verbose; then
    echo ""
    echo "========================================"
    echo "Migrations applied successfully!"
    echo "========================================"
else
    echo ""
    echo "========================================"
    echo "Migration application failed!"
    echo "========================================"
    exit 1
fi
