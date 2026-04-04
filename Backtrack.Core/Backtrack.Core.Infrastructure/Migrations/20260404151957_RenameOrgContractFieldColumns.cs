using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backtrack.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrgContractFieldColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "required_finder_contact_fields",
                table: "organizations",
                newName: "required_finder_contract_fields");

            migrationBuilder.RenameColumn(
                name: "required_owner_form_fields",
                table: "organizations",
                newName: "required_owner_contract_fields");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "required_finder_contract_fields",
                table: "organizations",
                newName: "required_finder_contact_fields");

            migrationBuilder.RenameColumn(
                name: "required_owner_contract_fields",
                table: "organizations",
                newName: "required_owner_form_fields");
        }
    }
}
