using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMCEventAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixRegistrationRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Events_EventId1",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Participants_ParticipantId1",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_EventId1",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_ParticipantId1",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "ParticipantId1",
                table: "Registrations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventId1",
                table: "Registrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParticipantId1",
                table: "Registrations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_EventId1",
                table: "Registrations",
                column: "EventId1");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ParticipantId1",
                table: "Registrations",
                column: "ParticipantId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Events_EventId1",
                table: "Registrations",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Participants_ParticipantId1",
                table: "Registrations",
                column: "ParticipantId1",
                principalTable: "Participants",
                principalColumn: "ParticipantId");
        }
    }
}
