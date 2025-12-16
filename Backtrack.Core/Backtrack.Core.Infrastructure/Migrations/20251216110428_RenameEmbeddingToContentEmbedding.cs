using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmbeddingToContentEmbedding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "embedding",
                table: "posts",
                newName: "content_embedding");

            migrationBuilder.RenameIndex(
                name: "ix_posts_embedding",
                table: "posts",
                newName: "ix_posts_content_embedding");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "content_embedding",
                table: "posts",
                newName: "embedding");

            migrationBuilder.RenameIndex(
                name: "ix_posts_content_embedding",
                table: "posts",
                newName: "ix_posts_embedding");
        }
    }
}
