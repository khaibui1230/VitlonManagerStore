using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanVitLonManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 20, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 255, nullable: false),
                    WelcomeMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 255, nullable: false),
                    GoodbyeMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DetailedDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Ingredients = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PreparationInstructions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<int>(type: "int", nullable: false),
                    IsNew = table.Column<bool>(type: "bit", nullable: false),
                    IsPopular = table.Column<bool>(type: "bit", nullable: false),
                    IsOnSale = table.Column<bool>(type: "bit", nullable: false),
                    Calories = table.Column<int>(type: "int", nullable: true),
                    Protein = table.Column<int>(type: "int", nullable: true),
                    Fat = table.Column<int>(type: "int", nullable: true),
                    Carbs = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TableId = table.Column<int>(type: "int", nullable: true),
                    TableNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    ReservationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserCartId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemId);
                    table.ForeignKey(
                        name: "FK_CartItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_UserCarts_UserCartId",
                        column: x => x.UserCartId,
                        principalTable: "UserCarts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 500, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "DishOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OrderTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    MenuItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishOrders_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DishOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDetails_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", "ca9e8afa-03ef-4208-b9fc-be6411c3fabd", "Admin", "ADMIN" },
                    { "2", "e0c38c60-2e50-47c8-ac77-23010d9f2af7", "Staff", "STAFF" },
                    { "3", "c11fce88-53e6-4098-b230-5fb755c2523b", "Customer", "CUSTOMER" },
                    { "4", "17cae6f0-68cc-40d3-9e70-ee16eb35bf59", "Chef", "CHEF" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "1", 0, "Admin Address", "4945a04e-4718-476c-81f3-19559854e095", new DateTime(2025, 4, 9, 14, 32, 10, 354, DateTimeKind.Utc).AddTicks(6744), "admin@gmail.com", true, "Admin", "User", false, null, "ADMIN@GMAIL.COM", "ADMIN@GMAIL.COM", "AQAAAAIAAYagAAAAECJPbiyHFCQHHMrnxg3rnjzXZFoieywHfehxqi3OegdJ3okLMVv9ydfLBKQx5cG5Zw==", "0123456789", false, "91baa4db-b21d-4402-8517-979d83b3b017", false, "admin@gmail.com" },
                    { "2", 0, "Staff Address", "921f6b4d-e799-495b-a6da-9526b66730ef", new DateTime(2025, 4, 9, 14, 32, 10, 412, DateTimeKind.Utc).AddTicks(8568), "staff@gmail.com", true, "Staff", "User", false, null, "STAFF@GMAIL.COM", "STAFF@GMAIL.COM", "AQAAAAIAAYagAAAAEIcs1T1PlPIeQXLtmDHjyeoMGJl2DvckWvZPDkzo7ZwH0fIFs94kD07xcqo+n8NbYA==", "0123456788", false, "ceaec550-ee09-4e05-b850-59b90297832e", false, "staff@gmail.com" },
                    { "3", 0, "Customer Address", "f5e4dbcf-56b7-4ad5-bb07-672206e4ef94", new DateTime(2025, 4, 9, 14, 32, 10, 496, DateTimeKind.Utc).AddTicks(4511), "customer@gmail.com", true, "Customer", "User", false, null, "CUSTOMER@GMAIL.COM", "CUSTOMER@GMAIL.COM", "AQAAAAIAAYagAAAAENaaw0tS9z7x26PejtYWc14LsyPPrt4Cd4q1rJvyWMcfb5YO1wjbMUYGymhNsDzrcA==", "0123456787", false, "9d2db350-b523-4216-a2d6-a6e408455500", false, "customer@gmail.com" },
                    { "4", 0, "Chef Address", "086f8490-2d1b-4432-b601-e66f9eddc426", new DateTime(2025, 4, 9, 14, 32, 10, 600, DateTimeKind.Utc).AddTicks(2708), "chef@gmail.com", true, "Chef", "User", false, null, "CHEF@GMAIL.COM", "CHEF@GMAIL.COM", "AQAAAAIAAYagAAAAEPpLaoglqXmMFsZBTJZbfGI8WVaNEpmd6enc0nP+/hN/Rpe1KWCCs5fcibo1PckRag==", "0123456786", false, "9d294c47-4103-4afb-ace7-5f373142176b", false, "chef@gmail.com" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "DisplayOrder", "ImageUrl", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Các món chiên giòn", 5, "/images/categories/mon-chien.jpg", true, "Món Chiên" },
                    { 2, "Các món xào ngon miệng", 6, "/images/categories/mon-xao.jpg", true, "Món Xào" },
                    { 3, "Mì xào các loại", 7, "/images/categories/mi-xao.jpg", true, "Mì Xào" },
                    { 4, "Các món bắp xào", 8, "/images/categories/bap-xao.jpg", true, "Bắp Xào" },
                    { 5, "Các món luộc", 9, "/images/categories/mon-luoc.jpg", true, "Món Luộc" },
                    { 6, "Đồ uống", 10, "/images/categories/nuoc.jpg", true, "Nước giải khát" },
                    { 7, "Các loại nước chấm", 11, "/images/categories/nuoc-cham.jpg", true, "Nước Chấm" }
                });

            migrationBuilder.InsertData(
                table: "RestaurantInfo",
                columns: new[] { "Id", "Address", "Description", "Email", "GoodbyeMessage", "LogoUrl", "Name", "Phone", "TaxId", "WelcomeMessage" },
                values: new object[] { 1, "354 lê văn thọ, phường 11 quận gò vấp, tp hcm", "Nhà hàng chuyên phục vụ các món vịt lộn và cút lộn", "quanHienvitlon@gmail.com", "Cảm ơn quý khách đã ghé thăm", "/images/logo.png", "Quán Hiển vịt lộn - Cút lộn", "0379665639", "1234567890", "Chào mừng đến với Quán Vịt Lộn" });

            migrationBuilder.InsertData(
                table: "Tables",
                columns: new[] { "Id", "Area", "Capacity", "IsAvailable", "Status", "TableNumber" },
                values: new object[,]
                {
                    { 1, "Tầng 1", 4, true, 0, "Bàn 1" },
                    { 2, "Tầng 1", 4, true, 0, "Bàn 2" },
                    { 3, "Tầng 1", 6, true, 0, "Bàn 3" },
                    { 4, "Tầng 2", 4, true, 0, "Bàn 4" },
                    { 5, "Tầng 2", 6, true, 0, "Bàn 5" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "1", "1" },
                    { "2", "2" },
                    { "3", "3" },
                    { "4", "4" }
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Calories", "Carbs", "CategoryId", "Description", "DetailedDescription", "DiscountPercentage", "DisplayOrder", "Fat", "ImageUrl", "Ingredients", "IsAvailable", "IsNew", "IsOnSale", "IsPopular", "Name", "OriginalPrice", "PreparationInstructions", "Price", "Protein" },
                values: new object[,]
                {
                    { 1, null, null, 1, null, null, 0, 1, null, null, null, true, false, false, false, "Cút lộn chiên giòn chấm me", 23000m, null, 23000m, null },
                    { 2, null, null, 1, null, null, 0, 2, null, null, null, true, false, false, false, "Cút lộn chiên giòn chấm sa tế", 23000m, null, 23000m, null },
                    { 3, null, null, 1, null, null, 0, 3, null, null, null, true, false, false, false, "Cút lộn chiên giòn tương ớt", 23000m, null, 23000m, null },
                    { 4, null, null, 1, null, null, 0, 4, null, null, null, true, false, false, false, "Cút lộn chiên giòn xí muội", 23000m, null, 23000m, null },
                    { 5, null, null, 1, null, null, 0, 5, null, null, null, true, false, false, false, "Cút lộn chiên giòn mayone", 23000m, null, 23000m, null },
                    { 6, null, null, 1, null, null, 0, 6, null, null, null, true, false, false, false, "Cút lộn chiên giòn sốt bơ hành", 23000m, null, 23000m, null },
                    { 7, null, null, 1, null, null, 0, 7, null, null, null, true, false, false, false, "Cút lộn chiên giòn sốt bơ tỏi", 23000m, null, 23000m, null },
                    { 8, null, null, 1, null, null, 0, 8, null, null, null, true, false, false, false, "Cút lộn chiên giòn phô mai hành", 25000m, null, 25000m, null },
                    { 9, null, null, 1, null, null, 0, 9, null, null, null, true, false, false, false, "Cút lộn chiên giòn phô mai tỏi", 25000m, null, 25000m, null },
                    { 10, null, null, 1, null, null, 0, 10, null, null, null, true, false, false, false, "Vịt lộn chiên giòn chấm me", 24000m, null, 24000m, null },
                    { 11, null, null, 1, null, null, 0, 11, null, null, null, true, false, false, false, "Vịt lộn chiên giòn chấm sa tế", 24000m, null, 24000m, null },
                    { 12, null, null, 1, null, null, 0, 12, null, null, null, true, false, false, false, "Vịt lộn chiên giòn tương ớt", 24000m, null, 24000m, null },
                    { 13, null, null, 1, null, null, 0, 13, null, null, null, true, false, false, false, "Vịt lộn chiên giòn xí muội", 24000m, null, 24000m, null },
                    { 14, null, null, 1, null, null, 0, 14, null, null, null, true, false, false, false, "Vịt lộn chiên giòn tương cà", 24000m, null, 24000m, null },
                    { 15, null, null, 1, null, null, 0, 15, null, null, null, true, false, false, false, "Vịt lộn chiên giòn mayone", 24000m, null, 24000m, null },
                    { 16, null, null, 1, null, null, 0, 16, null, null, null, true, false, false, false, "Vịt lộn chiên giòn sốt bơ hành", 24000m, null, 24000m, null },
                    { 17, null, null, 1, null, null, 0, 17, null, null, null, true, false, false, false, "Vịt lộn chiên giòn sốt bơ tỏi", 24000m, null, 24000m, null },
                    { 18, null, null, 1, null, null, 0, 18, null, null, null, true, false, false, false, "Vịt lộn chiên giòn phô mai hành", 26000m, null, 26000m, null },
                    { 19, null, null, 1, null, null, 0, 19, null, null, null, true, false, false, false, "Vịt lộn chiên giòn phô mai tỏi", 26000m, null, 26000m, null },
                    { 20, null, null, 2, null, null, 0, 1, null, null, null, true, false, false, false, "Cút xào me", 21000m, null, 21000m, null },
                    { 21, null, null, 2, null, null, 0, 2, null, null, null, true, false, false, false, "Cút xào bơ tỏi", 21000m, null, 21000m, null },
                    { 22, null, null, 2, null, null, 0, 3, null, null, null, true, false, false, false, "Cút xào sa tế", 21000m, null, 21000m, null },
                    { 23, null, null, 2, null, null, 0, 4, null, null, null, true, false, false, false, "Cút xào phô mai", 24000m, null, 24000m, null },
                    { 24, null, null, 2, null, null, 0, 5, null, null, null, true, false, false, false, "Cút xào rau muống me", 23000m, null, 23000m, null },
                    { 25, null, null, 2, null, null, 0, 6, null, null, null, true, false, false, false, "Cút xào rau muống bơ tỏi", 23000m, null, 23000m, null },
                    { 26, null, null, 2, null, null, 0, 7, null, null, null, true, false, false, false, "Cút xào rau muống sa tế", 23000m, null, 23000m, null },
                    { 27, null, null, 2, null, null, 0, 8, null, null, null, true, false, false, false, "Vịt xào me", 21000m, null, 21000m, null },
                    { 28, null, null, 2, null, null, 0, 9, null, null, null, true, false, false, false, "Vịt xào bơ tỏi", 21000m, null, 21000m, null },
                    { 29, null, null, 2, null, null, 0, 10, null, null, null, true, false, false, false, "Vịt xào sa tế", 21000m, null, 21000m, null },
                    { 30, null, null, 2, null, null, 0, 11, null, null, null, true, false, false, false, "Vịt xào phô mai", 24000m, null, 24000m, null },
                    { 31, null, null, 2, null, null, 0, 12, null, null, null, true, false, false, false, "Vịt xào rau muống me", 23000m, null, 23000m, null },
                    { 32, null, null, 2, null, null, 0, 13, null, null, null, true, false, false, false, "Vịt xào rau muống bơ tỏi", 23000m, null, 23000m, null },
                    { 33, null, null, 2, null, null, 0, 14, null, null, null, true, false, false, false, "Vịt xào rau muống sa tế", 23000m, null, 23000m, null },
                    { 34, null, null, 2, null, null, 0, 15, null, null, null, true, false, false, false, "Gà xào me", 21000m, null, 21000m, null },
                    { 35, null, null, 2, null, null, 0, 16, null, null, null, true, false, false, false, "Gà xào bơ tỏi", 21000m, null, 21000m, null },
                    { 36, null, null, 2, null, null, 0, 17, null, null, null, true, false, false, false, "Gà xào phô mai", 24000m, null, 24000m, null },
                    { 37, null, null, 2, null, null, 0, 18, null, null, null, true, false, false, false, "Gà xào rau muống me", 23000m, null, 23000m, null },
                    { 38, null, null, 2, null, null, 0, 19, null, null, null, true, false, false, false, "Gà xào rau muống bơ tỏi", 23000m, null, 23000m, null },
                    { 39, null, null, 2, null, null, 0, 20, null, null, null, true, false, false, false, "Gà xào rau muống sa tế", 23000m, null, 23000m, null },
                    { 40, null, null, 3, null, null, 0, 1, null, null, null, true, false, false, false, "Mì xào rau muống không trứng", 15000m, null, 15000m, null },
                    { 41, null, null, 3, null, null, 0, 2, null, null, null, true, false, false, false, "Mì xào rau muống cút lộn", 32000m, null, 32000m, null },
                    { 42, null, null, 3, null, null, 0, 3, null, null, null, true, false, false, false, "Mì xào rau muống vịt lộn", 32000m, null, 32000m, null },
                    { 43, null, null, 3, null, null, 0, 4, null, null, null, true, false, false, false, "Mì xào rau muống hột gà nướng", 32000m, null, 32000m, null },
                    { 44, null, null, 3, null, null, 0, 5, null, null, null, true, false, false, false, "Mì xào rau muống bò", 35000m, null, 35000m, null },
                    { 45, null, null, 3, null, null, 0, 6, null, null, null, true, false, false, false, "Nui xào rau muống bò", 35000m, null, 35000m, null },
                    { 46, null, null, 4, null, null, 0, 1, null, null, null, true, false, false, false, "Bắp xào truyền thống", 20000m, null, 20000m, null },
                    { 47, null, null, 4, null, null, 0, 2, null, null, null, true, false, false, false, "Bắp xào trứng muối", 27000m, null, 27000m, null },
                    { 48, null, null, 4, null, null, 0, 3, null, null, null, true, false, false, false, "Bắp xào phô mai", 27000m, null, 27000m, null },
                    { 49, null, null, 4, null, null, 0, 4, null, null, null, true, false, false, false, "Bắp xào Cút lộn", 30000m, null, 30000m, null },
                    { 50, null, null, 4, null, null, 0, 5, null, null, null, true, false, false, false, "Bắp xào Vịt lộn", 30000m, null, 30000m, null },
                    { 51, null, null, 4, null, null, 0, 6, null, null, null, true, false, false, false, "Bắp xào Hột gà nướng", 30000m, null, 30000m, null },
                    { 52, null, null, 5, null, null, 0, 1, null, null, null, true, false, false, false, "Cút lộn luộc", 14000m, null, 14000m, null },
                    { 53, null, null, 5, null, null, 0, 2, null, null, null, true, false, false, false, "Vịt lộn luộc", 8000m, null, 8000m, null },
                    { 54, null, null, 5, null, null, 0, 3, null, null, null, true, false, false, false, "Trứng gà nướng", 7000m, null, 7000m, null },
                    { 55, null, null, 6, null, null, 0, 1, null, null, null, true, false, false, false, "Sting", 12000m, null, 12000m, null },
                    { 56, null, null, 6, null, null, 0, 2, null, null, null, true, false, false, false, "Pepsi", 12000m, null, 12000m, null },
                    { 57, null, null, 6, null, null, 0, 3, null, null, null, true, false, false, false, "7up", 12000m, null, 12000m, null },
                    { 58, null, null, 6, null, null, 0, 4, null, null, null, true, false, false, false, "Coca", 12000m, null, 12000m, null },
                    { 59, null, null, 6, null, null, 0, 5, null, null, null, true, false, false, false, "Xá xị", 12000m, null, 12000m, null },
                    { 60, null, null, 6, null, null, 0, 6, null, null, null, true, false, false, false, "Nước suối", 8000m, null, 8000m, null },
                    { 61, null, null, 7, null, null, 0, 1, null, null, null, true, false, false, false, "Nước chấm me", 2000m, null, 2000m, null },
                    { 62, null, null, 7, null, null, 0, 2, null, null, null, true, false, false, false, "Nước chấm sa tế", 2000m, null, 2000m, null },
                    { 63, null, null, 7, null, null, 0, 3, null, null, null, true, false, false, false, "Nước chấm xí muội", 2000m, null, 2000m, null },
                    { 64, null, null, 7, null, null, 0, 4, null, null, null, true, false, false, false, "Nước chấm tương ớt", 2000m, null, 2000m, null },
                    { 65, null, null, 7, null, null, 0, 5, null, null, null, true, false, false, false, "Nước chấm mayonese", 2000m, null, 2000m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_MenuItemId",
                table: "CartItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserCartId",
                table: "CartItems",
                column: "UserCartId");

            migrationBuilder.CreateIndex(
                name: "IX_DishOrders_MenuItemId",
                table: "DishOrders",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DishOrders_OrderId",
                table: "DishOrders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CategoryId",
                table: "MenuItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_MenuItemId",
                table: "OrderDetails",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableId",
                table: "Orders",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TableId",
                table: "Reservations",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MenuItemId",
                table: "Reviews",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCarts_UserId",
                table: "UserCarts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "DishOrders");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "RestaurantInfo");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "UserCarts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Tables");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
