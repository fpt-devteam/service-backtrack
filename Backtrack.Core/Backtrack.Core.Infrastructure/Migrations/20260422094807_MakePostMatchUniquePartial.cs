using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakePostMatchUniquePartial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_post_matches_lost_found",
                table: "post_matches");

            migrationBuilder.CreateIndex(
                name: "ix_post_matches_lost_found",
                table: "post_matches",
                columns: ["lost_post_id", "found_post_id"],
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_post_matches_lost_found",
                table: "post_matches");

            migrationBuilder.CreateIndex(
                name: "ix_post_matches_lost_found",
                table: "post_matches",
                columns: ["lost_post_id", "found_post_id"],
                unique: true);
        }
    }
}
