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

namespace QuanVitLonManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanLy")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
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
                    .ToListAsync()
            };
            
            return View(dashboardViewModel);
        }
    }
} 