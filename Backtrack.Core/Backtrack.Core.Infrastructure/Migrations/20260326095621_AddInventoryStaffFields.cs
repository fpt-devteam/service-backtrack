using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryStaffFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "handover_staff_id",
                table: "org_inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "receiver_staff_id",
                table: "org_inventories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_org_inventories_handover_staff_id",
                table: "org_inventories",
                column: "handover_staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_org_inventories_receiver_staff_id",
                table: "org_inventories",
                column: "receiver_staff_id");

            migrationBuilder.AddForeignKey(
                name: "fk_org_inventories_handover_staff_id_users_id",
                table: "org_inventories",
                column: "handover_staff_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_org_inventories_receiver_staff_id_users_id",
                table: "org_inventories",
                column: "receiver_staff_id",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_org_inventories_handover_staff_id_users_id",
                table: "org_inventories");

            migrationBuilder.DropForeignKey(
                name: "fk_org_inventories_receiver_staff_id_users_id",
                table: "org_inventories");

            migrationBuilder.DropIndex(
                name: "IX_org_inventories_handover_staff_id",
                table: "org_inventories");

            migrationBuilder.DropIndex(
                name: "IX_org_inventories_receiver_staff_id",
                table: "org_inventories");

            migrationBuilder.DropColumn(
                name: "handover_staff_id",
                table: "org_inventories");

            migrationBuilder.DropColumn(
                name: "receiver_staff_id",
                table: "org_inventories");
        }
    }
}
