using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostMatchesTableWithUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "post_matches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lost_post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    found_post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_score = table.Column<float>(type: "real", nullable: false),
                    location_score = table.Column<float>(type: "real", nullable: false),
                    description_score = table.Column<float>(type: "real", nullable: false),
                    distance_meters = table.Column<float>(type: "real", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_matches", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_matches_found_post_id",
                        column: x => x.found_post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_matches_lost_post_id",
                        column: x => x.lost_post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_post_matches_found_post_id",
                table: "post_matches",
                column: "found_post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_matches_lost_post_id",
                table: "post_matches",
                column: "lost_post_id");

            migrationBuilder.CreateIndex(
                name: "ux_post_matches_found_lost_active",
                table: "post_matches",
                columns: new[] { "found_post_id", "lost_post_id" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_matches");
        }
    }
}
