using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanVitLonManager.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesToDishOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CartItems",
                newName: "CartItemId");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserCartId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCarts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserCartId",
                table: "CartItems",
                column: "UserCartId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCarts_UserId",
                table: "UserCarts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_UserCarts_UserCartId",
                table: "CartItems",
                column: "UserCartId",
                principalTable: "UserCarts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_UserCarts_UserCartId",
                table: "CartItems");

            migrationBuilder.DropTable(
                name: "UserCarts");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_UserCartId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "UserCartId",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "CartItemId",
                table: "CartItems",
                newName: "Id");
        }
    }
}
