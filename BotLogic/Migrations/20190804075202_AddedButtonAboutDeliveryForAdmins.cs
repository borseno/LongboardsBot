using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LongBoardsBot.Migrations
{
    public partial class AddedButtonAboutDeliveryForAdmins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LatestPurchaseGuid",
                table: "BotUsers",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseGuid",
                table: "BotUserLongBoard",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Purchase",
                columns: table => new
                {
                    Guid = table.Column<Guid>(nullable: false),
                    Cost = table.Column<decimal>(nullable: false),
                    Delivered = table.Column<bool>(nullable: false),
                    BotUserChatId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchase", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Purchase_BotUsers_BotUserChatId",
                        column: x => x.BotUserChatId,
                        principalTable: "BotUsers",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BotUsers_LatestPurchaseGuid",
                table: "BotUsers",
                column: "LatestPurchaseGuid");

            migrationBuilder.CreateIndex(
                name: "IX_BotUserLongBoard_PurchaseGuid",
                table: "BotUserLongBoard",
                column: "PurchaseGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_BotUserChatId",
                table: "Purchase",
                column: "BotUserChatId");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BotUserLongBoard_Purchase_PurchaseGuid",
                table: "BotUserLongBoard");

            migrationBuilder.DropForeignKey(
                name: "FK_BotUsers_Purchase_LatestPurchaseGuid",
                table: "BotUsers");

            migrationBuilder.DropTable(
                name: "Purchase");

            migrationBuilder.DropIndex(
                name: "IX_BotUsers_LatestPurchaseGuid",
                table: "BotUsers");

            migrationBuilder.DropIndex(
                name: "IX_BotUserLongBoard_PurchaseGuid",
                table: "BotUserLongBoard");

            migrationBuilder.DropColumn(
                name: "LatestPurchaseGuid",
                table: "BotUsers");

            migrationBuilder.DropColumn(
                name: "PurchaseGuid",
                table: "BotUserLongBoard");
        }
    }
}
