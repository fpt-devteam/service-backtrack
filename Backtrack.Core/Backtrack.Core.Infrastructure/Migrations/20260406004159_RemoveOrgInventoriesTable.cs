using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrgInventoriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "finder_contacts");

            migrationBuilder.DropTable(
                name: "org_inventories");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "posts",
                type: "text",
                nullable: false,
                defaultValue: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "posts");

            migrationBuilder.CreateTable(
                name: "org_inventories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    handover_staff_id = table.Column<string>(type: "text", nullable: true),
                    logged_by_id = table.Column<string>(type: "text", nullable: false),
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_staff_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    distinctive_marks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    image_urls = table.Column<string[]>(type: "text[]", nullable: false),
                    item_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    logged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    multimodal_embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    storage_location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_inventories", x => x.id);
                    table.ForeignKey(
                        name: "fk_org_inventories_handover_staff_id_users_id",
                        column: x => x.handover_staff_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_org_inventories_logged_by_id_users_id",
                        column: x => x.logged_by_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_org_inventories_org_id_organizations_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_org_inventories_receiver_staff_id_users_id",
                        column: x => x.receiver_staff_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "finder_contacts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    national_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    org_member_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_finder_contacts", x => x.id);
                    table.ForeignKey(
                        name: "fk_finder_contacts_inventory_id_org_inventories_id",
                        column: x => x.inventory_id,
                        principalTable: "org_inventories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_finder_contacts_inventory_id",
                table: "finder_contacts",
                column: "inventory_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_org_inventories_handover_staff_id",
                table: "org_inventories",
                column: "handover_staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_inventories_logged_by_id",
                table: "org_inventories",
                column: "logged_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_inventories_multimodal_embedding",
                table: "org_inventories",
                column: "multimodal_embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_org_inventories_org_id",
                table: "org_inventories",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_org_inventories_receiver_staff_id",
                table: "org_inventories",
                column: "receiver_staff_id");
        }
    }
}
