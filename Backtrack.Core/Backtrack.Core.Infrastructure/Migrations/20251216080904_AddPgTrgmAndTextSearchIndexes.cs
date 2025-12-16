using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPgTrgmAndTextSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable pg_trgm extension for trigram-based text search
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // Create GIN index on item_name for fast text search
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS ix_posts_item_name_gin ON posts USING gin (item_name gin_trgm_ops);");

            // Create GIN index on description for fast text search
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS ix_posts_description_gin ON posts USING gin (description gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop GIN indexes
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_posts_item_name_gin;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_posts_description_gin;");

            // Note: We don't drop the pg_trgm extension as other tables might be using it
        }
    }
}
