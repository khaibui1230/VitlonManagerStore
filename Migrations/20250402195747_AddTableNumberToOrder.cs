using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanVitLonManager.Migrations
{
    /// <inheritdoc />
    public partial class AddTableNumberToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Note",
                table: "DishOrders",
                newName: "Notes");

            migrationBuilder.AddColumn<string>(
                name: "TableNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MenuItemId",
                table: "DishOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "DishOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DishOrders_MenuItemId",
                table: "DishOrders",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DishOrders_OrderId",
                table: "DishOrders",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishOrders_MenuItems_MenuItemId",
                table: "DishOrders",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DishOrders_Orders_OrderId",
                table: "DishOrders",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishOrders_MenuItems_MenuItemId",
                table: "DishOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_DishOrders_Orders_OrderId",
                table: "DishOrders");

            migrationBuilder.DropIndex(
                name: "IX_DishOrders_MenuItemId",
                table: "DishOrders");

            migrationBuilder.DropIndex(
                name: "IX_DishOrders_OrderId",
                table: "DishOrders");

            migrationBuilder.DropColumn(
                name: "TableNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MenuItemId",
                table: "DishOrders");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "DishOrders");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "DishOrders",
                newName: "Note");
        }
    }
}
