using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeRequiredFieldInC2CReturnReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_c2c_return_reports_finder_post_id",
                table: "c2c_return_reports");

            migrationBuilder.DropForeignKey(
                name: "fk_c2c_return_reports_owner_post_id",
                table: "c2c_return_reports");

            migrationBuilder.DropColumn(
                name: "activated_by_id",
                table: "c2c_return_reports");

            migrationBuilder.AlterColumn<Guid>(
                name: "owner_post_id",
                table: "c2c_return_reports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "finder_post_id",
                table: "c2c_return_reports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "delivered_at",
                table: "c2c_return_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "evidence_image_urls",
                table: "c2c_return_reports",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_c2c_return_reports_finder_post_id",
                table: "c2c_return_reports",
                column: "finder_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_c2c_return_reports_owner_post_id",
                table: "c2c_return_reports",
                column: "owner_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_c2c_return_reports_finder_post_id",
                table: "c2c_return_reports");

            migrationBuilder.DropForeignKey(
                name: "fk_c2c_return_reports_owner_post_id",
                table: "c2c_return_reports");

            migrationBuilder.DropColumn(
                name: "delivered_at",
                table: "c2c_return_reports");

            migrationBuilder.DropColumn(
                name: "evidence_image_urls",
                table: "c2c_return_reports");

            migrationBuilder.AlterColumn<Guid>(
                name: "owner_post_id",
                table: "c2c_return_reports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "finder_post_id",
                table: "c2c_return_reports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "activated_by_id",
                table: "c2c_return_reports",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_c2c_return_reports_finder_post_id",
                table: "c2c_return_reports",
                column: "finder_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_c2c_return_reports_owner_post_id",
                table: "c2c_return_reports",
                column: "owner_post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
