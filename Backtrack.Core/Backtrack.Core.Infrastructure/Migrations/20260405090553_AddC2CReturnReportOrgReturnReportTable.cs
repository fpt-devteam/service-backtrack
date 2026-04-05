using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddC2CReturnReportOrgReturnReportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "org_handovers");

            migrationBuilder.DropTable(
                name: "P2PHandovers");

            migrationBuilder.DropTable(
                name: "Handovers");

            migrationBuilder.CreateTable(
                name: "c2c_return_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    finder_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    owner_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finder_post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_c2c_return_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_c2c_return_reports_finder_id",
                        column: x => x.finder_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_c2c_return_reports_finder_post_id",
                        column: x => x.finder_post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_c2c_return_reports_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_c2c_return_reports_owner_post_id",
                        column: x => x.owner_post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "org_return_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_required_info = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_return_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_org_return_reports_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_org_return_reports_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_org_return_reports_staff_id",
                        column: x => x.staff_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_c2c_return_reports_finder_id",
                table: "c2c_return_reports",
                column: "finder_id");

            migrationBuilder.CreateIndex(
                name: "ix_c2c_return_reports_finder_post_id",
                table: "c2c_return_reports",
                column: "finder_post_id");

            migrationBuilder.CreateIndex(
                name: "ix_c2c_return_reports_owner_id",
                table: "c2c_return_reports",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_c2c_return_reports_owner_post_id",
                table: "c2c_return_reports",
                column: "owner_post_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_return_reports_org_id",
                table: "org_return_reports",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_return_reports_post_id",
                table: "org_return_reports",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_return_reports_staff_id",
                table: "org_return_reports",
                column: "staff_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "c2c_return_reports");

            migrationBuilder.DropTable(
                name: "org_return_reports");

            migrationBuilder.CreateTable(
                name: "Handovers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Handovers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "org_handovers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    finder_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    finder_post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    owner_confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    owner_form_data = table.Column<string>(type: "jsonb", nullable: true),
                    owner_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    staff_confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_handovers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_org_handovers_Handovers_Id",
                        column: x => x.Id,
                        principalTable: "Handovers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_org_handovers_finder_id",
                        column: x => x.finder_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_org_handovers_finder_post_id",
                        column: x => x.finder_post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_org_handovers_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_org_handovers_staff_id",
                        column: x => x.staff_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "P2PHandovers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinderId = table.Column<string>(type: "character varying(255)", nullable: false),
                    FinderPostId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerId = table.Column<string>(type: "character varying(255)", nullable: false),
                    OwnerPostId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_P2PHandovers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_P2PHandovers_Handovers_Id",
                        column: x => x.Id,
                        principalTable: "Handovers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_P2PHandovers_posts_FinderPostId",
                        column: x => x.FinderPostId,
                        principalTable: "posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_P2PHandovers_posts_OwnerPostId",
                        column: x => x.OwnerPostId,
                        principalTable: "posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_P2PHandovers_users_FinderId",
                        column: x => x.FinderId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_P2PHandovers_users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_org_handovers_finder_id",
                table: "org_handovers",
                column: "finder_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_handovers_finder_post_id",
                table: "org_handovers",
                column: "finder_post_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_handovers_org_id",
                table: "org_handovers",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_handovers_staff_id",
                table: "org_handovers",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_P2PHandovers_FinderId",
                table: "P2PHandovers",
                column: "FinderId");

            migrationBuilder.CreateIndex(
                name: "IX_P2PHandovers_FinderPostId",
                table: "P2PHandovers",
                column: "FinderPostId");

            migrationBuilder.CreateIndex(
                name: "IX_P2PHandovers_OwnerId",
                table: "P2PHandovers",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_P2PHandovers_OwnerPostId",
                table: "P2PHandovers",
                column: "OwnerPostId");
        }
    }
}
