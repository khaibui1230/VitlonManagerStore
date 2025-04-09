using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace QuanVitLonManager.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<DishOrder> DishOrders { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<RestaurantInfo> RestaurantInfo { get; set; }
        public DbSet<UserCart> UserCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure decimal properties
            builder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);
            
            builder.Entity<MenuItem>()
                .Property(m => m.OriginalPrice)
                .HasPrecision(18, 2);

            builder.Entity<OrderDetail>()
                .Property(od => od.Price)
                .HasPrecision(18, 2);

            builder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<OrderDetail>()
                .Property(od => od.Subtotal)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<DishOrder>()
                .Property(d => d.Price)
                .HasPrecision(18, 2);
                
            builder.Entity<DishOrder>()
                .Property(d => d.TotalPrice)
                .HasPrecision(18, 2);

            // Configure text fields for PostgreSQL compatibility with Vietnamese characters
            builder.Entity<Category>()
                .Property(c => c.Name)
                .HasColumnType("nvarchar(255)");
            
            builder.Entity<Category>()
                .Property(c => c.Description)
                .HasColumnType("nvarchar(1000)");
            
            builder.Entity<Category>()
                .Property(c => c.ImageUrl)
                .HasColumnType("nvarchar(500)");

            builder.Entity<MenuItem>()
                .Property(m => m.Name)
                .HasColumnType("nvarchar(255)");
            
            builder.Entity<MenuItem>()
                .Property(m => m.Description)
                .HasColumnType("nvarchar(1000)");
            
            builder.Entity<MenuItem>()
                .Property(m => m.DetailedDescription)
                .HasColumnType("nvarchar(4000)");
            
            builder.Entity<MenuItem>()
                .Property(m => m.ImageUrl)
                .HasColumnType("nvarchar(500)");
            
            builder.Entity<MenuItem>()
                .Property(m => m.Ingredients)
                .HasColumnType("nvarchar(2000)");
            
            builder.Entity<MenuItem>()
                .Property(m => m.PreparationInstructions)
                .HasColumnType("nvarchar(4000)");

            builder.Entity<Order>()
                .Property(o => o.Notes)
                .HasColumnType("nvarchar(1000)");

            builder.Entity<OrderDetail>()
                .Property(od => od.Notes)
                .HasColumnType("nvarchar(1000)");

            builder.Entity<Reservation>()
                .Property(r => r.Notes)
                .HasColumnType("nvarchar(1000)");

            builder.Entity<Review>()
                .Property(r => r.Comment)
                .HasColumnType("nvarchar(2000)");

            builder.Entity<RestaurantInfo>()
                .Property(r => r.Name)
                .HasColumnType("nvarchar(255)");
            
            builder.Entity<RestaurantInfo>()
                .Property(r => r.Description)
                .HasColumnType("nvarchar(2000)");
            
            builder.Entity<RestaurantInfo>()
                .Property(r => r.Address)
                .HasColumnType("nvarchar(500)");
            
            builder.Entity<RestaurantInfo>()
                .Property(r => r.Phone)
                .HasColumnType("nvarchar(50)");
            
            builder.Entity<RestaurantInfo>()
                .Property(r => r.Email)
                .HasColumnType("nvarchar(100)");
            
            builder.Entity<RestaurantInfo>()
                .Property(r => r.LogoUrl)
                .HasColumnType("nvarchar(500)");
            
            builder.Entity<RestaurantInfo>()
                .Property(r => r.WelcomeMessage)
                .HasColumnType("nvarchar(1000)");
            
            builder.Entity<RestaurantInfo>()
                .Property(r => r.GoodbyeMessage)
                .HasColumnType("nvarchar(1000)");
            
            builder.Entity<RestaurantInfo>()
                .Property(r => r.TaxId)
                .HasColumnType("nvarchar(50)");

            // Configure relationships and constraints
            builder.Entity<MenuItem>()
                .HasOne(m => m.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.Table)
                .WithMany()
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.MenuItem)
                .WithMany(m => m.OrderDetails)
                .HasForeignKey(od => od.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<DishOrder>()
                .HasOne(d => d.Order)
                .WithMany(o => o.DishOrders)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<DishOrder>()
                .HasOne(d => d.MenuItem)
                .WithMany()
                .HasForeignKey(d => d.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserCart>()
                .HasOne(uc => uc.User)
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.UserCart)
                .WithMany(uc => uc.CartItems)
                .HasForeignKey(ci => ci.UserCartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.MenuItem)
                .WithMany()
                .HasForeignKey(ci => ci.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.MenuItem)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial data
            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // Seed Roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = Guid.NewGuid().ToString() },
                new IdentityRole { Id = "2", Name = "Staff", NormalizedName = "STAFF", ConcurrencyStamp = Guid.NewGuid().ToString() },
                new IdentityRole { Id = "3", Name = "Customer", NormalizedName = "CUSTOMER", ConcurrencyStamp = Guid.NewGuid().ToString() },
                new IdentityRole { Id = "4", Name = "Chef", NormalizedName = "CHEF", ConcurrencyStamp = Guid.NewGuid().ToString() }
            );

            // Seed Users
            var hasher = new PasswordHasher<ApplicationUser>();
            
            // Admin user
            var adminUser = new ApplicationUser
            {
                Id = "1",
                UserName = "admin@gmail.com",
                NormalizedUserName = "ADMIN@GMAIL.COM",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "0123456789",
                Address = "Admin Address",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");
            
            // Staff user
            var staffUser = new ApplicationUser
            {
                Id = "2",
                UserName = "staff@gmail.com",
                NormalizedUserName = "STAFF@GMAIL.COM",
                Email = "staff@gmail.com",
                NormalizedEmail = "STAFF@GMAIL.COM",
                EmailConfirmed = true,
                FirstName = "Staff",
                LastName = "User",
                PhoneNumber = "0123456788",
                Address = "Staff Address",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            staffUser.PasswordHash = hasher.HashPassword(staffUser, "Staff@123");
            
            // Customer user
            var customerUser = new ApplicationUser
            {
                Id = "3",
                UserName = "customer@gmail.com",
                NormalizedUserName = "CUSTOMER@GMAIL.COM",
                Email = "customer@gmail.com",
                NormalizedEmail = "CUSTOMER@GMAIL.COM",
                EmailConfirmed = true,
                FirstName = "Customer",
                LastName = "User",
                PhoneNumber = "0123456787",
                Address = "Customer Address",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            customerUser.PasswordHash = hasher.HashPassword(customerUser, "Customer@123");
            
            // Chef user
            var chefUser = new ApplicationUser
            {
                Id = "4",
                UserName = "chef@gmail.com",
                NormalizedUserName = "CHEF@GMAIL.COM",
                Email = "chef@gmail.com",
                NormalizedEmail = "CHEF@GMAIL.COM",
                EmailConfirmed = true,
                FirstName = "Chef",
                LastName = "User",
                PhoneNumber = "0123456786",
                Address = "Chef Address",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            chefUser.PasswordHash = hasher.HashPassword(chefUser, "Chef@123");

            builder.Entity<ApplicationUser>().HasData(adminUser, staffUser, customerUser, chefUser);

            // Seed UserRoles
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = "1", RoleId = "1" }, // Admin
                new IdentityUserRole<string> { UserId = "2", RoleId = "2" }, // Staff
                new IdentityUserRole<string> { UserId = "3", RoleId = "3" }, // Customer
                new IdentityUserRole<string> { UserId = "4", RoleId = "4" }  // Chef
            );

            // Seed RestaurantInfo
            builder.Entity<RestaurantInfo>().HasData(
                new RestaurantInfo
                {
                    Id = 1,
                    Name = "Quán Hiển vịt lộn - Cút lộn",
                    Description = "Nhà hàng chuyên phục vụ các món vịt lộn và cút lộn",
                    Address = "354 lê văn thọ, phường 11 quận gò vấp, tp hcm",
                    Phone = "0379665639",
                    Email = "quanHienvitlon@gmail.com",
                    LogoUrl = "/images/logo.png",
                    WelcomeMessage = "Chào mừng đến với Quán Vịt Lộn",
                    GoodbyeMessage = "Cảm ơn quý khách đã ghé thăm",
                    TaxId = "1234567890"
                }
            );

            // Seed Categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Món Chiên", Description = "Các món chiên giòn", DisplayOrder = 5, IsActive = true, ImageUrl = "/images/categories/mon-chien.jpg" },
                new Category { Id = 2, Name = "Món Xào", Description = "Các món xào ngon miệng", DisplayOrder = 6, IsActive = true, ImageUrl = "/images/categories/mon-xao.jpg" },
                new Category { Id = 3, Name = "Mì Xào", Description = "Mì xào các loại", DisplayOrder = 7, IsActive = true, ImageUrl = "/images/categories/mi-xao.jpg" },
                new Category { Id = 4, Name = "Bắp Xào", Description = "Các món bắp xào", DisplayOrder = 8, IsActive = true, ImageUrl = "/images/categories/bap-xao.jpg" },
                new Category { Id = 5, Name = "Món Luộc", Description = "Các món luộc", DisplayOrder = 9, IsActive = true, ImageUrl = "/images/categories/mon-luoc.jpg" },
                new Category { Id = 6, Name = "Nước giải khát", Description = "Đồ uống", DisplayOrder = 10, IsActive = true, ImageUrl = "/images/categories/nuoc.jpg" },
                new Category { Id = 7, Name = "Nước Chấm", Description = "Các loại nước chấm", DisplayOrder = 11, IsActive = true, ImageUrl = "/images/categories/nuoc-cham.jpg" }
            );

            // Seed Tables
            builder.Entity<Table>().HasData(
                new Table { 
                    Id = 1, 
                    TableNumber = "Bàn 1", 
                    Area = "Tầng 1", 
                    Status = TableStatus.Available, 
                    Capacity = 4, 
                    IsAvailable = true
                },
                new Table { 
                    Id = 2, 
                    TableNumber = "Bàn 2", 
                    Area = "Tầng 1", 
                    Status = TableStatus.Available, 
                    Capacity = 4, 
                    IsAvailable = true
                },
                new Table { 
                    Id = 3, 
                    TableNumber = "Bàn 3", 
                    Area = "Tầng 1", 
                    Status = TableStatus.Available, 
                    Capacity = 6, 
                    IsAvailable = true
                },
                new Table { 
                    Id = 4, 
                    TableNumber = "Bàn 4", 
                    Area = "Tầng 2", 
                    Status = TableStatus.Available, 
                    Capacity = 4, 
                    IsAvailable = true
                },
                new Table { 
                    Id = 5, 
                    TableNumber = "Bàn 5", 
                    Area = "Tầng 2", 
                    Status = TableStatus.Available, 
                    Capacity = 6, 
                    IsAvailable = true
                }
            );

            // Seed MenuItems
            builder.Entity<MenuItem>().HasData(
                // Món Chiên
                new MenuItem { Id = 1, Name = "Cút lộn chiên giòn chấm me", Price = 23000m, OriginalPrice = 23000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 1 },
                new MenuItem { Id = 2, Name = "Cút lộn chiên giòn chấm sa tế", Price = 23000m, OriginalPrice = 23000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 2 },
                new MenuItem { Id = 3, Name = "Cút lộn chiên giòn tương ớt", Price = 23000m, OriginalPrice = 23000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 3 },
                new MenuItem { Id = 4, Name = "Cút lộn chiên giòn xí muội", Price = 23000m, OriginalPrice = 23000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 4 },
                new MenuItem { Id = 5, Name = "Cút lộn chiên giòn mayone", Price = 23000m, OriginalPrice = 23000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 5 },
                new MenuItem { Id = 6, Name = "Cút lộn chiên giòn sốt bơ hành", Price = 23000m, OriginalPrice = 23000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 6 },
                new MenuItem { Id = 7, Name = "Cút lộn chiên giòn sốt bơ tỏi", Price = 23000m, OriginalPrice = 23000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 7 },
                new MenuItem { Id = 8, Name = "Cút lộn chiên giòn phô mai hành", Price = 25000m, OriginalPrice = 25000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 8 },
                new MenuItem { Id = 9, Name = "Cút lộn chiên giòn phô mai tỏi", Price = 25000m, OriginalPrice = 25000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 9 },
                new MenuItem { Id = 10, Name = "Vịt lộn chiên giòn chấm me", Price = 24000m, OriginalPrice = 24000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 10 },
                new MenuItem { Id = 11, Name = "Vịt lộn chiên giòn chấm sa tế", Price = 24000m, OriginalPrice = 24000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 11 },
                new MenuItem { Id = 12, Name = "Vịt lộn chiên giòn tương ớt", Price = 24000m, OriginalPrice = 24000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 12 },
                new MenuItem { Id = 13, Name = "Vịt lộn chiên giòn xí muội", Price = 24000m, OriginalPrice = 24000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 13 },
                new MenuItem { Id = 14, Name = "Vịt lộn chiên giòn tương cà", Price = 24000m, OriginalPrice = 24000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 14 },
                new MenuItem { Id = 15, Name = "Vịt lộn chiên giòn mayone", Price = 24000m, OriginalPrice = 24000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 15 },
                new MenuItem { Id = 16, Name = "Vịt lộn chiên giòn sốt bơ hành", Price = 24000m, OriginalPrice = 24000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 16 },
                new MenuItem { Id = 17, Name = "Vịt lộn chiên giòn sốt bơ tỏi", Price = 24000m, OriginalPrice = 24000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 17 },
                new MenuItem { Id = 18, Name = "Vịt lộn chiên giòn phô mai hành", Price = 26000m, OriginalPrice = 26000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 18 },
                new MenuItem { Id = 19, Name = "Vịt lộn chiên giòn phô mai tỏi", Price = 26000m, OriginalPrice = 26000m, CategoryId = 1, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 19 },
                // Món Xào
                new MenuItem { Id = 20, Name = "Cút xào me", Price = 21000m, OriginalPrice = 21000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 1 },
                new MenuItem { Id = 21, Name = "Cút xào bơ tỏi", Price = 21000m, OriginalPrice = 21000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 2 },
                new MenuItem { Id = 22, Name = "Cút xào sa tế", Price = 21000m, OriginalPrice = 21000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 3 },
                new MenuItem { Id = 23, Name = "Cút xào phô mai", Price = 24000m, OriginalPrice = 24000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 4 },
                new MenuItem { Id = 24, Name = "Cút xào rau muống me", Price = 23000m, OriginalPrice = 23000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 5 },
                new MenuItem { Id = 25, Name = "Cút xào rau muống bơ tỏi", Price = 23000m, OriginalPrice = 23000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 6 },
                new MenuItem { Id = 26, Name = "Cút xào rau muống sa tế", Price = 23000m, OriginalPrice = 23000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 7 },
                new MenuItem { Id = 27, Name = "Vịt xào me", Price = 21000m, OriginalPrice = 21000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 8 },
                new MenuItem { Id = 28, Name = "Vịt xào bơ tỏi", Price = 21000m, OriginalPrice = 21000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 9 },
                new MenuItem { Id = 29, Name = "Vịt xào sa tế", Price = 21000m, OriginalPrice = 21000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 10 },
                new MenuItem { Id = 30, Name = "Vịt xào phô mai", Price = 24000m, OriginalPrice = 24000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 11 },
                new MenuItem { Id = 31, Name = "Vịt xào rau muống me", Price = 23000m, OriginalPrice = 23000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 12 },
                new MenuItem { Id = 32, Name = "Vịt xào rau muống bơ tỏi", Price = 23000m, OriginalPrice = 23000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 13 },
                new MenuItem { Id = 33, Name = "Vịt xào rau muống sa tế", Price = 23000m, OriginalPrice = 23000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 14 },
                new MenuItem { Id = 34, Name = "Gà xào me", Price = 21000m, OriginalPrice = 21000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 15 },
                new MenuItem { Id = 35, Name = "Gà xào bơ tỏi", Price = 21000m, OriginalPrice = 21000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 16 },
                new MenuItem { Id = 36, Name = "Gà xào phô mai", Price = 24000m, OriginalPrice = 24000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 17 },
                new MenuItem { Id = 37, Name = "Gà xào rau muống me", Price = 23000m, OriginalPrice = 23000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 18 },
                new MenuItem { Id = 38, Name = "Gà xào rau muống bơ tỏi", Price = 23000m, OriginalPrice = 23000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 19 },
                new MenuItem { Id = 39, Name = "Gà xào rau muống sa tế", Price = 23000m, OriginalPrice = 23000m, CategoryId = 2, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 20 },
                // Mì Xào
                new MenuItem { Id = 40, Name = "Mì xào rau muống không trứng", Price = 15000m, OriginalPrice = 15000m, CategoryId = 3, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 1 },
                new MenuItem { Id = 41, Name = "Mì xào rau muống cút lộn", Price = 32000m, OriginalPrice = 32000m, CategoryId = 3, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 2 },
                new MenuItem { Id = 42, Name = "Mì xào rau muống vịt lộn", Price = 32000m, OriginalPrice = 32000m, CategoryId = 3, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 3 },
                new MenuItem { Id = 43, Name = "Mì xào rau muống hột gà nướng", Price = 32000m, OriginalPrice = 32000m, CategoryId = 3, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 4 },
                new MenuItem { Id = 44, Name = "Mì xào rau muống bò", Price = 35000m, OriginalPrice = 35000m, CategoryId = 3, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 5 },
                new MenuItem { Id = 45, Name = "Nui xào rau muống bò", Price = 35000m, OriginalPrice = 35000m, CategoryId = 3, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 6 },
                // Bắp Xào
                new MenuItem { Id = 46, Name = "Bắp xào truyền thống", Price = 20000m, OriginalPrice = 20000m, CategoryId = 4, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 1 },
                new MenuItem { Id = 47, Name = "Bắp xào trứng muối", Price = 27000m, OriginalPrice = 27000m, CategoryId = 4, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 2 },
                new MenuItem { Id = 48, Name = "Bắp xào phô mai", Price = 27000m, OriginalPrice = 27000m, CategoryId = 4, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 3 },
                new MenuItem { Id = 49, Name = "Bắp xào Cút lộn", Price = 30000m, OriginalPrice = 30000m, CategoryId = 4, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 4 },
                new MenuItem { Id = 50, Name = "Bắp xào Vịt lộn", Price = 30000m, OriginalPrice = 30000m, CategoryId = 4, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 5 },
                new MenuItem { Id = 51, Name = "Bắp xào Hột gà nướng", Price = 30000m, OriginalPrice = 30000m, CategoryId = 4, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 6 },
                // Món Luộc
                new MenuItem { Id = 52, Name = "Cút lộn luộc", Price = 14000m, OriginalPrice = 14000m, CategoryId = 5, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 1 },
                new MenuItem { Id = 53, Name = "Vịt lộn luộc", Price = 8000m, OriginalPrice = 8000m, CategoryId = 5, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 2 },
                new MenuItem { Id = 54, Name = "Trứng gà nướng", Price = 7000m, OriginalPrice = 7000m, CategoryId = 5, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 3 },
                // Nước
                new MenuItem { Id = 55, Name = "Sting", Price = 12000m, OriginalPrice = 12000m, CategoryId = 6, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 1 },
                new MenuItem { Id = 56, Name = "Pepsi", Price = 12000m, OriginalPrice = 12000m, CategoryId = 6, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 2 },
                new MenuItem { Id = 57, Name = "7up", Price = 12000m, OriginalPrice = 12000m, CategoryId = 6, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 3 },
                new MenuItem { Id = 58, Name = "Coca", Price = 12000m, OriginalPrice = 12000m, CategoryId = 6, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 4 },
                new MenuItem { Id = 59, Name = "Xá xị", Price = 12000m, OriginalPrice = 12000m, CategoryId = 6, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 5 },
                new MenuItem { Id = 60, Name = "Nước suối", Price = 8000m, OriginalPrice = 8000m, CategoryId = 6, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 6 },
                // Nước Chấm
                new MenuItem { Id = 61, Name = "Nước chấm me", Price = 2000m, OriginalPrice = 2000m, CategoryId = 7, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 1 },
                new MenuItem { Id = 62, Name = "Nước chấm sa tế", Price = 2000m, OriginalPrice = 2000m, CategoryId = 7, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 2 },
                new MenuItem { Id = 63, Name = "Nước chấm xí muội", Price = 2000m, OriginalPrice = 2000m, CategoryId = 7, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 3 },
                new MenuItem { Id = 64, Name = "Nước chấm tương ớt", Price = 2000m, OriginalPrice = 2000m, CategoryId = 7, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 4 },
                new MenuItem { Id = 65, Name = "Nước chấm mayonese", Price = 2000m, OriginalPrice = 2000m, CategoryId = 7, DiscountPercentage = 0, IsNew = false, IsPopular = false, IsOnSale = false, IsAvailable = true, DisplayOrder = 5 }
            );
        }
    }
}