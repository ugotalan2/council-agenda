using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CouncilAgendaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgPositionsAndRefactorRotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgendaAttendees_Members_MemberId",
                table: "AgendaAttendees");

            migrationBuilder.DropForeignKey(
                name: "FK_RotationLogs_Members_MemberId",
                table: "RotationLogs");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "RotationLogs",
                newName: "PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_RotationLogs_MemberId",
                table: "RotationLogs",
                newName: "IX_RotationLogs_PositionId");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "AgendaAttendees",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "GuestLabel",
                table: "AgendaAttendees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "AgendaAttendees",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "OrgPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    OrgTypeScope = table.Column<string>(type: "text", nullable: false),
                    IsStanding = table.Column<bool>(type: "boolean", nullable: false),
                    IsGuestDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsRotationEligible = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrgPositions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberPositions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberPositions_OrgPositions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "OrgPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RotationLogs_OrganizationId",
                table: "RotationLogs",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaAttendees_PositionId",
                table: "AgendaAttendees",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberPositions_MemberId",
                table: "MemberPositions",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberPositions_PositionId",
                table: "MemberPositions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgPositions_OrganizationId",
                table: "OrgPositions",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgendaAttendees_Members_MemberId",
                table: "AgendaAttendees",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AgendaAttendees_OrgPositions_PositionId",
                table: "AgendaAttendees",
                column: "PositionId",
                principalTable: "OrgPositions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RotationLogs_OrgPositions_PositionId",
                table: "RotationLogs",
                column: "PositionId",
                principalTable: "OrgPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RotationLogs_Organizations_OrganizationId",
                table: "RotationLogs",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgendaAttendees_Members_MemberId",
                table: "AgendaAttendees");

            migrationBuilder.DropForeignKey(
                name: "FK_AgendaAttendees_OrgPositions_PositionId",
                table: "AgendaAttendees");

            migrationBuilder.DropForeignKey(
                name: "FK_RotationLogs_OrgPositions_PositionId",
                table: "RotationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RotationLogs_Organizations_OrganizationId",
                table: "RotationLogs");

            migrationBuilder.DropTable(
                name: "MemberPositions");

            migrationBuilder.DropTable(
                name: "OrgPositions");

            migrationBuilder.DropIndex(
                name: "IX_RotationLogs_OrganizationId",
                table: "RotationLogs");

            migrationBuilder.DropIndex(
                name: "IX_AgendaAttendees_PositionId",
                table: "AgendaAttendees");

            migrationBuilder.DropColumn(
                name: "GuestLabel",
                table: "AgendaAttendees");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "AgendaAttendees");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "RotationLogs",
                newName: "MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_RotationLogs_PositionId",
                table: "RotationLogs",
                newName: "IX_RotationLogs_MemberId");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "AgendaAttendees",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AgendaAttendees_Members_MemberId",
                table: "AgendaAttendees",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RotationLogs_Members_MemberId",
                table: "RotationLogs",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
