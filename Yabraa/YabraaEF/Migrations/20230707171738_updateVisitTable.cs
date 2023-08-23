using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YabraaEF.Migrations
{
    public partial class updateVisitTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "VisitStatuses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "VisitStatusId",
                table: "VisitDetails",
                type: "int",
                nullable: true,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_VisitStatusId",
                table: "VisitDetails",
                column: "VisitStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_VisitDetails_VisitStatuses_VisitStatusId",
                table: "VisitDetails",
                column: "VisitStatusId",
                principalTable: "VisitStatuses",
                principalColumn: "VisitStatusId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisitDetails_VisitStatuses_VisitStatusId",
                table: "VisitDetails");

            migrationBuilder.DropIndex(
                name: "IX_VisitDetails_VisitStatusId",
                table: "VisitDetails");

            migrationBuilder.DropColumn(
                name: "VisitStatusId",
                table: "VisitDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "VisitStatuses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
