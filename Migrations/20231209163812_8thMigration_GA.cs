using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperAdmin.Service.Migrations
{
    /// <inheritdoc />
    public partial class _8thMigration_GA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoSavingSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationInMonths = table.Column<int>(type: "int", nullable: false),
                    InterestPercentage = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoSavingSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutoSavingSettings_DurationInMonths",
                table: "AutoSavingSettings",
                column: "DurationInMonths",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoSavingSettings");
        }
    }
}
