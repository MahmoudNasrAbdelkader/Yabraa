using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YabraaEF.Migrations
{
    public partial class alterPaymentModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "InvoiceDetails",
                newName: "Price");

            migrationBuilder.AddColumn<double>(
                name: "LocatioAltitude",
                table: "Invoices",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LocatioLongitude",
                table: "Invoices",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LocationLatitude",
                table: "Invoices",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocatioAltitude",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LocatioLongitude",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LocationLatitude",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "InvoiceDetails",
                newName: "TotalPrice");
        }
    }
}
