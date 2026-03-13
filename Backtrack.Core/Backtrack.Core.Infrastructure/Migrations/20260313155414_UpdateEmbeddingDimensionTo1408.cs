using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmbeddingDimensionTo1408 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posts_content_embedding",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_org_inventories_content_embedding",
                table: "org_inventories");

            migrationBuilder.DropColumn(
                name: "content_embedding",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "content_embedding",
                table: "org_inventories");

            migrationBuilder.AddColumn<Vector>(
                name: "multimodal_embedding",
                table: "posts",
                type: "vector(1408)",
                nullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "multimodal_embedding",
                table: "org_inventories",
                type: "vector(1408)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_multimodal_embedding",
                table: "posts",
                column: "multimodal_embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_org_inventories_multimodal_embedding",
                table: "org_inventories",
                column: "multimodal_embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posts_multimodal_embedding",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_org_inventories_multimodal_embedding",
                table: "org_inventories");

            migrationBuilder.DropColumn(
                name: "multimodal_embedding",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "multimodal_embedding",
                table: "org_inventories");

            migrationBuilder.AddColumn<Vector>(
                name: "content_embedding",
                table: "posts",
                type: "vector(768)",
                nullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "content_embedding",
                table: "org_inventories",
                type: "vector(768)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_content_embedding",
                table: "posts",
                column: "content_embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_org_inventories_content_embedding",
                table: "org_inventories",
                column: "content_embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }
    }
}
