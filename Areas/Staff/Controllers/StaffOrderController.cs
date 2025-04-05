using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using QuanVitLonManager.Hubs;

namespace QuanVitLonManager.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "QuanLy,NhanVien")]
    public class StaffOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StaffOrderController> _logger;

        public StaffOrderController(ApplicationDbContext context, ILogger<StaffOrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Staff/StaffOrder/NewOrders
        public async Task<IActionResult> NewOrders()
        {
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);
            
            var newOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MenuItem)
                .Include(o => o.DishOrders)
                .Where(o => o.OrderDate >= todayStart && o.OrderDate < todayEnd && 
                      (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.Title = "Đơn hàng mới";
            return View("Orders", newOrders);
        }

        // GET: Staff/StaffOrder/Orders
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MenuItem)
                .Include(o => o.DishOrders)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Staff/StaffOrder/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MenuItem)
                .Include(o => o.DishOrders)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Staff/StaffOrder/UpdatePaymentStatus
        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(int id, PaymentStatus status, PaymentMethod method)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            try
            {
                // Cập nhật thông tin thanh toán
                order.PaymentStatus = status;
                order.PaymentMethod = method;
                
                // Nếu thanh toán đã xong thì cập nhật ngày thanh toán
                if (status == PaymentStatus.Paid)
                {
                    order.PaymentDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Đã cập nhật trạng thái thanh toán đơn hàng #{id} thành {status}");

                return Json(new { 
                    success = true, 
                    paymentStatus = status.ToString(),
                    paymentDate = order.PaymentDate?.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật trạng thái thanh toán đơn hàng #{id}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Staff/StaffOrder/UpdatePayment
        [HttpPost]
        public async Task<IActionResult> UpdatePayment(int orderId, PaymentMethod paymentMethod)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            try
            {
                // Cập nhật thông tin thanh toán
                order.PaymentMethod = paymentMethod;
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaymentDate = DateTime.Now;
                
                // Cập nhật trạng thái đơn hàng thành Đã hoàn thành nếu đang ở trạng thái Billing
                if (order.Status == OrderStatus.Billing)
                {
                    order.Status = OrderStatus.Completed;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Đã cập nhật thanh toán đơn hàng #{orderId}, phương thức: {paymentMethod}");

                return Json(new { 
                    success = true, 
                    paymentStatus = "Đã thanh toán",
                    paymentDate = order.PaymentDate?.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật thanh toán đơn hàng #{orderId}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Staff/StaffOrder/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            try
            {
                // Parse string status to OrderStatus enum
                if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
                {
                    order.Status = orderStatus;
                    
                    // Nếu chuyển sang trạng thái tính tiền, ghi lại thời điểm
                    if (orderStatus == OrderStatus.Billing)
                    {
                        _logger.LogInformation($"Đơn hàng #{id} đã chuyển sang trạng thái tính tiền vào lúc {DateTime.Now}");
                    }
                    
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Đã cập nhật đơn hàng #{id} sang trạng thái {status}");
                    
                    return Json(new { 
                        success = true,
                        newStatus = status,
                        statusText = GetOrderStatusText(orderStatus),
                        statusBadgeClass = GetOrderStatusBadgeClass(orderStatus)
                    });
                }
                
                return Json(new { success = false, message = "Trạng thái không hợp lệ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật trạng thái đơn hàng #{id}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        
        // Helper methods for status text and badge class
        private string GetOrderStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Đang chờ xác nhận",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Preparing => "Đang chuẩn bị",
                OrderStatus.Delivering => "Đang giao hàng",
                OrderStatus.Billing => "Đang tính tiền",
                OrderStatus.Completed => "Đã hoàn thành",
                OrderStatus.Cancelled => "Đã hủy",
                _ => "Không xác định"
            };
        }
        
        private string GetOrderStatusBadgeClass(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "bg-warning text-dark",
                OrderStatus.Confirmed => "bg-info",
                OrderStatus.Preparing => "bg-primary",
                OrderStatus.Delivering => "bg-primary",
                OrderStatus.Billing => "bg-primary",
                OrderStatus.Completed => "bg-success",
                OrderStatus.Cancelled => "bg-danger",
                _ => "bg-secondary"
            };
        }
    }
} 