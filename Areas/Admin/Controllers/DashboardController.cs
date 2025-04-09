using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace QuanVitLonManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Log user information
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation($"User {user.Email} has roles: {string.Join(", ", roles)}");
            }
            else
            {
                _logger.LogWarning("No user found");
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Check if user is in Admin role
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning($"User {user.Email} attempted to access admin area without Admin role");
                return Forbid();
            }

            // Tạo ViewModel cho dashboard
            var dashboardViewModel = new AdminDashboardViewModel
            {
                // Số lượng món ăn
                TotalMenuItems = await _context.MenuItems.CountAsync(),
                
                // Số lượng người dùng (không bao gồm admin)
                TotalUsers = await _context.Users.CountAsync(),
                
                // Số lượng đơn hàng
                TotalOrders = await _context.Orders.CountAsync(),
                
                // Tổng doanh thu
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .SumAsync(o => o.TotalAmount),
                
                // Đơn hàng mới nhất
                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync(),
                
                // Món ăn bán chạy nhất
                TopSellingItems = await _context.OrderDetails
                    .Include(od => od.MenuItem)
                    .GroupBy(od => od.MenuItemId)
                    .Select(g => new TopSellingItemViewModel
                    {
                        MenuItemId = g.Key,
                        MenuItemName = g.First().MenuItem.Name,
                        TotalQuantity = g.Sum(od => od.Quantity),
                        TotalRevenue = g.Sum(od => od.Price * od.Quantity)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(5)
                    .ToListAsync(),
                
                // Doanh thu theo ngày trong 7 ngày gần nhất
                DailyRevenue = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Completed && o.OrderDate >= DateTime.Now.AddDays(-7))
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailyRevenueViewModel
                    {
                        Date = g.Key,
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync(),
                
                // Adding required properties
                MenuItemName = "Default Item",
                Quantity = 0,
                TotalAmount = 0
            };
            
            return View(dashboardViewModel);
        }
    }
} 