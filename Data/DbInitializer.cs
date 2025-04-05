using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Models;

namespace QuanVitLonManager.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Check if roles exist
            if (!await roleManager.RoleExistsAsync("QuanLy"))
            {
                await roleManager.CreateAsync(new IdentityRole("QuanLy"));
            }
            if (!await roleManager.RoleExistsAsync("NhanVien"))
            {
                await roleManager.CreateAsync(new IdentityRole("NhanVien"));
            }
            if (!await roleManager.RoleExistsAsync("Chef"))
            {
                await roleManager.CreateAsync(new IdentityRole("Chef"));
            }
            if (!await roleManager.RoleExistsAsync("KhachHang"))
            {
                await roleManager.CreateAsync(new IdentityRole("KhachHang"));
            }

            // Check if admin user exists
            if (!context.Users.Any())
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@quanvitlon.com",
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User"
                };

                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "QuanLy");
                
                var chefUser = new ApplicationUser
                {
                    UserName = "chef",
                    Email = "chef@quanvitlon.com",
                    EmailConfirmed = true,
                    FirstName = "Đầu Bếp",
                    LastName = "Trưởng"
                };

                await userManager.CreateAsync(chefUser, "Chef@123");
                await userManager.AddToRoleAsync(chefUser, "Chef");
            }

            // Add categories if none exist
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Món Chiên", Description = "Các món chiên giòn", DisplayOrder = 5, IsActive = true, ImageUrl = "/images/categories/mon-chien.jpg" },
                    new Category { Name = "Món Xào", Description = "Các món xào ngon miệng", DisplayOrder = 6, IsActive = true, ImageUrl = "/images/categories/mon-xao.jpg" },
                    new Category { Name = "Mì Xào", Description = "Mì xào các loại", DisplayOrder = 7, IsActive = true, ImageUrl = "/images/categories/mi-xao.jpg" },
                    new Category { Name = "Bắp Xào", Description = "Các món bắp xào", DisplayOrder = 8, IsActive = true, ImageUrl = "/images/categories/bap-xao.jpg" },
                    new Category { Name = "Món Luộc", Description = "Các món luộc", DisplayOrder = 9, IsActive = true, ImageUrl = "/images/categories/mon-luoc.jpg" },
                    new Category { Name = "Nước giải khát", Description = "Đồ uống", DisplayOrder = 10, IsActive = true, ImageUrl = "/images/categories/nuoc.jpg" },
                    new Category { Name = "Nước Chấm", Description = "Các loại nước chấm", DisplayOrder = 11, IsActive = true, ImageUrl = "/images/categories/nuoc-cham.jpg" }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Add menu items for each category if none exist
            if (!context.MenuItems.Any())
            {
                var monChien = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Món Chiên");
                var monXao = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Món Xào");
                var miXao = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Mì Xào");
                var bapXao = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Bắp Xào");
                var monLuoc = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Món Luộc");
                var nuoc = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Nước");
                var nuocCham = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Nước Chấm");

                var menuItems = new List<MenuItem>();

                // Món Chiên
                if (monChien != null)
                {
                    menuItems.AddRange(new[]
                    {
                        // Món Chiên
                    new MenuItem { Name = "Cút lộn chiên giòn chấm me", Price = 23000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 1 },
                    new MenuItem { Name = "Cút lộn chiên giòn chấm sa tế", Price = 23000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 2 },
                    new MenuItem { Name = "Cút lộn chiên giòn tương ớt", Price = 23000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 3 },
                    new MenuItem { Name = "Cút lộn chiên giòn xí muội", Price = 23000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 4 },
                    new MenuItem { Name = "Cút lộn chiên giòn mayone", Price = 23000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 5 },
                    new MenuItem { Name = "Cút lộn chiên giòn sốt bơ hành", Price = 23000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 6 },
                    new MenuItem { Name = "Cút lộn chiên giòn sốt bơ tỏi", Price = 23000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 7 },
                    new MenuItem { Name = "Cút lộn chiên giòn phô mai hành", Price = 25000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 8 },
                    new MenuItem { Name = "Cút lộn chiên giòn phô mai tỏi", Price = 25000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 9 },
                    new MenuItem { Name = "Vịt lộn chiên giòn chấm me", Price = 24000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 10 },
                    new MenuItem { Name = "Vịt lộn chiên giòn chấm sa tế", Price = 24000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 11 },
                    new MenuItem { Name = "Vịt lộn chiên giòn tương ớt", Price = 24000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 12 },
                    new MenuItem { Name = "Vịt lộn chiên giòn xí muội", Price = 24000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 13 },
                    new MenuItem { Name = "Vịt lộn chiên giòn tương cà", Price = 24000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 14 },
                    new MenuItem { Name = "Vịt lộn chiên giòn mayone", Price = 24000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 15 },
                    new MenuItem { Name = "Vịt lộn chiên giòn sốt bơ hành", Price = 24000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 16 },
                    new MenuItem { Name = "Vịt lộn chiên giòn sốt bơ tỏi", Price = 24000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 17 },
                    new MenuItem { Name = "Vịt lộn chiên giòn phô mai hành", Price = 26000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 18 },
                    new MenuItem { Name = "Vịt lộn chiên giòn phô mai tỏi", Price = 26000, Category = monChien, CategoryId = monChien.Id, IsAvailable = true, DisplayOrder = 19 },
                    });
                }

                // Món Xào
                if (monXao != null)
                {
                    menuItems.AddRange(new[]
                    {
                        new MenuItem { Name = "Cút xào me", Price = 21000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 1 },
                        new MenuItem { Name = "Cút xào bơ tỏi", Price = 21000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 2 },
                        new MenuItem { Name = "Cút xào sa tế", Price = 21000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 3 },
                        new MenuItem { Name = "Cút xào phô mai", Price = 24000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 4 },
                        new MenuItem { Name = "Cút xào rau muống me", Price = 23000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 5 },
                        new MenuItem { Name = "Cút xào rau muống bơ tỏi", Price = 23000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 6 },
                        new MenuItem { Name = "Cút xào rau muống sa tế", Price = 23000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 7 },
                        new MenuItem { Name = "Vịt xào me", Price = 21000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 8 },
                        new MenuItem { Name = "Vịt xào bơ tỏi", Price = 21000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 9 },
                        new MenuItem { Name = "Vịt xào sa tế", Price = 21000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 10 },
                        new MenuItem { Name = "Vịt xào phô mai", Price = 24000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 11 },
                        new MenuItem { Name = "Vịt xào rau muống me", Price = 23000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 12 },
                        new MenuItem { Name = "Vịt xào rau muống bơ tỏi", Price = 23000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 13 },
                        new MenuItem { Name = "Vịt xào rau muống sa tế", Price = 23000, Category = monXao, CategoryId = monXao.Id, IsAvailable = true, DisplayOrder = 14 },
                    });
                }

                // Mì Xào
                if (miXao != null)
                {
                    menuItems.AddRange(new[]
                    {
                        // Mì Xào
                        new MenuItem { Name = "Mì xào không trứng", Price = 15000, Category = miXao, CategoryId = miXao.Id, IsAvailable = true, DisplayOrder = 1 },
                        new MenuItem { Name = "Mì xào cút lộn", Price = 32000, Category = miXao, CategoryId = miXao.Id, IsAvailable = true, DisplayOrder = 2 },
                        new MenuItem { Name = "Mì xào vịt lộn", Price = 32000, Category = miXao, CategoryId = miXao.Id, IsAvailable = true, DisplayOrder = 3 },
                        new MenuItem { Name = "Mì xào hột gà nướng", Price = 32000, Category = miXao, CategoryId = miXao.Id, IsAvailable = true, DisplayOrder = 4 },
                        new MenuItem { Name = "Mì xào bò", Price = 35000, Category = miXao, CategoryId = miXao.Id, IsAvailable = true, DisplayOrder = 5 },
                    });
                }

                // Additional categories and items can be added here following the same format...
                if (bapXao != null)
                {
                    menuItems.AddRange(new[]
                    {
                        // Bắp Xào
                        new MenuItem { Name = "Bắp xào truyền thống", Price = 20000, Category = bapXao, CategoryId = bapXao.Id, IsAvailable = true, DisplayOrder = 1 },
                        new MenuItem { Name = "Bắp xào trứng muối", Price = 27000, Category = bapXao, CategoryId = bapXao.Id, IsAvailable = true, DisplayOrder = 2 },
                        new MenuItem { Name = "Bắp xào phô mai", Price = 27000, Category = bapXao, CategoryId = bapXao.Id, IsAvailable = true, DisplayOrder = 3 },
                        new MenuItem { Name = "Bắp xào Cút lộn", Price = 30000, Category = bapXao, CategoryId = bapXao.Id, IsAvailable = true, DisplayOrder = 4 },
                        new MenuItem { Name = "Bắp xào Vịt lộn", Price = 30000, Category = bapXao, CategoryId = bapXao.Id, IsAvailable = true, DisplayOrder = 5 },
                        new MenuItem { Name = "Bắp xào Hột gà nướng", Price = 30000, Category = bapXao, CategoryId = bapXao.Id, IsAvailable = true, DisplayOrder = 6 },
                    });
                }
                // Món Luộc

                if (monLuoc != null)
                {
                    menuItems.AddRange(new[]
                    {
                        new MenuItem { Name = "Cút lộn luộc", Price = 14000, Category = monLuoc, CategoryId = monLuoc.Id, IsAvailable = true, DisplayOrder = 1 },
                        new MenuItem { Name = "Vịt lộn luộc", Price = 8000, Category = monLuoc, CategoryId = monLuoc.Id, IsAvailable = true, DisplayOrder = 2 },
                        new MenuItem { Name = "Trứng gà nướng", Price = 7000, Category = monLuoc, CategoryId = monLuoc.Id, IsAvailable = true, DisplayOrder = 3 }
                    });
                }

                // Nước 
                if (nuoc != null)
                {
                    menuItems.AddRange(new[]
                    {
                            new MenuItem { Name = "Sting", Price = 12000, Category = nuoc, CategoryId = nuoc.Id, IsAvailable = true, DisplayOrder = 1 },
                            new MenuItem { Name = "Pepsi", Price = 12000, Category = nuoc, CategoryId = nuoc.Id, IsAvailable = true, DisplayOrder = 2 },
                            new MenuItem { Name = "7up", Price = 12000, Category = nuoc, CategoryId = nuoc.Id, IsAvailable = true, DisplayOrder = 3 },
                            new MenuItem { Name = "Coca", Price = 12000, Category = nuoc, CategoryId = nuoc.Id, IsAvailable = true, DisplayOrder = 4 },
                            new MenuItem { Name = "Xá xị", Price = 12000, Category = nuoc, CategoryId = nuoc.Id, IsAvailable = true, DisplayOrder = 5 },
                            new MenuItem { Name = "Nước suối", Price = 8000, Category = nuoc, CategoryId = nuoc.Id, IsAvailable = true, DisplayOrder = 6 },
                    });
                }
                // Nước Chấm
                if (nuocCham != null)
                {
                    menuItems.AddRange(new[]
                    {
                       // Nước Chấm
                        new MenuItem { Name = "Nước chấm me", Price = 2000, Category = nuocCham, CategoryId = nuocCham.Id, IsAvailable = true, DisplayOrder = 1 },
                        new MenuItem { Name = "Nước chấm sa tế", Price = 2000, Category = nuocCham, CategoryId = nuocCham.Id, IsAvailable = true, DisplayOrder = 2 },
                        new MenuItem { Name = "Nước chấm xí muội", Price = 2000, Category = nuocCham, CategoryId = nuocCham.Id, IsAvailable = true, DisplayOrder = 3 },
                        new MenuItem { Name = "Nước chấm tương ớt", Price = 2000, Category = nuocCham, CategoryId = nuocCham.Id, IsAvailable = true, DisplayOrder = 4 },
                        new MenuItem { Name = "Nước chấm mayonese", Price = 2000, Category = nuocCham, CategoryId = nuocCham.Id, IsAvailable = true, DisplayOrder = 5 },
                    });
                }
                context.MenuItems.AddRange(menuItems);
                await context.SaveChangesAsync();
            }
        }
    }
}
