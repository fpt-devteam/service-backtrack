using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostMatchEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "post_matches",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending");

            migrationBuilder.AddColumn<double>(
                name: "DistanceMeters",
                table: "post_matches",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TimeGapDays",
                table: "post_matches",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceMeters",
                table: "post_matches");

            migrationBuilder.DropColumn(
                name: "TimeGapDays",
                table: "post_matches");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "post_matches",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
