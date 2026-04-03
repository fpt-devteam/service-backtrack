using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FlatPostItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "item",
                table: "posts");

            migrationBuilder.AddColumn<string>(
                name: "item_additional_details",
                table: "posts",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_brand",
                table: "posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_category",
                table: "posts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "item_color",
                table: "posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_condition",
                table: "posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_distinctive_marks",
                table: "posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_material",
                table: "posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "item_name",
                table: "posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "item_size",
                table: "posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "item_search",
                table: "posts",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('english', coalesce(item_name, '') || ' ' || coalesce(item_category, '') || ' ' || coalesce(item_color, '') || ' ' || coalesce(item_brand, '') || ' ' || coalesce(item_condition, '') || ' ' || coalesce(item_material, '') || ' ' || coalesce(item_size, '') || ' ' || coalesce(item_distinctive_marks, '') || ' ' || coalesce(item_additional_details, ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_item_search",
                table: "posts",
                column: "item_search")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posts_item_search",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_search",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_additional_details",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_brand",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_category",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_color",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_condition",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_distinctive_marks",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_material",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_name",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "item_size",
                table: "posts");

            migrationBuilder.AddColumn<string>(
                name: "item",
                table: "posts",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
