using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanVitLonManager.Controllers
{
    [Authorize(Roles = "Admin,Staff,Chef")]
    public class ChefController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChefController> _logger;

        public ChefController(ApplicationDbContext context, ILogger<ChefController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string filterStatus = "pending")
        {
            _logger.LogInformation("Loading chef dashboard");
            
            // Xác định vai trò người dùng
            bool isChef = User.IsInRole("Chef");
            ViewBag.IsChef = isChef;
            
            // Get dish orders based on filter status
            var query = _context.DishOrders
                .Include(d => d.MenuItem)
                .Include(d => d.Order)
                .AsQueryable();

            // Apply filter
            if (filterStatus == "completed")
            {
                query = query.Where(d => d.Status == DishOrderStatus.Completed);
            }
            else if (filterStatus == "all")
            {
                // No filter - get all
            }
            else // Default: pending
            {
                query = query.Where(d => d.Status == DishOrderStatus.Pending || d.Status == DishOrderStatus.Preparing);
                filterStatus = "pending"; // Ensure correct value
            }

            var dishOrders = await query
                .OrderByDescending(d => d.OrderTime) // Sắp xếp theo thời gian đặt món mới nhất lên đầu
                .ToListAsync();

            _logger.LogInformation($"Found {dishOrders.Count} dishes with filter: {filterStatus}");
            
            // Group dishes by name, notes and status
            var groupedDishes = dishOrders
                .GroupBy(d => new { d.MenuItemId, d.Name, d.Notes, d.Status })
                .Select(g => new DishItemViewModel
                {
                    DishId = g.Key.MenuItemId ?? 0,
                    DishName = g.Key.Name,
                    TotalQuantity = g.Sum(d => d.Quantity),
                    Notes = g.Where(d => !string.IsNullOrEmpty(d.Notes))
                            .Select(d => d.Notes)
                            .Distinct()
                            .ToList(),
                    Status = g.Key.Status.ToString().ToLower(),
                    OrderIds = g.Where(d => d.OrderId.HasValue)
                              .Select(d => d.OrderId.Value)
                              .ToList(),
                    DishOrderIds = g.Select(d => d.Id).ToList(),
                    OrderTime = g.Max(d => d.OrderTime) // Lấy thời gian đặt hàng mới nhất trong nhóm
                })
                .OrderByDescending(d => d.Status == "pending") // Ưu tiên món đang chờ
                .ThenByDescending(d => d.OrderTime) // Sắp xếp theo thời gian mới nhất
                .ThenBy(d => d.DishName) // Sau đó sắp xếp theo tên món
                .ToList();

            _logger.LogInformation($"Grouped into {groupedDishes.Count} unique dish items");
            
            // Debug: log dish items
            foreach (var item in groupedDishes)
            {
                _logger.LogInformation($"Dish: {item.DishName}, Status: {item.Status}, Quantity: {item.TotalQuantity}, OrderTime: {item.OrderTime}");
            }

            var viewModel = new ChefDashboardViewModel
            {
                DishItems = groupedDishes,
                FilterStatus = filterStatus,
                LastRefreshTime = DateTime.Now
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAllStatus(int dishId, string status)
        {
            if (string.IsNullOrEmpty(status) || !Enum.TryParse(status, out DishOrderStatus statusEnum))
            {
                TempData["Error"] = "Trạng thái không hợp lệ";
                return RedirectToAction(nameof(Index));
            }

            var dishOrders = await _context.DishOrders
                .Where(d => d.MenuItemId == dishId || (dishId == 0 && d.Id == dishId))
                .ToListAsync();
                
            if (!dishOrders.Any())
            {
                TempData["Error"] = "Không tìm thấy món ăn";
                return RedirectToAction(nameof(Index));
            }

            foreach (var dish in dishOrders)
            {
                dish.Status = statusEnum;
                _context.Update(dish);
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Updated {dishOrders.Count} dishes with ID {dishId} to status {status}");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDishNoteStatus(int menuItemId, string notes, string status)
        {
            try
            {
                if (string.IsNullOrEmpty(status) || !Enum.TryParse(status, out DishOrderStatus statusEnum))
                {
                    return Json(new { success = false, message = "Trạng thái không hợp lệ" });
                }

                // Xử lý cả trường hợp notes là null hoặc chuỗi rỗng
                var dishOrders = await _context.DishOrders
                    .Where(d => d.MenuItemId == menuItemId && 
                               (string.IsNullOrEmpty(notes) ? string.IsNullOrEmpty(d.Notes) : d.Notes == notes))
                    .ToListAsync();
                    
                if (!dishOrders.Any())
                {
                    return Json(new { success = false, message = "Không tìm thấy món ăn phù hợp" });
                }

                foreach (var dish in dishOrders)
                {
                    dish.Status = statusEnum;
                    _context.Update(dish);
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Updated {dishOrders.Count} dishes with MenuItemId {menuItemId} and Notes '{notes ?? "Empty"}' to status {status}");

                return Json(new { 
                    success = true, 
                    message = $"Đã cập nhật {dishOrders.Count} món thành công",
                    dishCount = dishOrders.Count,
                    newStatus = status.ToLower()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dish status");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái món ăn" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int dishOrderId, string status)
        {
            if (string.IsNullOrEmpty(status) || !Enum.TryParse(status, out DishOrderStatus statusEnum))
            {
                return Json(new { success = false, message = "Trạng thái không hợp lệ" });
            }

            var dishOrder = await _context.DishOrders.FindAsync(dishOrderId);
            if (dishOrder == null)
            {
                return Json(new { success = false, message = "Không tìm thấy món ăn" });
            }

            dishOrder.Status = statusEnum;
            _context.Update(dishOrder);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Updated single dish order #{dishOrderId} to status {status}");

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingOrders()
        {
            var pendingOrders = await _context.DishOrders
                .Include(o => o.Order)
                .Where(o => o.Status == DishOrderStatus.Pending || o.Status == DishOrderStatus.Preparing)
                .OrderBy(o => o.OrderTime)
                .ToListAsync();
            
            return PartialView("_PendingOrdersList", pendingOrders);
        }
    }
}