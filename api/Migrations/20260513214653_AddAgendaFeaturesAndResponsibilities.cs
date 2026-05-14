using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CouncilAgendaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAgendaFeaturesAndResponsibilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Meetings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AgendaItemId",
                table: "Assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AgendaAttendees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MeetingId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Attending = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaAttendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgendaAttendees_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgendaAttendees_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgendaNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MeetingId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgendaItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedByClerkUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgendaNotes_AgendaItems_AgendaItemId",
                        column: x => x.AgendaItemId,
                        principalTable: "AgendaItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AgendaNotes_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgendaItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    AttachmentType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_AgendaItems_AgendaItemId",
                        column: x => x.AgendaItemId,
                        principalTable: "AgendaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Attachments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetingDay = table.Column<string>(type: "text", nullable: false),
                    MeetingTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Frequency = table.Column<string>(type: "text", nullable: false),
                    WeekOfMonth = table.Column<int>(type: "integer", nullable: true),
                    MeetingDurationMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationSettings_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecurringResponsibilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    LcrUrl = table.Column<string>(type: "text", nullable: true),
                    MinWeeks = table.Column<int>(type: "integer", nullable: false),
                    MaxWeeks = table.Column<int>(type: "integer", nullable: false),
                    LastCheckedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCheckedByClerkUserId = table.Column<string>(type: "text", nullable: true),
                    OrgTypeScope = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringResponsibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringResponsibilities_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResponsibilityCheckins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ResponsibilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedByClerkUserId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibilityCheckins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponsibilityCheckins_RecurringResponsibilities_Responsibi~",
                        column: x => x.ResponsibilityId,
                        principalTable: "RecurringResponsibilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_AgendaItemId",
                table: "Assignments",
                column: "AgendaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaAttendees_MeetingId",
                table: "AgendaAttendees",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaAttendees_MemberId",
                table: "AgendaAttendees",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaNotes_AgendaItemId",
                table: "AgendaNotes",
                column: "AgendaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaNotes_MeetingId",
                table: "AgendaNotes",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_AgendaItemId",
                table: "Attachments",
                column: "AgendaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_OrganizationId",
                table: "Attachments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSettings_OrganizationId",
                table: "OrganizationSettings",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringResponsibilities_OrganizationId",
                table: "RecurringResponsibilities",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityCheckins_ResponsibilityId",
                table: "ResponsibilityCheckins",
                column: "ResponsibilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_AgendaItems_AgendaItemId",
                table: "Assignments",
                column: "AgendaItemId",
                principalTable: "AgendaItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_AgendaItems_AgendaItemId",
                table: "Assignments");

            migrationBuilder.DropTable(
                name: "AgendaAttendees");

            migrationBuilder.DropTable(
                name: "AgendaNotes");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "OrganizationSettings");

            migrationBuilder.DropTable(
                name: "ResponsibilityCheckins");

            migrationBuilder.DropTable(
                name: "RecurringResponsibilities");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_AgendaItemId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "AgendaItemId",
                table: "Assignments");
        }
    }
}
