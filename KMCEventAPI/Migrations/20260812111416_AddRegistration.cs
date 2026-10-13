using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMCEventAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "IX_Registrations_EventId",
                table: "Registrations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_EventId1",
                table: "Registrations",
                column: "EventId1");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ParticipantId",
                table: "Registrations",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ParticipantId1",
                table: "Registrations",
                column: "ParticipantId1");

            migrationBuilder.CreateIndex(
                name: "IX_Events_OrganizerId",
                table: "Events",
                column: "OrganizerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Users_OrganizerId",
                table: "Events",
                column: "OrganizerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Events_EventId",
                table: "Registrations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Events_EventId1",
                table: "Registrations",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Participants_ParticipantId",
                table: "Registrations",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "ParticipantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Participants_ParticipantId1",
                table: "Registrations",
                column: "ParticipantId1",
                principalTable: "Participants",
                principalColumn: "ParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Users_OrganizerId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Events_EventId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Events_EventId1",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Participants_ParticipantId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Participants_ParticipantId1",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_EventId",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_EventId1",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_ParticipantId",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_ParticipantId1",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Events_OrganizerId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "ParticipantId1",
                table: "Registrations");
        }
    }
}
