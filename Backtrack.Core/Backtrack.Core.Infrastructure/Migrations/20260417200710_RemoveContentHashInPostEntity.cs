using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContentHashInPostEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_hash",
                table: "posts");

            migrationBuilder.AddColumn<string>(
                name: "content_hash",
                table: "post_personal_belonging_details",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "content_hash",
                table: "post_other_details",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "content_hash",
                table: "post_electronic_details",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "content_hash",
                table: "post_card_details",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_hash",
                table: "post_personal_belonging_details");

            migrationBuilder.DropColumn(
                name: "content_hash",
                table: "post_other_details");

            migrationBuilder.DropColumn(
                name: "content_hash",
                table: "post_electronic_details");

            migrationBuilder.DropColumn(
                name: "content_hash",
                table: "post_card_details");

            migrationBuilder.AddColumn<string>(
                name: "content_hash",
                table: "posts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }
    }
}
