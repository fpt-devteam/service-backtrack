using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnswerTypeAndImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "answer_text",
                table: "answers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<List<string>>(
                name: "image_urls",
                table: "answers",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "answers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_urls",
                table: "answers");

            migrationBuilder.DropColumn(
                name: "type",
                table: "answers");

            migrationBuilder.AlterColumn<string>(
                name: "answer_text",
                table: "answers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
