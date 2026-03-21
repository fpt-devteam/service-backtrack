using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTextImageEmbeddingsAndCriteriaAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Vector>(
                name: "image_embedding",
                table: "posts",
                type: "vector(1536)",
                nullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "text_embedding",
                table: "posts",
                type: "vector(1536)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "matching_level",
                table: "post_matches",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "criteria_json",
                table: "post_matches",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_assessed",
                table: "post_matches",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_embedding",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "text_embedding",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "criteria_json",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "is_assessed",
                table: "post_matches");

            migrationBuilder.AlterColumn<int>(
                name: "matching_level",
                table: "post_matches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
