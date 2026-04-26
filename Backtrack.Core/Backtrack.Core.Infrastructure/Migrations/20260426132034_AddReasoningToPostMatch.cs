using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReasoningToPostMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reasoning",
                table: "post_matches",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reasoning",
                table: "post_matches");
        }
    }
}
