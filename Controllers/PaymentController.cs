using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;
using System;
using System.Threading.Tasks;

namespace QuanVitLonManager.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Payment/Process/5
        public async Task<IActionResult> Process(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["WarningMessage"] = "Đơn hàng này đã được thanh toán trước đó.";
                return RedirectToAction("Details", "Order", new { id = order.Id });
            }

            var viewModel = new PaymentViewModel
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                Notes = order.Notes,
                Items = order.OrderDetails.Select(od => new PaymentItemViewModel
                {
                    Name = od.MenuItem?.Name ?? "Món không tồn tại",
                    Quantity = od.Quantity,
                    Price = od.Price,
                    Subtotal = od.Quantity * od.Price
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Payment/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id, PaymentMethod paymentMethod)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["WarningMessage"] = "Đơn hàng này đã được thanh toán trước đó.";
                return RedirectToAction("Details", "Order", new { id = order.Id });
            }

            // Cập nhật thông tin thanh toán
            order.PaymentMethod = paymentMethod;
            order.PaymentStatus = PaymentStatus.Paid;
            order.PaymentDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thanh toán thành công! Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.";
            return RedirectToAction("PaymentSuccess", new { id = order.Id });
        }

        // GET: Payment/PaymentSuccess/5
        public async Task<IActionResult> PaymentSuccess(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.PaymentStatus != PaymentStatus.Paid)
            {
                return RedirectToAction("Process", new { id = order.Id });
            }

            return View(order);
        }
    }
} 