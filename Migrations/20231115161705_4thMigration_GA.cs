using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperAdmin.Service.Migrations
{
    /// <inheritdoc />
    public partial class _4thMigration_GA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalaryDisbursementChargeSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Percentage = table.Column<double>(type: "float", nullable: false),
                    SettingType = table.Column<int>(type: "int", nullable: false),
                    ChargeDeductionPeriod = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryDisbursementChargeSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanySalaryDisbursementChargeSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaysCompanyId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SettingStatus = table.Column<int>(type: "int", nullable: false),
                    SalaryPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextChargeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalaryDisbursementChargeSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySalaryDisbursementChargeSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanySalaryDisbursementChargeSettings_SalaryDisbursementChargeSettings_SalaryDisbursementChargeSettingId",
                        column: x => x.SalaryDisbursementChargeSettingId,
                        principalTable: "SalaryDisbursementChargeSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySalaryDisbursementChargeSettings_SalaryDisbursementChargeSettingId",
                table: "CompanySalaryDisbursementChargeSettings",
                column: "SalaryDisbursementChargeSettingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySalaryDisbursementChargeSettings");

            migrationBuilder.DropTable(
                name: "SalaryDisbursementChargeSettings");
        }
    }
}
