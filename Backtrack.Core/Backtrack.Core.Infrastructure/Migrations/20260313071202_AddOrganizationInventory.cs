using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "org_inventories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    logged_by_id = table.Column<string>(type: "text", nullable: false),
                    item_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    distinctive_marks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    image_urls = table.Column<string[]>(type: "text[]", nullable: false),
                    storage_location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    content_embedding = table.Column<Vector>(type: "vector(768)", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    logged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_inventories", x => x.id);
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
                });

            migrationBuilder.CreateIndex(
                name: "ix_org_inventories_content_embedding",
                table: "org_inventories",
                column: "content_embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_org_inventories_logged_by_id",
                table: "org_inventories",
                column: "logged_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_org_inventories_org_id",
                table: "org_inventories",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "org_inventories");
        }
    }
}
