using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgContractFieldToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "required_finder_contact_fields",
                table: "organizations",
                newName: "required_owner_contract_fields");

            migrationBuilder.AddColumn<string[]>(
                name: "required_finder_contract_fields",
                table: "organizations",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.CreateTable(
                name: "Handovers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    finder_post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    owner_form_data = table.Column<string>(type: "jsonb", nullable: true),
                    staff_confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    owner_confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    OwnerId = table.Column<string>(type: "character varying(255)", nullable: false),
                    FinderPostId = table.Column<Guid>(type: "uuid", nullable: true),
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "org_handovers");

            migrationBuilder.DropTable(
                name: "P2PHandovers");

            migrationBuilder.DropTable(
                name: "Handovers");

            migrationBuilder.DropColumn(
                name: "required_finder_contract_fields",
                table: "organizations");

            migrationBuilder.RenameColumn(
                name: "required_owner_contract_fields",
                table: "organizations",
                newName: "required_finder_contact_fields");
        }
    }
}
