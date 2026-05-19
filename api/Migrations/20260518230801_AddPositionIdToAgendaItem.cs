using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CouncilAgendaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionIdToAgendaItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "AgendaItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PositionId",
                table: "AgendaAttendees",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItems_PositionId",
                table: "AgendaItems",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgendaItems_OrgPositions_PositionId",
                table: "AgendaItems",
                column: "PositionId",
                principalTable: "OrgPositions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgendaItems_OrgPositions_PositionId",
                table: "AgendaItems");

            migrationBuilder.DropIndex(
                name: "IX_AgendaItems_PositionId",
                table: "AgendaItems");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "AgendaItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "PositionId",
                table: "AgendaAttendees",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
