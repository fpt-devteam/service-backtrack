using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMatchingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_post_matches_candidate_post_id",
                table: "post_matches");

            migrationBuilder.DropForeignKey(
                name: "fk_post_matches_source_post_id",
                table: "post_matches");

            migrationBuilder.DropIndex(
                name: "IX_post_matches_candidate_post_id",
                table: "post_matches");

            migrationBuilder.RenameColumn(
                name: "source_post_id",
                table: "post_matches",
                newName: "lost_post_id");

            migrationBuilder.RenameColumn(
                name: "candidate_post_id",
                table: "post_matches",
                newName: "found_post_id");

            migrationBuilder.RenameIndex(
                name: "ix_post_matches_source_candidate",
                table: "post_matches",
                newName: "ix_post_matches_lost_found");

            migrationBuilder.RenameIndex(
                name: "ix_post_matches_by_source",
                table: "post_matches",
                newName: "ix_post_matches_by_lost");

            migrationBuilder.CreateIndex(
                name: "ix_post_matches_by_found",
                table: "post_matches",
                columns: new[] { "found_post_id", "status", "score" });

            migrationBuilder.AddForeignKey(
                name: "fk_post_matches_found_post_id",
                table: "post_matches",
                column: "found_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_post_matches_lost_post_id",
                table: "post_matches",
                column: "lost_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_post_matches_found_post_id",
                table: "post_matches");

            migrationBuilder.DropForeignKey(
                name: "fk_post_matches_lost_post_id",
                table: "post_matches");

            migrationBuilder.DropIndex(
                name: "ix_post_matches_by_found",
                table: "post_matches");

            migrationBuilder.RenameColumn(
                name: "lost_post_id",
                table: "post_matches",
                newName: "source_post_id");

            migrationBuilder.RenameColumn(
                name: "found_post_id",
                table: "post_matches",
                newName: "candidate_post_id");

            migrationBuilder.RenameIndex(
                name: "ix_post_matches_lost_found",
                table: "post_matches",
                newName: "ix_post_matches_source_candidate");

            migrationBuilder.RenameIndex(
                name: "ix_post_matches_by_lost",
                table: "post_matches",
                newName: "ix_post_matches_by_source");

            migrationBuilder.CreateIndex(
                name: "IX_post_matches_candidate_post_id",
                table: "post_matches",
                column: "candidate_post_id");

            migrationBuilder.AddForeignKey(
                name: "fk_post_matches_candidate_post_id",
                table: "post_matches",
                column: "candidate_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_post_matches_source_post_id",
                table: "post_matches",
                column: "source_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
