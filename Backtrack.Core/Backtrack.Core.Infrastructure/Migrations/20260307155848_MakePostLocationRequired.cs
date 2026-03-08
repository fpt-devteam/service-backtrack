using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakePostLocationRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Point>(
                name: "location",
                table: "posts",
                type: "geography(point, 4326)",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "geography(point, 4326)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Point>(
                name: "location",
                table: "posts",
                type: "geography(point, 4326)",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geography(point, 4326)");
        }
    }
}
