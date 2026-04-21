using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemNameFieldToPostTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "item_identifier",
                table: "post_other_details",
                newName: "item_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "item_name",
                table: "post_other_details",
                newName: "item_identifier");
        }
    }
}
