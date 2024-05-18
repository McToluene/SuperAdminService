using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperAdmin.Service.Migrations
{
    /// <inheritdoc />
    public partial class _11thMigration_SupportActionLogDropUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketActionLogs_AspNetUsers_PerformedByUserIdId",
                table: "TicketActionLogs");

            migrationBuilder.DropIndex(
                name: "IX_TicketActionLogs_PerformedByUserIdId",
                table: "TicketActionLogs");

            migrationBuilder.DropColumn(
                name: "PerformedByUserIdId",
                table: "TicketActionLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PerformedByUserIdId",
                table: "TicketActionLogs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketActionLogs_PerformedByUserIdId",
                table: "TicketActionLogs",
                column: "PerformedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketActionLogs_AspNetUsers_PerformedByUserIdId",
                table: "TicketActionLogs",
                column: "PerformedByUserIdId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
