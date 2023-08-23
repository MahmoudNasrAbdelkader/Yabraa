using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YabraaEF.Migrations
{
    public partial class UpdateVisisteDetailsTable2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VisitDetails",
                columns: table => new
                {
                    VisitDetailsId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitDT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocatioLongitude = table.Column<double>(type: "float", nullable: true),
                    LocationLatitude = table.Column<double>(type: "float", nullable: true),
                    LocatioAltitude = table.Column<double>(type: "float", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserFamilyId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceDetailsId = table.Column<long>(type: "bigint", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    VisitStatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitDetails", x => x.VisitDetailsId);
                    table.ForeignKey(
                        name: "FK_VisitDetails_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitDetails_InvoiceDetails_InvoiceDetailsId",
                        column: x => x.InvoiceDetailsId,
                        principalTable: "InvoiceDetails",
                        principalColumn: "InvoiceDetailsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitDetails_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_VisitDetails_UserFamilies_UserFamilyId",
                        column: x => x.UserFamilyId,
                        principalTable: "UserFamilies",
                        principalColumn: "UserFamilyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitDetails_VisitStatuses_VisitStatusId",
                        column: x => x.VisitStatusId,
                        principalTable: "VisitStatuses",
                        principalColumn: "VisitStatusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_ApplicationUserId",
                table: "VisitDetails",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_InvoiceDetailsId",
                table: "VisitDetails",
                column: "InvoiceDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_PackageId",
                table: "VisitDetails",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_UserFamilyId",
                table: "VisitDetails",
                column: "UserFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_VisitStatusId",
                table: "VisitDetails",
                column: "VisitStatusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisitDetails");
        }
    }
}
