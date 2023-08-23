using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YabraaEF.Migrations
{
    public partial class updatvar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LocatioLongitude",
                table: "VisitDetails",
                newName: "LocationLongitude");

            migrationBuilder.RenameColumn(
                name: "LocatioAltitude",
                table: "VisitDetails",
                newName: "LocationAltitude");

            migrationBuilder.RenameColumn(
                name: "LocatioLongitude",
                table: "Invoices",
                newName: "LocationLongitude");

            migrationBuilder.RenameColumn(
                name: "LocatioAltitude",
                table: "Invoices",
                newName: "LocationAltitude");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LocationLongitude",
                table: "VisitDetails",
                newName: "LocatioLongitude");

            migrationBuilder.RenameColumn(
                name: "LocationAltitude",
                table: "VisitDetails",
                newName: "LocatioAltitude");

            migrationBuilder.RenameColumn(
                name: "LocationLongitude",
                table: "Invoices",
                newName: "LocatioLongitude");

            migrationBuilder.RenameColumn(
                name: "LocationAltitude",
                table: "Invoices",
                newName: "LocatioAltitude");
        }
    }
}
