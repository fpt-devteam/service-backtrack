#!/bin/sh
# =====================================================
# Script: Truncate App Tables (Exclude Extension-owned)
# Description: Truncates all user-created tables in public schema,
#              excluding tables owned by any extension (PostGIS, etc.)
#              and excluding __EFMigrationsHistory
# Usage: ./03_truncate_all_data.sh
# =====================================================

# Connection settings - change if needed
DB_HOST="localhost"
DB_PORT="5432"
DB_NAME="backtrack-core-db"
DB_USER="postgres"
DB_PASSWORD="postgres123"

printf 'WARNING: This will TRUNCATE ALL DATA from domain tables in database "%s" (public schema, excluding extension-owned tables)\n' "$DB_NAME"
printf 'Continue? (yes/no): '
read -r confirmation

if [ "$confirmation" != "yes" ]; then
    echo "Cancelled"
    exit 0
fi

echo "Truncating data from domain tables..."

TRUNCATE_SQL='DO $$
DECLARE
  r RECORD;
  table_list TEXT := '"''"';
BEGIN
  RAISE NOTICE '"'Collecting tables to truncate...'"';

  FOR r IN (
    SELECT c.relname AS tablename
    FROM pg_class c
    JOIN pg_namespace n ON n.oid = c.relnamespace
    LEFT JOIN pg_depend d
      ON d.objid = c.oid
     AND d.deptype = '"'e'"'
    WHERE n.nspname = '"'public'"'
      AND c.relkind = '"'r'"'
      AND d.objid IS NULL
      AND c.relname <> '"'__EFMigrationsHistory'"'
    ORDER BY c.relname
  ) LOOP
    RAISE NOTICE '"'Will truncate: public.%'"', r.tablename;

    table_list := table_list
      || CASE WHEN table_list <> '"''"' THEN '"', '"' ELSE '"''"' END
      || format('"'public.%I'"', r.tablename);
  END LOOP;

  IF table_list <> '"''"' THEN
    RAISE NOTICE '"'Executing: TRUNCATE TABLE %'"', table_list;
    EXECUTE '"'TRUNCATE TABLE '"' || table_list || '"' RESTART IDENTITY CASCADE'"';
    RAISE NOTICE '"'Done truncating domain tables.'"';
  ELSE
    RAISE NOTICE '"'No domain tables found to truncate (or all tables are extension-owned / excluded).'"';
  END IF;
END $$;'

# Write SQL to a temp file
TEMP_FILE="$(mktemp /tmp/truncate_domain_tables.XXXXXX.sql)"
printf '%s\n' "$TRUNCATE_SQL" > "$TEMP_FILE"

if command -v psql > /dev/null 2>&1; then
    PGPASSWORD="$DB_PASSWORD" psql \
        -h "$DB_HOST" \
        -p "$DB_PORT" \
        -U "$DB_USER" \
        -d "$DB_NAME" \
        -v ON_ERROR_STOP=1 \
        -f "$TEMP_FILE"
    exit_code=$?

    rm -f "$TEMP_FILE"

    if [ "$exit_code" -eq 0 ]; then
        echo "Done!"
        exit 0
    else
        echo "psql exited with code $exit_code"
        exit "$exit_code"
    fi
else
    SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
    SQL_FILE="$SCRIPT_DIR/truncate_domain_tables.sql"
    cp "$TEMP_FILE" "$SQL_FILE"
    rm -f "$TEMP_FILE"

    echo ""
    echo "psql not found!"
    echo "SQL file created: $SQL_FILE"
    echo ""
    echo "Option 1: Install PostgreSQL client and run:"
    echo "  psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -v ON_ERROR_STOP=1 -f \"$SQL_FILE\""
    echo ""
    echo "Option 2: Run the SQL manually in pgAdmin or your database tool"
    echo ""
fi
