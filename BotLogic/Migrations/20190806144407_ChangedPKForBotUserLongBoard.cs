using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LongBoardsBot.Migrations
{
    public partial class ChangedPKForBotUserLongBoard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BotUserLongBoard",
                table: "BotUserLongBoard");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "BotUserLongBoard",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BotUserLongBoard",
                table: "BotUserLongBoard",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BotUserLongBoard_BotUserId",
                table: "BotUserLongBoard",
                column: "BotUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BotUserLongBoard",
                table: "BotUserLongBoard");

            migrationBuilder.DropIndex(
                name: "IX_BotUserLongBoard_BotUserId",
                table: "BotUserLongBoard");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "BotUserLongBoard");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BotUserLongBoard",
                table: "BotUserLongBoard",
                columns: new[] { "BotUserId", "LongboardId" });
        }
    }
}
