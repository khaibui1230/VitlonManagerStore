using Microsoft.EntityFrameworkCore.Migrations;
using System.Linq;

#nullable disable

namespace QuanVitLonManager.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitalMenuItemData : Migration
    {
        /// <inheritdoc />
       
    protected override void Up(MigrationBuilder migrationBuilder)
    {
       
        // Seed Categories
        migrationBuilder.InsertData(
            table: "Categories",
            columns: new[] { "Id", "Name", "Description", "DisplayOrder", "IsActive", "ImageUrl" },
            values: new object[,]
            {
                { 1, "Món Chiên", "Các món chiên giòn", 5, true, "/images/categories/mon-chien.jpg" },
                { 2, "Món Xào", "Các món xào ngon miệng", 6, true, "/images/categories/mon-xao.jpg" },
                { 3, "Mì Xào", "Mì xào các loại", 7, true, "/images/categories/mi-xao.jpg" },
                { 4, "Bắp Xào", "Các món bắp xào", 8, true, "/images/categories/bap-xao.jpg" },
                { 5, "Món Luộc", "Các món luộc", 9, true, "/images/categories/mon-luoc.jpg" },
                { 6, "Nước giải khát", "Đồ uống", 10, true, "/images/categories/nuoc.jpg" },
                { 7, "Nước Chấm", "Các loại nước chấm", 11, true, "/images/categories/nuoc-cham.jpg" }
            });

        // Seed Menu Items
        migrationBuilder.InsertData(
            table: "MenuItems",
            columns: new[] { "Id", "Name", "Price", "OriginalPrice", "CategoryId", "DiscountPercentage", "IsNew", "IsPopular", "IsOnSale", "IsAvailable", "DisplayOrder" },
            values: new object[,]
            {
                // Món Chiên
                { 1, "Cút lộn chiên giòn chấm me", 23000m, 23000m, 1, 0, false, false, false, true, 1 },
                { 2, "Cút lộn chiên giòn chấm sa tế", 23000, 23000, 1, 0, false, false, false, true, 2 },
                { 3, "Cút lộn chiên giòn tương ớt", 23000, 23000, 1, 0, false, false, false, true, 3 },
                { 4, "Cút lộn chiên giòn xí muội", 23000, 23000, 1, 0, false, false, false, true, 4 },
                { 5, "Cút lộn chiên giòn mayone", 23000, 23000, 1, 0, false, false, false, true, 5 },
                { 6, "Cút lộn chiên giòn sốt bơ hành", 23000, 23000, 1, 0, false, false, false, true, 6 },
                { 7, "Cút lộn chiên giòn sốt bơ tỏi", 23000, 23000, 1, 0, false, false, false, true, 7 },
                { 8, "Cút lộn chiên giòn phô mai hành", 25000, 25000, 1, 0, false, false, false, true, 8 },
                { 9, "Cút lộn chiên giòn phô mai tỏi", 25000, 25000, 1, 0, false, false, false, true, 9 },
                { 10, "Vịt lộn chiên giòn chấm me", 24000, 24000, 1, 0, false, false, false, true, 10 },
                { 11, "Vịt lộn chiên giòn chấm sa tế", 24000, 24000, 1, 0, false, false, false, true, 11 },
                { 12, "Vịt lộn chiên giòn tương ớt", 24000, 24000, 1, 0, false, false, false, true, 12 },
                { 13, "Vịt lộn chiên giòn xí muội", 24000, 24000, 1, 0, false, false, false, true, 13 },
                { 14, "Vịt lộn chiên giòn tương cà", 24000, 24000, 1, 0, false, false, false, true, 14 },
                { 15, "Vịt lộn chiên giòn mayone", 24000, 24000, 1, 0, false, false, false, true, 15 },
                { 16, "Vịt lộn chiên giòn sốt bơ hành", 24000, 24000, 1, 0, false, false, false, true, 16 },
                { 17, "Vịt lộn chiên giòn sốt bơ tỏi", 24000, 24000, 1, 0, false, false, false, true, 17 },
                { 18, "Vịt lộn chiên giòn phô mai hành", 26000, 26000, 1, 0, false, false, false, true, 18 },
                { 19, "Vịt lộn chiên giòn phô mai tỏi", 26000, 26000, 1, 0, false, false, false, true, 19 },
                // Món Xào
                { 20, "Cút xào me", 21000, 21000, 2, 0, false, false, false, true, 1 },
                { 21, "Cút xào bơ tỏi", 21000, 21000, 2, 0, false, false, false, true, 2 },
                { 22, "Cút xào sa tế", 21000, 21000, 2, 0, false, false, false, true, 3 },
                { 23, "Cút xào phô mai", 24000, 24000, 2, 0, false, false, false, true, 4 },
                { 24, "Cút xào rau muống me", 23000, 23000, 2, 0, false, false, false, true, 5 },
                { 25, "Cút xào rau muống bơ tỏi", 23000, 23000, 2, 0, false, false, false, true, 6 },
                { 26, "Cút xào rau muống sa tế", 23000, 23000, 2, 0, false, false, false, true, 7 },
                { 27, "Vịt xào me", 21000, 21000, 2, 0, false, false, false, true, 8 },
                { 28, "Vịt xào bơ tỏi", 21000, 21000, 2, 0, false, false, false, true, 9 },
                { 29, "Vịt xào sa tế", 21000, 21000, 2, 0, false, false, false, true, 10 },
                { 30, "Vịt xào phô mai", 24000, 24000, 2, 0, false, false, false, true, 11 },
                { 31, "Vịt xào rau muống me", 23000, 23000, 2, 0, false, false, false, true, 12 },
                { 32, "Vịt xào rau muống bơ tỏi", 23000, 23000, 2, 0, false, false, false, true, 13 },
                { 33, "Vịt xào rau muống sa tế", 23000, 23000, 2, 0, false, false, false, true, 14 },
                // Mì Xào
                { 34, "Mì xào không trứng", 15000, 15000, 3, 0, false, false, false, true, 1 },
                { 35, "Mì xào cút lộn", 32000, 32000, 3, 0, false, false, false, true, 2 },
                { 36, "Mì xào vịt lộn", 32000, 32000, 3, 0, false, false, false, true, 3 },
                { 37, "Mì xào hột gà nướng", 32000, 32000, 3, 0, false, false, false, true, 4 },
                { 38, "Mì xào bò", 35000, 35000, 3, 0, false, false, false, true, 5 },
                { 39, "Nui xào bò", 35000, 35000, 3, 0, false, false, false, true, 6 },
                // Bắp Xào
                { 40, "Bắp xào truyền thống", 20000, 20000, 4, 0, false, false, false, true, 1 },
                { 41, "Bắp xào trứng muối", 27000, 27000, 4, 0, false, false, false, true, 2 },
                { 42, "Bắp xào phô mai", 27000, 27000, 4, 0, false, false, false, true, 3 },
                { 43, "Bắp xào Cút lộn", 30000, 30000, 4, 0, false, false, false, true, 4 },
                { 44, "Bắp xào Vịt lộn", 30000, 30000, 4, 0, false, false, false, true, 5 },
                { 45, "Bắp xào Hột gà nướng", 30000, 30000, 4, 0, false, false, false, true, 6 },
                // Món Luộc
                { 46, "Cút lộn luộc", 14000, 14000, 5, 0, false, false, false, true, 1 },
                { 47, "Vịt lộn luộc", 8000, 8000, 5, 0, false, false, false, true, 2 },
                { 48, "Trứng gà nướng", 7000, 7000, 5, 0, false, false, false, true, 3 },
                // Nước
                { 49, "Sting", 12000, 12000, 6, 0, false, false, false, true, 1 },
                { 50, "Pepsi", 12000, 12000, 6, 0, false, false, false, true, 2 },
                { 51, "7up", 12000, 12000, 6, 0, false, false, false, true, 3 },
                { 52, "Coca", 12000, 12000, 6, 0, false, false, false, true, 4 },
                { 53, "Xá xị", 12000, 12000, 6, 0, false, false, false, true, 5 },
                { 54, "Nước suối", 8000, 8000, 6, 0, false, false, false, true, 6 },
                // Nước Chấm
                { 55, "Nước chấm me", 2000, 2000, 7, 0, false, false, false, true, 1 },
                { 56, "Nước chấm sa tế", 2000, 2000, 7, 0, false, false, false, true, 2 },
                { 57, "Nước chấm xí muội", 2000, 2000, 7, 0, false, false, false, true, 3 },
                { 58, "Nước chấm tương ớt", 2000, 2000, 7, 0, false, false, false, true, 4 },
                { 59, "Nước chấm mayonese", 2000, 2000, 7, 0, false, false, false, true, 5 }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "MenuItems",
            keyColumn: "Id",
            keyValues: Enumerable.Range(1, 60).Select(i => (object)i).ToArray());

        migrationBuilder.DeleteData(
            table: "Categories",
            keyColumn: "Id",
            keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7 });
    }
    }
}