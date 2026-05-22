using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgLocationFieldsAndEvidenceImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "internal_location",
                table: "posts",
                newName: "organization_storage_location");

            migrationBuilder.AddColumn<string>(
                name: "organization_found_location",
                table: "posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "organization_found_location",
                table: "posts");

            migrationBuilder.RenameColumn(
                name: "organization_storage_location",
                table: "posts",
                newName: "internal_location");
        }
    }
}
