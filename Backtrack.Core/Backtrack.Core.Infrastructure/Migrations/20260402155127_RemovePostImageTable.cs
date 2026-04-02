using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePostImageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_images");

            migrationBuilder.DropColumn(
                name: "description",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_name",
                table: "posts");

            migrationBuilder.AddColumn<List<string>>(
                name: "image_urls",
                table: "posts",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "item",
                table: "posts",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_urls",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item",
                table: "posts");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "posts",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "item_name",
                table: "posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "post_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base64_data = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_images_post_id_posts_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_post_images_post_id",
                table: "post_images",
                column: "post_id");
        }
    }
}
