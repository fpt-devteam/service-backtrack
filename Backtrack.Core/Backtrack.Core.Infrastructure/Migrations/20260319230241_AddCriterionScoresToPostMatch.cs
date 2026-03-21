using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCriterionScoresToPostMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "description_score",
                table: "post_matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "location_score",
                table: "post_matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "time_window_score",
                table: "post_matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "visual_score",
                table: "post_matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description_score",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "location_score",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "time_window_score",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "visual_score",
                table: "post_matches");
        }
    }
}
