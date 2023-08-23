using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YabraaEF.Migrations
{
    public partial class updateUserFamilyTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_UserFamilyId",
                table: "VisitDetails",
                column: "UserFamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_VisitDetails_UserFamilies_UserFamilyId",
                table: "VisitDetails",
                column: "UserFamilyId",
                principalTable: "UserFamilies",
                principalColumn: "UserFamilyId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisitDetails_UserFamilies_UserFamilyId",
                table: "VisitDetails");

            migrationBuilder.DropIndex(
                name: "IX_VisitDetails_UserFamilyId",
                table: "VisitDetails");
        }
    }
}
