using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YabraaEF.Migrations
{
    public partial class updateattchandnotetables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "VisitDetailsId",
                table: "VisitNotes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "VisitDetailsId",
                table: "VisitAttachments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_VisitNotes_VisitDetailsId",
                table: "VisitNotes",
                column: "VisitDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitAttachments_VisitDetailsId",
                table: "VisitAttachments",
                column: "VisitDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_VisitAttachments_VisitDetails_VisitDetailsId",
                table: "VisitAttachments",
                column: "VisitDetailsId",
                principalTable: "VisitDetails",
                principalColumn: "VisitDetailsId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitNotes_VisitDetails_VisitDetailsId",
                table: "VisitNotes",
                column: "VisitDetailsId",
                principalTable: "VisitDetails",
                principalColumn: "VisitDetailsId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisitAttachments_VisitDetails_VisitDetailsId",
                table: "VisitAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitNotes_VisitDetails_VisitDetailsId",
                table: "VisitNotes");

            migrationBuilder.DropIndex(
                name: "IX_VisitNotes_VisitDetailsId",
                table: "VisitNotes");

            migrationBuilder.DropIndex(
                name: "IX_VisitAttachments_VisitDetailsId",
                table: "VisitAttachments");

            migrationBuilder.DropColumn(
                name: "VisitDetailsId",
                table: "VisitNotes");

            migrationBuilder.DropColumn(
                name: "VisitDetailsId",
                table: "VisitAttachments");
        }
    }
}
