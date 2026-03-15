using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLocationInOrganizationAndPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "address",
                table: "organizations",
                newName: "external_place_id");

            migrationBuilder.AlterColumn<string>(
                name: "display_address",
                table: "posts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "display_address",
                table: "organizations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Point>(
                name: "location",
                table: "organizations",
                type: "geography(point, 4326)",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_posts_organization_id",
                table: "posts",
                column: "organization_id");

            migrationBuilder.AddForeignKey(
                name: "fk_posts_organization_id_organizations_id",
                table: "posts",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_posts_organization_id_organizations_id",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "IX_posts_organization_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "display_address",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "location",
                table: "organizations");

            migrationBuilder.RenameColumn(
                name: "external_place_id",
                table: "organizations",
                newName: "address");

            migrationBuilder.AlterColumn<string>(
                name: "display_address",
                table: "posts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);
        }
    }
}
