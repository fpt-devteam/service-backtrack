using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OwerInfoFieldInOrgReturnReportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "owner_required_info",
                table: "org_return_reports");

            migrationBuilder.AddColumn<string>(
                name: "owner_info",
                table: "org_return_reports",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "owner_info",
                table: "org_return_reports");

            migrationBuilder.AddColumn<string>(
                name: "owner_required_info",
                table: "org_return_reports",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
