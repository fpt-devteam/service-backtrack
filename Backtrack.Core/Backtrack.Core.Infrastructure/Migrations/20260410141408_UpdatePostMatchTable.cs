using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostMatchTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "criteria_json",
                table: "post_matches");

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

            migrationBuilder.AlterColumn<string>(
                name: "assessment_summary",
                table: "post_matches",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "assessment_summary",
                table: "post_matches",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "criteria_json",
                table: "post_matches",
                type: "jsonb",
                nullable: true);

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
    }
}
