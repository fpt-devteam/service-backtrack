using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidatePostEmbeddingField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posts_multimodal_embedding",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "image_embedding",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "multimodal_embedding",
                table: "posts");

            migrationBuilder.RenameColumn(
                name: "text_embedding",
                table: "posts",
                newName: "embedding");

            migrationBuilder.RenameColumn(
                name: "content_embedding_status",
                table: "posts",
                newName: "embedding_status");

            migrationBuilder.CreateIndex(
                name: "ix_posts_embedding",
                table: "posts",
                column: "embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posts_embedding",
                table: "posts");

            migrationBuilder.RenameColumn(
                name: "embedding_status",
                table: "posts",
                newName: "content_embedding_status");

            migrationBuilder.RenameColumn(
                name: "embedding",
                table: "posts",
                newName: "text_embedding");

            migrationBuilder.AddColumn<Vector>(
                name: "image_embedding",
                table: "posts",
                type: "vector(1536)",
                nullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "multimodal_embedding",
                table: "posts",
                type: "vector(1536)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_multimodal_embedding",
                table: "posts",
                column: "multimodal_embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }
    }
}
