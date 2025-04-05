using Microsoft.AspNetCore.Mvc;
using QuanVitLonManager.Services;
using QuanVitLonManager.Models;
using QuanVitLonManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Thêm namespace này
using System;
using System.Security.Claims;
using System.Linq;

namespace QuanVitLonManager.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _context;

        public CartController(ICartService cartService, ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var cartItems = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .Include(c => c.MenuItem)
                    .ToListAsync();

                // GetOrderType đã có giá trị mặc định, không cần kiểm tra null
                ViewBag.OrderType = _cartService.GetOrderType();
                ViewBag.CartTotal = cartItems.Sum(item => item.MenuItem != null ? item.MenuItem.Price * item.Quantity : 0);

                return View(cartItems);
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải giỏ hàng";
                return View(new List<CartItem>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int menuItemId, int quantity = 1)
        {
            // Kiểm tra xem món ăn có tồn tại không
            var menuItem = await _context.MenuItems.FindAsync(menuItemId);
            if (menuItem == null)
            {
                return NotFound();
            }

            _cartService.AddToCart(menuItemId, quantity);
            
            // Thêm thông báo thành công
            TempData["SuccessMessage"] = $"Đã thêm {quantity} {menuItem.Name} vào giỏ hàng";
            
            // Nếu là Ajax request, trả về JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { 
                    success = true, 
                    message = $"Đã thêm {quantity} {menuItem.Name} vào giỏ hàng",
                    cartCount = _cartService.GetCartItemCount(),
                    cartTotal = _cartService.GetCartTotal()
                });
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int menuItemId, int quantity)
        {
            if (quantity <= 0)
            {
                _cartService.RemoveFromCart(menuItemId);
            }
            else
            {
                _cartService.UpdateQuantity(menuItemId, quantity);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { 
                    success = true,
                    cartCount = _cartService.GetCartItemCount(),
                    cartTotal = _cartService.GetCartTotal()
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateOrderType(CartOrderType orderType)
        {
            _cartService.SetOrderType(orderType);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateTableNumber(string tableNumber)
        {
            _cartService.SetTableNumber(tableNumber);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int menuItemId)
        {
            _cartService.RemoveFromCart(menuItemId);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { 
                    success = true,
                    cartCount = _cartService.GetCartItemCount(),
                    cartTotal = _cartService.GetCartTotal()
                });
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetCartInfo()
        {
            return Json(new { 
                itemCount = _cartService.GetCartItemCount(),
                total = _cartService.GetCartTotal(),
                orderType = _cartService.GetOrderType(),
                tableNumber = _cartService.GetTableNumber()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateNotes(int menuItemId, string notes)
        {
            _cartService.UpdateItemNotes(menuItemId, notes);
            return Json(new { success = true });
        }

        // Phương thức để lấy thông tin chi tiết của các mục trong giỏ hàng
        private async Task<List<CartItem>> GetCartItemsWithDetails()
        {
            var cartItems = _cartService.GetCart();
            
            // Lấy thông tin chi tiết của các món ăn
            foreach (var item in cartItems)
            {
                item.MenuItem = await _context.MenuItems
                    .Include(m => m.Category)
                    .FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
            }
            
            return cartItems;
        }

        [HttpGet]
        public async Task<IActionResult> GetItemPrice(int menuItemId, int quantity)
        {
            var menuItem = await _context.MenuItems.FindAsync(menuItemId);
            if (menuItem == null)
            {
                return Json(new { success = false });
            }

            decimal price = menuItem.Price * quantity;
            return Json(new { success = true, price });
        }
    }
}