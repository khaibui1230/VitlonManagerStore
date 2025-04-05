using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace QuanVitLonManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanLy")]
    public class OrderManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status = null, string? searchTerm = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                .Include(o => o.DishOrders)
                .AsQueryable();

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    query = query.Where(o => o.Status == orderStatus);
                }
            }

            // Tìm kiếm theo ID hoặc tên khách hàng
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o => 
                    o.Id.ToString().Contains(searchTerm) ||
                    (o.CustomerName != null && o.CustomerName.Contains(searchTerm)) ||
                    (o.PhoneNumber != null && o.PhoneNumber.Contains(searchTerm))
                );
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var viewModel = new OrderManagementViewModel
            {
                Orders = orders,
                CurrentStatus = status ?? string.Empty,
                SearchTerm = searchTerm ?? string.Empty,
                StatusList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Tất cả", Value = "" },
                    new SelectListItem { Text = "Chờ xử lý", Value = OrderStatus.Pending.ToString(), Selected = status == OrderStatus.Pending.ToString() },
                    new SelectListItem { Text = "Đã xác nhận", Value = OrderStatus.Confirmed.ToString(), Selected = status == OrderStatus.Confirmed.ToString() },
                    new SelectListItem { Text = "Đang chuẩn bị", Value = OrderStatus.Preparing.ToString(), Selected = status == OrderStatus.Preparing.ToString() },
                    new SelectListItem { Text = "Đang giao", Value = OrderStatus.Delivering.ToString(), Selected = status == OrderStatus.Delivering.ToString() },
                    new SelectListItem { Text = "Hoàn thành", Value = OrderStatus.Completed.ToString(), Selected = status == OrderStatus.Completed.ToString() },
                    new SelectListItem { Text = "Đã hủy", Value = OrderStatus.Cancelled.ToString(), Selected = status == OrderStatus.Cancelled.ToString() }
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string newStatus)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                if (!Enum.TryParse<OrderStatus>(newStatus, true, out var orderStatus))
                {
                    return Json(new { success = false, message = "Trạng thái không hợp lệ." });
                }

                order.Status = orderStatus;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã cập nhật trạng thái đơn hàng." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
} 