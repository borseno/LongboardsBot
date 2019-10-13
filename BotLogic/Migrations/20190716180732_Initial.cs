using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LongBoardsBot.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Longboards",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Style = table.Column<string>(nullable: false),
                    Price = table.Column<decimal>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Longboards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BotUsers",
                columns: table => new
                {
                    ChatId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(nullable: true),
                    Stage = table.Column<int>(nullable: false, defaultValue: 0),
                    PendingId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BotUsers", x => x.ChatId);
                    table.ForeignKey(
                        name: "FK_BotUsers_Longboards_PendingId",
                        column: x => x.PendingId,
                        principalTable: "Longboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BotUserLongBoard",
                columns: table => new
                {
                    BotUserId = table.Column<long>(nullable: false),
                    LongboardId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BotUserLongBoard", x => new { x.BotUserId, x.LongboardId });
                    table.ForeignKey(
                        name: "FK_BotUserLongBoard_BotUsers_BotUserId",
                        column: x => x.BotUserId,
                        principalTable: "BotUsers",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BotUserLongBoard_Longboards_LongboardId",
                        column: x => x.LongboardId,
                        principalTable: "Longboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessage",
                columns: table => new
                {
                    MessageId = table.Column<int>(nullable: false),
                    IgnoreDelete = table.Column<bool>(nullable: false),
                    UserChatId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessage", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_ChatMessage_BotUsers_UserChatId",
                        column: x => x.UserChatId,
                        principalTable: "BotUsers",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BotUserLongBoard_LongboardId",
                table: "BotUserLongBoard",
                column: "LongboardId");

            migrationBuilder.CreateIndex(
                name: "IX_BotUsers_PendingId",
                table: "BotUsers",
                column: "PendingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_UserChatId",
                table: "ChatMessage",
                column: "UserChatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BotUserLongBoard");

            migrationBuilder.DropTable(
                name: "ChatMessage");

            migrationBuilder.DropTable(
                name: "BotUsers");

            migrationBuilder.DropTable(
                name: "Longboards");
        }
    }
}
