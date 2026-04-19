using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalNoteForCardPostTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "notes",
                table: "post_other_details",
                newName: "additional_details");

            migrationBuilder.AddColumn<string>(
                name: "additional_details",
                table: "post_card_details",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "additional_details",
                table: "post_card_details");

            migrationBuilder.RenameColumn(
                name: "additional_details",
                table: "post_other_details",
                newName: "notes");
        }
    }
}
