using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Security.Claims;

namespace QuanVitLonManager.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CartService> _logger;
        private readonly string CART_SESSION_KEY = "Cart";
        private readonly string ORDER_TYPE_SESSION_KEY = "OrderType";
        private readonly string TABLE_NUMBER_SESSION_KEY = "TableNumber";

        public CartService(
            IHttpContextAccessor httpContextAccessor, 
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CartService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        private HttpContext HttpContext => _httpContextAccessor.HttpContext;

        public void AddToCart(int menuItemId, int quantity, string notes = null)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("User must be logged in to add items to cart");
            }

            var existingItem = _context.CartItems.FirstOrDefault(i => i.MenuItemId == menuItemId && i.UserId == userId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                if (!string.IsNullOrEmpty(notes))
                {
                    existingItem.Notes = notes;
                }
            }
            else
            {
                var newItem = new CartItem
                {
                    MenuItemId = menuItemId,
                    Quantity = quantity,
                    Notes = notes,
                    UserId = userId
                };
                _context.CartItems.Add(newItem);
            }

            _context.SaveChanges();
        }

        public void UpdateQuantity(int menuItemId, int quantity)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(i => i.MenuItemId == menuItemId);

            if (existingItem != null)
            {
                if (quantity > 0)
                {
                    existingItem.Quantity = quantity;
                }
                else
                {
                    cart.Remove(existingItem);
                }
            }

            SaveCart(cart);
            
            // Nếu người dùng đã đăng nhập, lưu giỏ hàng vào database
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                SaveCartToDatabase();
            }
        }

        public void UpdateItemNotes(int menuItemId, string notes)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = GetCart();
            var item = cart.FirstOrDefault(item => item.MenuItemId == menuItemId);

            if (item != null)
            {
                item.Notes = notes;
                SaveCart(cart);
                
                // Nếu người dùng đã đăng nhập, lưu giỏ hàng vào database
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    SaveCartToDatabase();
                }
            }
        }

        public void RemoveFromCart(int menuItemId)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(i => i.MenuItemId == menuItemId);

            if (existingItem != null)
            {
                cart.Remove(existingItem);
            }

            SaveCart(cart);
            
            // Nếu người dùng đã đăng nhập, lưu giỏ hàng vào database
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                SaveCartToDatabase();
            }
        }

        public void SetOrderType(CartOrderType orderType)
        {
            HttpContext.Session.SetInt32(ORDER_TYPE_SESSION_KEY, (int)orderType);
            
            // Cập nhật OrderType cho tất cả các items trong giỏ hàng
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = GetCart();
            foreach (var item in cart)
            {
                item.OrderType = orderType;
            }
            SaveCart(cart);
            
            // Nếu người dùng đã đăng nhập, lưu giỏ hàng vào database
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                SaveCartToDatabase();
            }
        }

        public CartOrderType GetOrderType()
        {
            var orderTypeInt = HttpContext.Session.GetInt32(ORDER_TYPE_SESSION_KEY);
            return orderTypeInt.HasValue ? (CartOrderType)orderTypeInt.Value : CartOrderType.DineIn;
        }

        public void SetTableNumber(string tableNumber)
        {
            HttpContext.Session.SetString(TABLE_NUMBER_SESSION_KEY, tableNumber ?? string.Empty);
            
            // Cập nhật TableNumber cho tất cả các items trong giỏ hàng
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = GetCart();
            foreach (var item in cart)
            {
                item.TableNumber = tableNumber;
            }
            SaveCart(cart);
            
            // Nếu người dùng đã đăng nhập, lưu giỏ hàng vào database
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                SaveCartToDatabase();
            }
        }

        public string GetTableNumber()
        {
            return HttpContext.Session.GetString(TABLE_NUMBER_SESSION_KEY) ?? string.Empty;
        }

        public List<CartItem> GetCart()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new List<CartItem>();
            }

            return _context.CartItems
                .Include(c => c.MenuItem)
                .Where(c => c.UserId == userId)
                .ToList();
        }

        public int GetCartItemCount()
        {
            var cart = GetCart();
            return cart.Sum(item => item.Quantity);
        }

        public decimal GetCartTotal()
        {
            var cart = GetCart();
            decimal total = 0;
            
            foreach (var item in cart)
            {
                var menuItem = _context.MenuItems.FirstOrDefault(m => m.Id == item.MenuItemId);
                if (menuItem != null)
                {
                    total += menuItem.Price * item.Quantity;
                }
            }
            
            return total;
        }

        public void ClearCart()
        {
            HttpContext.Session.Remove(CART_SESSION_KEY);
            
            // Nếu người dùng đã đăng nhập, xóa giỏ hàng trong database
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var userCart = _context.UserCarts
                        .Include(uc => uc.CartItems)
                        .FirstOrDefault(uc => uc.UserId == userId);
                    
                    if (userCart != null)
                    {
                        _context.CartItems.RemoveRange(userCart.CartItems);
                        userCart.CartItems.Clear();
                        userCart.LastUpdated = DateTime.Now;
                        _context.SaveChanges();
                    }
                }
            }
        }

        private void SaveCart(List<CartItem> cart)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string cartJson = JsonSerializer.Serialize(cart, options);
            HttpContext.Session.SetString(CART_SESSION_KEY, cartJson);
        }

        public void SaveCartToDatabase()
        {
            try
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    var userId = _userManager.GetUserId(HttpContext.User);
                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogWarning("Không thể lưu giỏ hàng: UserId là null hoặc rỗng");
                        return;
                    }

                    var cart = GetCart();
                    var userCart = _context.UserCarts
                        .Include(uc => uc.CartItems)
                        .FirstOrDefault(uc => uc.UserId == userId);

                    if (userCart == null)
                    {
                        userCart = new UserCart
                        {
                            UserId = userId,
                            LastUpdated = DateTime.Now,
                            CartItems = new List<CartItem>()
                        };
                        _context.UserCarts.Add(userCart);
                    }
                    else
                    {
                        // Xóa các mục hiện có trong giỏ hàng
                        _context.CartItems.RemoveRange(userCart.CartItems);
                        userCart.CartItems.Clear();
                        userCart.LastUpdated = DateTime.Now;
                    }

                    // Thêm các mục mới từ session vào database
                    foreach (var item in cart)
                    {
                        userCart.CartItems.Add(new CartItem
                        {
                            MenuItemId = item.MenuItemId,
                            Quantity = item.Quantity,
                            Notes = item.Notes,
                            OrderType = item.OrderType,
                            TableNumber = item.TableNumber,
                            UserId = userId
                        });
                    }

                    _context.SaveChanges();
                    _logger.LogInformation($"Đã lưu giỏ hàng cho người dùng {userId} với {cart.Count} mục");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu giỏ hàng vào database");
            }
        }

        public void LoadCartFromDatabase()
        {
            try
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    var userId = _userManager.GetUserId(HttpContext.User);
                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogWarning("Không thể tải giỏ hàng: UserId là null hoặc rỗng");
                        return;
                    }

                    var userCart = _context.UserCarts
                        .Include(uc => uc.CartItems)
                        .FirstOrDefault(uc => uc.UserId == userId);

                    if (userCart != null && userCart.CartItems.Any())
                    {
                        var cartItems = userCart.CartItems.Select(item => new CartItem
                        {
                            MenuItemId = item.MenuItemId,
                            Quantity = item.Quantity,
                            Notes = item.Notes,
                            OrderType = item.OrderType,
                            TableNumber = item.TableNumber,
                            UserId = userId
                        }).ToList();

                        SaveCart(cartItems);
                        
                        // Cập nhật OrderType và TableNumber trong session
                        if (cartItems.Any())
                        {
                            var firstItem = cartItems.First();
                            SetOrderType(firstItem.OrderType);
                            SetTableNumber(firstItem.TableNumber);
                        }
                        
                        _logger.LogInformation($"Đã tải giỏ hàng cho người dùng {userId} với {cartItems.Count} mục");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải giỏ hàng từ database");
            }
        }
    }
}