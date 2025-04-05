using Microsoft.EntityFrameworkCore.Migrations;
using System.Linq;

#nullable disable

namespace QuanVitLonManager.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataAgains : Migration
    {
        /// <inheritdoc />
       
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Thêm Categories với ID mới
        migrationBuilder.InsertData(
            table: "Categories",
            columns: new[] { "Id", "Name", "Description", "DisplayOrder", "IsActive", "ImageUrl" },
            values: new object[,]
            {
                { 101, "Món Chiên", "Các món chiên giòn", 5, true, "/images/categories/mon-chien.jpg" },
                { 102, "Món Xào", "Các món xào ngon miệng", 6, true, "/images/categories/mon-xao.jpg" },
                { 103, "Mì Xào", "Mì xào các loại", 7, true, "/images/categories/mi-xao.jpg" },
                { 104, "Bắp Xào", "Các món bắp xào", 8, true, "/images/categories/bap-xao.jpg" },
                { 105, "Món Luộc", "Các món luộc", 9, true, "/images/categories/mon-luoc.jpg" },
                { 106, "Nước giải khát", "Đồ uống", 10, true, "/images/categories/nuoc.jpg" },
                { 107, "Nước Chấm", "Các loại nước chấm", 11, true, "/images/categories/nuoc-cham.jpg" }
            });

        // Thêm MenuItems với ID và CategoryId mới
        migrationBuilder.InsertData(
            table: "MenuItems",
            columns: new[] { "Id", "Name", "Price", "OriginalPrice", "CategoryId", "DiscountPercentage", "IsNew", "IsPopular", "IsOnSale", "IsAvailable", "DisplayOrder" },
            values: new object[,]
            {
                { 1001, "Cút lộn chiên giòn chấm me", 23000, 23000, 101, 0, false, false, false, true, 1 },
                { 1002, "Cút lộn chiên giòn chấm sa tế", 23000, 23000, 101, 0, false, false, false, true, 2 },
                { 1003, "Cút lộn chiên giòn tương ớt", 23000, 23000, 101, 0, false, false, false, true, 3 },
                { 1004, "Cút lộn chiên giòn xí muội", 23000, 23000, 101, 0, false, false, false, true, 4 },
                { 1005, "Cút lộn chiên giòn mayone", 23000, 23000, 101, 0, false, false, false, true, 5 },
                { 1006, "Cút lộn chiên giòn sốt bơ hành", 23000, 23000, 101, 0, false, false, false, true, 6 },
                { 1007, "Cút lộn chiên giòn sốt bơ tỏi", 23000, 23000, 101, 0, false, false, false, true, 7 },
                { 1008, "Cút lộn chiên giòn phô mai hành", 25000, 25000, 101, 0, false, false, false, true, 8 },
                { 1009, "Cút lộn chiên giòn phô mai tỏi", 25000, 25000, 101, 0, false, false, false, true, 9 },
                { 1010, "Vịt lộn chiên giòn chấm me", 24000, 24000, 101, 0, false, false, false, true, 10 },
                { 1011, "Vịt lộn chiên giòn chấm sa tế", 24000, 24000, 101, 0, false, false, false, true, 11 },
                { 1012, "Vịt lộn chiên giòn tương ớt", 24000, 24000, 101, 0, false, false, false, true, 12 },
                { 1013, "Vịt lộn chiên giòn xí muội", 24000, 24000, 101, 0, false, false, false, true, 13 },
                { 1014, "Vịt lộn chiên giòn tương cà", 24000, 24000, 101, 0, false, false, false, true, 14 },
                { 1015, "Vịt lộn chiên giòn mayone", 24000, 24000, 101, 0, false, false, false, true, 15 },
                { 1016, "Vịt lộn chiên giòn sốt bơ hành", 24000, 24000, 101, 0, false, false, false, true, 16 },
                { 1017, "Vịt lộn chiên giòn sốt bơ tỏi", 24000, 24000, 101, 0, false, false, false, true, 17 },
                { 1018, "Vịt lộn chiên giòn phô mai hành", 26000, 26000, 101, 0, false, false, false, true, 18 },
                { 1019, "Vịt lộn chiên giòn phô mai tỏi", 26000, 26000, 101, 0, false, false, false, true, 19 },
                
                { 20, "Cút xào me", 21000, 21000, 102, 0, false, false, false, true, 1 },
                { 21, "Cút xào bơ tỏi", 21000, 21000, 102, 0, false, false, false, true, 2 },
                { 22, "Cút xào sa tế", 21000, 21000, 102, 0, false, false, false, true, 3 },
                { 23, "Cút xào phô mai", 24000, 24000, 102, 0, false, false, false, true, 4 },
                { 24, "Cút xào rau muống me", 23000, 23000, 102, 0, false, false, false, true, 5 },
                { 25, "Cút xào rau muống bơ tỏi", 23000, 23000, 102, 0, false, false, false, true, 6 },
                { 26, "Cút xào rau muống sa tế", 23000, 23000, 102, 0, false, false, false, true, 7 },
                { 27, "Vịt xào me", 21000, 21000, 102, 0, false, false, false, true, 8 },
                { 28, "Vịt xào bơ tỏi", 21000, 21000, 102, 0, false, false, false, true, 9 },
                { 29, "Vịt xào sa tế", 21000, 21000, 102, 0, false, false, false, true, 10 },
                { 30, "Vịt xào phô mai", 24000, 24000, 102, 0, false, false, false, true, 11 },
                { 31, "Vịt xào rau muống me", 23000, 23000, 102, 0, false, false, false, true, 12 },
                { 32, "Vịt xào rau muống bơ tỏi", 23000, 23000, 102, 0, false, false, false, true, 13 },
                { 33, "Vịt xào rau muống sa tế", 23000, 23000, 102, 0, false, false, false, true, 14 },

                { 34, "Mì xào không trứng", 15000, 15000, 103, 0, false, false, false, true, 1 },
                { 35, "Mì xào cút lộn", 32000, 32000, 103, 0, false, false, false, true, 2 },
                { 36, "Mì xào vịt lộn", 32000, 32000, 103, 0, false, false, false, true, 3 },
                { 37, "Mì xào hột gà nướng", 32000, 32000, 103, 0, false, false, false, true, 4 },
                { 38, "Mì xào bò", 35000, 35000, 103, 0, false, false, false, true, 5 },
                { 39, "Nui xào bò", 35000, 35000, 103, 0, false, false, false, true, 6 },

                { 40, "Bắp xào truyền thống", 20000, 20000, 104, 0, false, false, false, true, 1 },
                { 41, "Bắp xào trứng muối", 27000, 27000, 104, 0, false, false, false, true, 2 },
                { 42, "Bắp xào phô mai", 27000, 27000, 104, 0, false, false, false, true, 3 },
                { 43, "Bắp xào Cút lộn", 30000, 30000, 104, 0, false, false, false, true, 4 },
                { 44, "Bắp xào Vịt lộn", 30000, 30000, 104, 0, false, false, false, true, 5 },
                { 45, "Bắp xào Hột gà nướng", 30000, 30000, 104, 0, false, false, false, true, 6 },
                { 46, "Cút lộn luộc", 14000, 14000, 105, 0, false, false, false, true, 1 },
                { 47, "Vịt lộn luộc", 8000, 8000, 105, 0, false, false, false, true, 2 },
                { 48, "Trứng gà nướng", 7000, 7000, 105, 0, false, false, false, true, 3 },

                { 49, "Sting", 12000, 12000, 106, 0, false, false, false, true, 1 },
                { 50, "Pepsi", 12000, 12000, 106, 0, false, false, false, true, 2 },
                { 51, "7up", 12000, 12000, 106, 0, false, false, false, true, 3 },
                { 52, "Coca", 12000, 12000, 106, 0, false, false, false, true, 4 },
                { 53, "Xá xị", 12000, 12000, 106, 0, false, false, false, true, 5 },
                { 54, "Nước suối", 8000, 8000, 106, 0, false, false, false, true, 6 },

                { 55, "Nước chấm me", 2000, 2000, 107, 0, false, false, false, true, 1 },
                { 56, "Nước chấm sa tế", 2000, 2000, 107, 0, false, false, false, true, 2 },
                { 57, "Nước chấm xí muội", 2000, 2000, 107, 0, false, false, false, true, 3 },
                { 58, "Nước chấm tương ớt", 2000, 2000, 107, 0, false, false, false, true, 4 },
                { 59, "Nước chấm mayonese", 2000, 2000, 107, 0, false, false, false, true, 5 }
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