using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentEmbeddingStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add column as nullable first
            migrationBuilder.AddColumn<int>(
                name: "content_embedding_status",
                table: "posts",
                type: "integer",
                nullable: true);

            // Set existing posts to Pending (0) status
            migrationBuilder.Sql(@"
                UPDATE posts
                SET content_embedding_status = 0
                WHERE content_embedding_status IS NULL;
            ");

            // Make column non-nullable with default value
            migrationBuilder.AlterColumn<int>(
                name: "content_embedding_status",
                table: "posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_embedding_status",
                table: "posts");
        }
    }
}
