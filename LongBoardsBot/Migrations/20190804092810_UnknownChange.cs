using Microsoft.EntityFrameworkCore.Migrations;

namespace LongBoardsBot.Migrations
{
    public partial class UnknownChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BotUserLongBoard_Purchase_PurchaseGuid",
                table: "BotUserLongBoard");

            migrationBuilder.DropForeignKey(
                name: "FK_BotUsers_Purchase_LatestPurchaseGuid",
                table: "BotUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_BotUsers_BotUserChatId",
                table: "Purchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Purchase",
                table: "Purchase");

            migrationBuilder.RenameTable(
                name: "Purchase",
                newName: "Purchases");

            migrationBuilder.RenameIndex(
                name: "IX_Purchase_BotUserChatId",
                table: "Purchases",
                newName: "IX_Purchases_BotUserChatId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Purchases",
                table: "Purchases",
                column: "Guid");

            migrationBuilder.AddForeignKey(
                name: "FK_BotUserLongBoard_Purchases_PurchaseGuid",
                table: "BotUserLongBoard",
                column: "PurchaseGuid",
                principalTable: "Purchases",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BotUsers_Purchases_LatestPurchaseGuid",
                table: "BotUsers",
                column: "LatestPurchaseGuid",
                principalTable: "Purchases",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_BotUsers_BotUserChatId",
                table: "Purchases",
                column: "BotUserChatId",
                principalTable: "BotUsers",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BotUserLongBoard_Purchases_PurchaseGuid",
                table: "BotUserLongBoard");

            migrationBuilder.DropForeignKey(
                name: "FK_BotUsers_Purchases_LatestPurchaseGuid",
                table: "BotUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_BotUsers_BotUserChatId",
                table: "Purchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Purchases",
                table: "Purchases");

            migrationBuilder.RenameTable(
                name: "Purchases",
                newName: "Purchase");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_BotUserChatId",
                table: "Purchase",
                newName: "IX_Purchase_BotUserChatId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Purchase",
                table: "Purchase",
                column: "Guid");

            migrationBuilder.AddForeignKey(
                name: "FK_BotUserLongBoard_Purchase_PurchaseGuid",
                table: "BotUserLongBoard",
                column: "PurchaseGuid",
                principalTable: "Purchase",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BotUsers_Purchase_LatestPurchaseGuid",
                table: "BotUsers",
                column: "LatestPurchaseGuid",
                principalTable: "Purchase",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_BotUsers_BotUserChatId",
                table: "Purchase",
                column: "BotUserChatId",
                principalTable: "BotUsers",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
