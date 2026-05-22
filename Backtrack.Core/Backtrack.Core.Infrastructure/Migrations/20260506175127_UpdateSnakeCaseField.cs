using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSnakeCaseField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReturnedAt",
                table: "posts",
                newName: "returned_at");

            migrationBuilder.RenameColumn(
                name: "RejectedAt",
                table: "posts",
                newName: "rejected_at");

            migrationBuilder.RenameColumn(
                name: "ExpiredAt",
                table: "posts",
                newName: "expired_at");

            migrationBuilder.RenameColumn(
                name: "DeliveredAt",
                table: "posts",
                newName: "delivered_at");

            migrationBuilder.RenameColumn(
                name: "ArchivedAt",
                table: "posts",
                newName: "archived_at");

            migrationBuilder.RenameColumn(
                name: "RejectedAt",
                table: "c2c_return_reports",
                newName: "rejected_at");

            migrationBuilder.RenameColumn(
                name: "ClosedAt",
                table: "c2c_return_reports",
                newName: "closed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "returned_at",
                table: "posts",
                newName: "ReturnedAt");

            migrationBuilder.RenameColumn(
                name: "rejected_at",
                table: "posts",
                newName: "RejectedAt");

            migrationBuilder.RenameColumn(
                name: "expired_at",
                table: "posts",
                newName: "ExpiredAt");

            migrationBuilder.RenameColumn(
                name: "delivered_at",
                table: "posts",
                newName: "DeliveredAt");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                table: "posts",
                newName: "ArchivedAt");

            migrationBuilder.RenameColumn(
                name: "rejected_at",
                table: "c2c_return_reports",
                newName: "RejectedAt");

            migrationBuilder.RenameColumn(
                name: "closed_at",
                table: "c2c_return_reports",
                newName: "ClosedAt");
        }
    }
}
