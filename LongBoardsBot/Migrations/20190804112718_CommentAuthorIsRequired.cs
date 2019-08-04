using Microsoft.EntityFrameworkCore.Migrations;

namespace LongBoardsBot.Migrations
{
    public partial class CommentAuthorIsRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_BotUsers_AuthorChatId",
                table: "Comments");

            migrationBuilder.AlterColumn<long>(
                name: "AuthorChatId",
                table: "Comments",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_BotUsers_AuthorChatId",
                table: "Comments",
                column: "AuthorChatId",
                principalTable: "BotUsers",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_BotUsers_AuthorChatId",
                table: "Comments");

            migrationBuilder.AlterColumn<long>(
                name: "AuthorChatId",
                table: "Comments",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_BotUsers_AuthorChatId",
                table: "Comments",
                column: "AuthorChatId",
                principalTable: "BotUsers",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
