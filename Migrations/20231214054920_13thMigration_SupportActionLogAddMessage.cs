using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperAdmin.Service.Migrations
{
    /// <inheritdoc />
    public partial class _13thMigration_SupportActionLogAddMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketActionLogs_AspNetUsers_PerformedByUserId",
                table: "TicketActionLogs");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "TicketActionLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketActionLogs_AspNetUsers_PerformedByUserId",
                table: "TicketActionLogs",
                column: "PerformedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketActionLogs_AspNetUsers_PerformedByUserId",
                table: "TicketActionLogs");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "TicketActionLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketActionLogs_AspNetUsers_PerformedByUserId",
                table: "TicketActionLogs",
                column: "PerformedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
