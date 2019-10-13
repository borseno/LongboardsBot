using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LongBoardsBot.Migrations
{
    public partial class AddedTestingProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestingInfo",
                columns: table => new
                {
                    BotUserId = table.Column<long>(nullable: false),
                    VisitDateTime = table.Column<DateTime>(nullable: false),
                    Occurred = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestingInfo", x => x.BotUserId);
                    table.ForeignKey(
                        name: "FK_TestingInfo_BotUsers_BotUserId",
                        column: x => x.BotUserId,
                        principalTable: "BotUsers",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestingInfo");
        }
    }
}
