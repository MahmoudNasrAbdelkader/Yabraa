using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YabraaEF.Migrations
{
    public partial class addServicetypeTable2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceTypeId",
                table: "VisitDetails",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "ServiceTypeId",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_ServiceTypeId",
                table: "VisitDetails",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceTypeId",
                table: "Services",
                column: "ServiceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_ServiceTypes_ServiceTypeId",
                table: "Services",
                column: "ServiceTypeId",
                principalTable: "ServiceTypes",
                principalColumn: "ServiceTypeId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitDetails_ServiceTypes_ServiceTypeId",
                table: "VisitDetails",
                column: "ServiceTypeId",
                principalTable: "ServiceTypes",
                principalColumn: "ServiceTypeId",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_ServiceTypes_ServiceTypeId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitDetails_ServiceTypes_ServiceTypeId",
                table: "VisitDetails");

            migrationBuilder.DropIndex(
                name: "IX_VisitDetails_ServiceTypeId",
                table: "VisitDetails");

            migrationBuilder.DropIndex(
                name: "IX_Services_ServiceTypeId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ServiceTypeId",
                table: "VisitDetails");

            migrationBuilder.DropColumn(
                name: "ServiceTypeId",
                table: "Services");
        }
    }
}
