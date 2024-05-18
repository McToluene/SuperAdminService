using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperAdmin.Service.Migrations
{
    /// <inheritdoc />
    public partial class _12thMigration_SupportActionLogAddUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PerformedByUserId",
                table: "TicketActionLogs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TicketActionLogs_PerformedByUserId",
                table: "TicketActionLogs",
                column: "PerformedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketActionLogs_AspNetUsers_PerformedByUserId",
                table: "TicketActionLogs",
                column: "PerformedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketActionLogs_AspNetUsers_PerformedByUserId",
                table: "TicketActionLogs");

            migrationBuilder.DropIndex(
                name: "IX_TicketActionLogs_PerformedByUserId",
                table: "TicketActionLogs");

            migrationBuilder.DropColumn(
                name: "PerformedByUserId",
                table: "TicketActionLogs");
        }
    }
}
