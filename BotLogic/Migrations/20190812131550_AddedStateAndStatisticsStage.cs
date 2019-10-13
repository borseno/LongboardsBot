using Microsoft.EntityFrameworkCore.Migrations;

namespace LongBoardsBot.Migrations
{
    public partial class AddedStateAndStatisticsStage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOneTimeStatistics",
                table: "BotUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "BotUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatisticsStage",
                table: "BotUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOneTimeStatistics",
                table: "BotUsers");

            migrationBuilder.DropColumn(
                name: "State",
                table: "BotUsers");

            migrationBuilder.DropColumn(
                name: "StatisticsStage",
                table: "BotUsers");
        }
    }
}
