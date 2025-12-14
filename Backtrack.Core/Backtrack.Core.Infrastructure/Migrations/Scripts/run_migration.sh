#!/bin/bash

# =====================================================
# Database Migration Helper Script
# Description: Interactive script to run migrations
# Usage: ./run_migration.sh
# =====================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Function to print colored output
print_info() {
    echo -e "${BLUE}ℹ ${NC}$1"
}

print_success() {
    echo -e "${GREEN}✓ ${NC}$1"
}

print_warning() {
    echo -e "${YELLOW}⚠ ${NC}$1"
}

print_error() {
    echo -e "${RED}✗ ${NC}$1"
}

print_header() {
    echo ""
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo ""
}

# Function to check if psql is installed
check_psql() {
    if ! command -v psql &> /dev/null; then
        print_error "psql is not installed or not in PATH"
        print_info "Install PostgreSQL client tools to continue"
        exit 1
    fi
    print_success "psql found: $(psql --version)"
}

# Function to get database connection string
get_connection_string() {
    print_header "Database Connection"

    # Check for .env file or appsettings
    if [ -f "$SCRIPT_DIR/../../../Backtrack.Core.WebApi/appsettings.Development.json" ]; then
        print_info "Found appsettings.Development.json"
        # You could parse the connection string here
    fi

    read -p "Database Host [localhost]: " DB_HOST
    DB_HOST=${DB_HOST:-localhost}

    read -p "Database Port [5432]: " DB_PORT
    DB_PORT=${DB_PORT:-5432}

    read -p "Database Name [backtrack_db]: " DB_NAME
    DB_NAME=${DB_NAME:-backtrack_db}

    read -p "Database User [postgres]: " DB_USER
    DB_USER=${DB_USER:-postgres}

    read -sp "Database Password: " DB_PASSWORD
    echo ""

    # Build connection string
    if [ -z "$DB_PASSWORD" ]; then
        CONN_STRING="postgresql://${DB_USER}@${DB_HOST}:${DB_PORT}/${DB_NAME}"
    else
        CONN_STRING="postgresql://${DB_USER}:${DB_PASSWORD}@${DB_HOST}:${DB_PORT}/${DB_NAME}"
    fi
}

# Function to test database connection
test_connection() {
    print_info "Testing database connection..."
    if psql "$CONN_STRING" -c "SELECT version();" &> /dev/null; then
        print_success "Database connection successful!"
        return 0
    else
        print_error "Failed to connect to database"
        print_info "Please check your connection details"
        return 1
    fi
}

# Function to run migration script
run_script() {
    local script_file=$1
    local script_path="$SCRIPT_DIR/$script_file"

    if [ ! -f "$script_path" ]; then
        print_error "Script file not found: $script_file"
        return 1
    fi

    print_info "Running: $script_file"
    echo ""

    if psql "$CONN_STRING" -v ON_ERROR_STOP=1 -a -f "$script_path"; then
        echo ""
        print_success "Script executed successfully!"
        return 0
    else
        echo ""
        print_error "Script execution failed!"
        return 1
    fi
}

# Function to create backup
create_backup() {
    local backup_dir="$SCRIPT_DIR/../Backups"
    mkdir -p "$backup_dir"

    local timestamp=$(date +%Y%m%d_%H%M%S)
    local backup_file="$backup_dir/backup_${DB_NAME}_${timestamp}.sql"

    print_info "Creating backup: $backup_file"

    if pg_dump "$CONN_STRING" > "$backup_file"; then
        print_success "Backup created successfully!"
        echo "Backup location: $backup_file"
        return 0
    else
        print_error "Backup failed!"
        return 1
    fi
}

# Main menu
show_menu() {
    print_header "Database Migration Tool"
    echo "1) Create Users Table (01_create_users_table.sql)"
    echo "2) Rollback Last Migration (02_rollback_last_migration.sql)"
    echo "3) Reset & Migrate (03_reset_and_migrate.sql)"
    echo "4) Create Database Backup"
    echo "5) Change Connection Settings"
    echo "6) Test Database Connection"
    echo "0) Exit"
    echo ""
    read -p "Select an option: " choice

    case $choice in
        1)
            print_warning "This will create the users table"
            read -p "Continue? (y/n): " confirm
            if [ "$confirm" = "y" ]; then
                run_script "01_create_users_table.sql"
            fi
            ;;
        2)
            print_warning "⚠️  WARNING: This will DELETE all user data!"
            read -p "Are you sure? (yes/no): " confirm
            if [ "$confirm" = "yes" ]; then
                run_script "02_rollback_last_migration.sql"
            else
                print_info "Rollback cancelled"
            fi
            ;;
        3)
            print_warning "⚠️  DANGER: This will DELETE ALL data in the database!"
            print_warning "This action cannot be undone!"
            read -p "Type 'DELETE EVERYTHING' to confirm: " confirm
            if [ "$confirm" = "DELETE EVERYTHING" ]; then
                print_info "Creating backup before reset..."
                if create_backup; then
                    run_script "03_reset_and_migrate.sql"
                else
                    print_error "Backup failed. Reset cancelled for safety."
                fi
            else
                print_info "Reset cancelled"
            fi
            ;;
        4)
            create_backup
            ;;
        5)
            get_connection_string
            test_connection
            ;;
        6)
            test_connection
            ;;
        0)
            print_info "Goodbye!"
            exit 0
            ;;
        *)
            print_error "Invalid option"
            ;;
    esac
}

# Main script
main() {
    clear
    check_psql
    get_connection_string

    if ! test_connection; then
        exit 1
    fi

    while true; do
        echo ""
        show_menu
        echo ""
        read -p "Press Enter to continue..."
        clear
    done
}

# Run main
main
