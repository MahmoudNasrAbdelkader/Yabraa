using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YabraaEF.Migrations
{
    public partial class verificationCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "verificationCode",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "verificationCode",
                table: "AspNetUsers");
        }
    }
}
