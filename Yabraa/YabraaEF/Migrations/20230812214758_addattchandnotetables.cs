using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YabraaEF.Migrations
{
    public partial class addattchandnotetables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VisitAttachments",
                columns: table => new
                {
                    VisitAttachmentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDTs = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateSystemUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitAttachments", x => x.VisitAttachmentId);
                });

            migrationBuilder.CreateTable(
                name: "VisitNotes",
                columns: table => new
                {
                    VisitNoteId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDTs = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateSystemUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitNotes", x => x.VisitNoteId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisitAttachments");

            migrationBuilder.DropTable(
                name: "VisitNotes");
        }
    }
}
