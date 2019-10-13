using Microsoft.EntityFrameworkCore.Migrations;

namespace LongBoardsBot.Migrations
{
    public partial class LongboardTableUniqueStyleAndSeededData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Style",
                table: "Longboards",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.InsertData(
                table: "Longboards",
                columns: new[] { "Id", "Amount", "Price", "Style" },
                values: new object[] { -1, 4, 2849m, "Cruiser" });

            migrationBuilder.InsertData(
                table: "Longboards",
                columns: new[] { "Id", "Amount", "Price", "Style" },
                values: new object[] { -2, 4, 3049m, "Downhill" });

            migrationBuilder.InsertData(
                table: "Longboards",
                columns: new[] { "Id", "Amount", "Price", "Style" },
                values: new object[] { -3, 4, 2649m, "Carving" });

            migrationBuilder.CreateIndex(
                name: "IX_Longboards_Style",
                table: "Longboards",
                column: "Style",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Longboards_Style",
                table: "Longboards");

            migrationBuilder.DeleteData(
                table: "Longboards",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "Longboards",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Longboards",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.AlterColumn<string>(
                name: "Style",
                table: "Longboards",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
