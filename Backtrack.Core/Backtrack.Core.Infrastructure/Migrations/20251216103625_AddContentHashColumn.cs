using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentHashColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add column as nullable first
            migrationBuilder.AddColumn<string>(
                name: "content_hash",
                table: "posts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            // Generate hash for existing posts
            migrationBuilder.Sql(@"
                UPDATE posts
                SET content_hash = lower(encode(sha256((item_name || E'\n' || description)::bytea), 'hex'))
                WHERE content_hash IS NULL;
            ");

            // Make column non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "content_hash",
                table: "posts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_hash",
                table: "posts");
        }
    }
}
