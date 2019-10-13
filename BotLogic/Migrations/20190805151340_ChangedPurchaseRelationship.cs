using Microsoft.EntityFrameworkCore.Migrations;

namespace LongBoardsBot.Migrations
{
    public partial class ChangedPurchaseRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_BotUsers_BotUserChatId",
                table: "Purchases");

            migrationBuilder.AlterColumn<long>(
                name: "BotUserChatId",
                table: "Purchases",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_BotUsers_BotUserChatId",
                table: "Purchases",
                column: "BotUserChatId",
                principalTable: "BotUsers",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_BotUsers_BotUserChatId",
                table: "Purchases");

            migrationBuilder.AlterColumn<long>(
                name: "BotUserChatId",
                table: "Purchases",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_BotUsers_BotUserChatId",
                table: "Purchases",
                column: "BotUserChatId",
                principalTable: "BotUsers",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
