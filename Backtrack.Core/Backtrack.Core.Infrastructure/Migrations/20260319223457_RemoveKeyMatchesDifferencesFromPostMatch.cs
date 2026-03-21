using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKeyMatchesDifferencesFromPostMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "key_differences",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "key_matches",
                table: "post_matches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "key_differences",
                table: "post_matches",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<string[]>(
                name: "key_matches",
                table: "post_matches",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'");
        }
    }
}
