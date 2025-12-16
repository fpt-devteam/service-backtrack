using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveColorMaterialBrandArrays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "brands",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "colors",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "material",
                table: "posts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "brands",
                table: "posts",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "colors",
                table: "posts",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "material",
                table: "posts",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }
    }
}
