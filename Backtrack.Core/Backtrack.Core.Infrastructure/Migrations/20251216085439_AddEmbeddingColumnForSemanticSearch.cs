using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingColumnForSemanticSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable pgvector extension for vector similarity search
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

            migrationBuilder.AddColumn<Vector>(
                name: "embedding",
                table: "posts",
                type: "vector(768)",
                nullable: true);

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

            migrationBuilder.DropColumn(
                name: "embedding",
                table: "posts");
        }
    }
}
