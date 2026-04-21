using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "post_title",
                table: "posts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "post_title",
                table: "posts");
        }
    }
}
