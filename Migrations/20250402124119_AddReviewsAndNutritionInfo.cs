using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanVitLonManager.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewsAndNutritionInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Calories",
                table: "MenuItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Carbs",
                table: "MenuItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailedDescription",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Fat",
                table: "MenuItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Protein",
                table: "MenuItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVerifiedPurchase = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MenuItemId",
                table: "Reviews",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropColumn(
                name: "Calories",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Carbs",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "DetailedDescription",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Fat",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Protein",
                table: "MenuItems");
        }
    }
}
