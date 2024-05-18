using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperAdmin.Service.Migrations
{
    /// <inheritdoc />
    public partial class _7thMigration_GA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayrollVendorChargeDeductionBasis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollVendorChargeDeductionBasis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollVendorChargeSetings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Percentage = table.Column<double>(type: "float", nullable: false),
                    ChargeDeductionBasisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollVendorChargeSetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollVendorChargeSetings_PayrollVendorChargeDeductionBasis_ChargeDeductionBasisId",
                        column: x => x.ChargeDeductionBasisId,
                        principalTable: "PayrollVendorChargeDeductionBasis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollVendorChargeSetings_ChargeDeductionBasisId",
                table: "PayrollVendorChargeSetings",
                column: "ChargeDeductionBasisId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollVendorChargeSetings");

            migrationBuilder.DropTable(
                name: "PayrollVendorChargeDeductionBasis");
        }
    }
}
