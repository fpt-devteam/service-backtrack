using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostIdToQnATable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "post_id",
                table: "qna",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_qna_post_id",
                table: "qna",
                column: "post_id");

            migrationBuilder.AddForeignKey(
                name: "fk_qna_post_id_posts_id",
                table: "qna",
                column: "post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_qna_post_id_posts_id",
                table: "qna");

            migrationBuilder.DropIndex(
                name: "ix_qna_post_id",
                table: "qna");

            migrationBuilder.DropColumn(
                name: "post_id",
                table: "qna");
        }
    }
}
