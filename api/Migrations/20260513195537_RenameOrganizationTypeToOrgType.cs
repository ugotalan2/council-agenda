using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CouncilAgendaApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrganizationTypeToOrgType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrganizationType",
                table: "Organizations",
                newName: "OrgType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrgType",
                table: "Organizations",
                newName: "OrganizationType");
        }
    }
}
