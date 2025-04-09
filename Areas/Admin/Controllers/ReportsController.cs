using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using QuanVitLonManager.Areas.Admin.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanVitLonManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]  
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Revenue(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Ngày bắt đầu mặc định là đầu tháng
            startDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            // Ngày kết thúc mặc định là hôm nay
            endDate ??= DateTime.Now;

            var revenueReport = new RevenueReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value
            };

            // Báo cáo doanh thu theo ngày
            revenueReport.DailyRevenue = await _context.Orders
                .Where(o => o.Status == Models.OrderStatus.Completed && 
                           o.OrderDate.Date >= startDate.Value.Date && 
                           o.OrderDate.Date <= endDate.Value.Date)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailyRevenueViewModel
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Tổng doanh thu
            revenueReport.TotalRevenue = revenueReport.DailyRevenue.Sum(d => d.Revenue);
            revenueReport.TotalOrders = revenueReport.DailyRevenue.Sum(d => d.OrderCount);

            // Báo cáo doanh thu theo danh mục
            revenueReport.CategoryRevenue = await _context.OrderDetails
                .Include(od => od.MenuItem)
                .ThenInclude(m => m.Category)
                .Where(od => od.Order.Status == Models.OrderStatus.Completed && 
                           od.Order.OrderDate.Date >= startDate.Value.Date && 
                           od.Order.OrderDate.Date <= endDate.Value.Date)
                .GroupBy(od => od.MenuItem.Category.Name)
                .Select(g => new CategoryRevenueViewModel
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(od => od.UnitPrice * od.Quantity),
                    ItemCount = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            return View(revenueReport);
        }

        public async Task<IActionResult> Products()
        {
            var startDate = DateTime.Now.AddDays(-30);
            var endDate = DateTime.Now;

            var productsReport = new ProductsReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            productsReport.TopSellingItems = await _context.OrderDetails
                .Include(od => od.MenuItem)
                .Where(od => od.Order.Status == Models.OrderStatus.Completed &&
                           od.Order.OrderDate >= startDate &&
                           od.Order.OrderDate <= endDate)
                .GroupBy(od => new { od.MenuItemId, od.MenuItem.Name })
                .Select(g => new TopSellingItemViewModel
                {
                    MenuItemId = g.Key.MenuItemId,
                    MenuItemName = g.Key.Name,
                    TotalQuantity = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.UnitPrice * od.Quantity),
                    AveragePrice = g.Average(od => od.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToListAsync();

            return View(productsReport);
        }

        public async Task<IActionResult> OrderStatus()
        {
            var startDate = DateTime.Now.AddDays(-30);
            var endDate = DateTime.Now;

            var orderStatusReport = new OrderStatusReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            orderStatusReport.OrderStatusData = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusViewModel
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(s => s.Count)
                .ToListAsync();

            return View(orderStatusReport);
        }

        public IActionResult Customers(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.Date.AddDays(-30);
            endDate ??= DateTime.Now.Date;

            var model = new CustomersReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value
            };

            var query = _context.Orders
                .Include(o => o.User)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

            var customerData = query
                .GroupBy(o => new { o.UserId, o.User.FirstName, o.User.LastName, o.User.PhoneNumber })
                .Select(g => new CustomerReportItem
                {
                    CustomerId = g.Key.UserId,
                    CustomerName = $"{g.Key.FirstName} {g.Key.LastName}",
                    PhoneNumber = g.Key.PhoneNumber,
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            model.Customers = customerData;
            model.TotalCustomers = customerData.Count;
            model.TotalOrders = customerData.Sum(c => c.OrderCount);
            model.TotalRevenue = customerData.Sum(c => c.TotalRevenue);

            return View(model);
        }
    }
} 