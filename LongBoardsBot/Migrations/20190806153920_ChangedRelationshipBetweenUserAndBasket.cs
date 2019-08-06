using Microsoft.EntityFrameworkCore.Migrations;

namespace LongBoardsBot.Migrations
{
    public partial class ChangedRelationshipBetweenUserAndBasket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BotUsers_Purchases_LatestPurchaseGuid",
                table: "BotUsers");

            migrationBuilder.RenameColumn(
                name: "LatestPurchaseGuid",
                table: "BotUsers",
                newName: "CurrentPurchaseGuid");

            migrationBuilder.RenameIndex(
                name: "IX_BotUsers_LatestPurchaseGuid",
                table: "BotUsers",
                newName: "IX_BotUsers_CurrentPurchaseGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_BotUsers_Purchases_CurrentPurchaseGuid",
                table: "BotUsers",
                column: "CurrentPurchaseGuid",
                principalTable: "Purchases",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BotUsers_Purchases_CurrentPurchaseGuid",
                table: "BotUsers");

            migrationBuilder.RenameColumn(
                name: "CurrentPurchaseGuid",
                table: "BotUsers",
                newName: "LatestPurchaseGuid");

            migrationBuilder.RenameIndex(
                name: "IX_BotUsers_CurrentPurchaseGuid",
                table: "BotUsers",
                newName: "IX_BotUsers_LatestPurchaseGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_BotUsers_Purchases_LatestPurchaseGuid",
                table: "BotUsers",
                column: "LatestPurchaseGuid",
                principalTable: "Purchases",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
