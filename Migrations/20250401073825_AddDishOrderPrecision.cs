using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanVitLonManager.Migrations
{
    /// <inheritdoc />
    public partial class AddDishOrderPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "DishOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "DishOrders");
        }
    }
}
