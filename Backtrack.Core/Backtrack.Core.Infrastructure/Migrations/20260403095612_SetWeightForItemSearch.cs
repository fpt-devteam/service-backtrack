using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetWeightForItemSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "item_search",
                table: "posts",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "setweight(to_tsvector('english', coalesce(item_name, '')),               'A') || setweight(to_tsvector('english', coalesce(item_category, '') || ' ' || coalesce(item_brand, '')), 'B') || setweight(to_tsvector('english', coalesce(item_color, '')     || ' ' || coalesce(item_condition, '') || ' ' || coalesce(item_material, '') || ' ' || coalesce(item_size, '') || ' ' || coalesce(item_distinctive_marks, '')), 'C') || setweight(to_tsvector('english', coalesce(item_additional_details, '')), 'D')",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true,
                oldComputedColumnSql: "to_tsvector('english', coalesce(item_name, '') || ' ' || coalesce(item_category, '') || ' ' || coalesce(item_color, '') || ' ' || coalesce(item_brand, '') || ' ' || coalesce(item_condition, '') || ' ' || coalesce(item_material, '') || ' ' || coalesce(item_size, '') || ' ' || coalesce(item_distinctive_marks, '') || ' ' || coalesce(item_additional_details, ''))",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "item_search",
                table: "posts",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('english', coalesce(item_name, '') || ' ' || coalesce(item_category, '') || ' ' || coalesce(item_color, '') || ' ' || coalesce(item_brand, '') || ' ' || coalesce(item_condition, '') || ' ' || coalesce(item_material, '') || ' ' || coalesce(item_size, '') || ' ' || coalesce(item_distinctive_marks, '') || ' ' || coalesce(item_additional_details, ''))",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true,
                oldComputedColumnSql: "setweight(to_tsvector('english', coalesce(item_name, '')),               'A') || setweight(to_tsvector('english', coalesce(item_category, '') || ' ' || coalesce(item_brand, '')), 'B') || setweight(to_tsvector('english', coalesce(item_color, '')     || ' ' || coalesce(item_condition, '') || ' ' || coalesce(item_material, '') || ' ' || coalesce(item_size, '') || ' ' || coalesce(item_distinctive_marks, '')), 'C') || setweight(to_tsvector('english', coalesce(item_additional_details, '')), 'D')",
                oldStored: true);
        }
    }
}
