using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YabraaEF.Migrations
{
    public partial class updatetableStartPages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "StartPages",
                newName: "TitleEn");

            migrationBuilder.RenameColumn(
                name: "SubTitle",
                table: "StartPages",
                newName: "SubTitleEn");

            migrationBuilder.AddColumn<string>(
                name: "SubTitleAR",
                table: "StartPages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "StartPages",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubTitleAR",
                table: "StartPages");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "StartPages");

            migrationBuilder.RenameColumn(
                name: "TitleEn",
                table: "StartPages",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "SubTitleEn",
                table: "StartPages",
                newName: "SubTitle");
        }
    }
}
