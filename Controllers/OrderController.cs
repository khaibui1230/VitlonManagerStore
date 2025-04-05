using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Hubs;
using QuanVitLonManager.Models;
using QuanVitLonManager.Services;
using QuanVitLonManager.ViewModels;
using Microsoft.Extensions.Logging;

namespace QuanVitLonManager.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _cartService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ICartService cartService,
            IHubContext<NotificationHub> hubContext,
            ILogger<OrderController> logger)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
            _hubContext = hubContext;
            _logger = logger;
        }

        // GET: Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            try
            {
                _logger.LogInformation("Checkout action called");
                
                var cartItems = await GetCartItemsWithDetails();
                
                // Kiểm tra nếu giỏ hàng trống
                if (cartItems == null || !cartItems.Any())
                {
                    _logger.LogWarning("Cart is empty when accessing Checkout");
                    TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Cart");
                }

                var orderType = _cartService.GetOrderType();
                var tableNumber = _cartService.GetTableNumber();

                // Tính tổng tiền, đảm bảo không bị lỗi nếu MenuItem là null
                decimal totalAmount = 0;
                foreach (var item in cartItems)
                {
                    if (item.MenuItem != null)
                    {
                        totalAmount += item.MenuItem.Price * item.Quantity;
                    }
                }

                var viewModel = new CheckoutViewModel
                {
                    CartItems = cartItems,
                    Order = new Order
                    {
                        TotalAmount = totalAmount,
                        Notes = orderType == CartOrderType.TakeAway ? "Mang về" : $"Ăn tại chỗ - Bàn số: {tableNumber}",
                        OrderType = orderType == CartOrderType.TakeAway ? OrderType.TakeAway : OrderType.DineIn
                    },
                    // Không yêu cầu đăng nhập cho bất kỳ loại đơn hàng nào
                    RequiresLogin = false
                };

                // Nếu đã có số bàn từ giỏ hàng, tìm Table tương ứng
                if (orderType == CartOrderType.DineIn && !string.IsNullOrEmpty(tableNumber))
                {
                    var table = await _context.Tables.FirstOrDefaultAsync(t => t.TableNumber == tableNumber);
                    if (table != null)
                    {
                        viewModel.Order.TableId = table.Id;
                    }
                }
                
                // Log thông tin viewModel để debug
                _logger.LogInformation("Checkout ViewModel created successfully. CartItems count: " + viewModel.CartItems.Count());

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Checkout action");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xử lý giỏ hàng. Vui lòng thử lại.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // POST: Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            try
            {
                _logger.LogInformation("PlaceOrder method called");
                
                // Log model state và các giá trị
                _logger.LogInformation($"Model state is valid: {ModelState.IsValid}");
                _logger.LogInformation($"CustomerName: {model.CustomerName}");
                _logger.LogInformation($"PhoneNumber: {model.PhoneNumber}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning($"Model state errors: {string.Join(", ", errors)}");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin", errors = errors });
                    }
                    
                    return View("Checkout", model);
                }

                var cartItems = await GetCartItemsWithDetails();
                if (!cartItems.Any())
                {
                    var message = "Giỏ hàng của bạn đang trống.";
                    _logger.LogWarning(message);
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = message });
                    }
                    TempData["ErrorMessage"] = message;
                    return RedirectToAction("Index", "Cart");
                }

                // Tạo đơn hàng mới
                var order = new Order
                {
                    UserId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : null,
                    CustomerName = model.CustomerName,
                    PhoneNumber = model.PhoneNumber,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    OrderType = model.Order.OrderType,
                    TableId = model.Order.TableId,
                    Notes = model.Order.Notes,
                    PaymentMethod = model.Order.PaymentMethod,
                    PaymentStatus = PaymentStatus.Unpaid,
                    OrderDetails = new List<OrderDetail>()
                };

                // Thêm chi tiết đơn hàng
                decimal total = 0;
                foreach (var item in cartItems)
                {
                    if (item.MenuItem != null)
                    {
                        var orderDetail = new OrderDetail
                        {
                            MenuItemId = item.MenuItemId,
                            Price = item.MenuItem.Price,
                            Quantity = item.Quantity,
                            Notes = item.Notes
                        };
                        order.OrderDetails.Add(orderDetail);
                        total += item.MenuItem.Price * item.Quantity;
                    }
                }
                order.TotalAmount = total;

                _logger.LogInformation($"Created order with {order.OrderDetails.Count} items, total: {total}");

                // Lưu đơn hàng vào database
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Lấy ID đơn hàng mới tạo
                int newOrderId = order.Id;
                _logger.LogInformation("Order created with ID: {orderId}", newOrderId);

                // Tạo DishOrders cho từng món ăn
                foreach (var orderDetail in order.OrderDetails)
                {
                    var menuItem = await _context.MenuItems
                        .FirstOrDefaultAsync(m => m.Id == orderDetail.MenuItemId);
                        
                    if (menuItem != null)
                    {
                        var dishOrder = new DishOrder
                        {
                            Name = menuItem.Name,
                            Price = orderDetail.Price,
                            TotalPrice = orderDetail.Price * orderDetail.Quantity,
                            OrderType = order.OrderType,
                            Notes = orderDetail.Notes,
                            OrderTime = DateTime.Now,
                            Status = DishOrderStatus.Pending,
                            Quantity = orderDetail.Quantity,
                            OrderId = order.Id,
                            MenuItemId = menuItem.Id
                        };
                        
                        _context.DishOrders.Add(dishOrder);
                    }
                }
                
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Created DishOrders for order {newOrderId}");

                // Lưu ID đơn hàng vào TempData và Session
                TempData["LastOrderId"] = newOrderId;
                HttpContext.Session.SetInt32("LastOrderId", newOrderId);

                // Xóa giỏ hàng
                _cartService.ClearCart();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    _logger.LogInformation("Returning JSON success response");
                    return Json(new { 
                        success = true, 
                        message = "Đặt hàng thành công!", 
                        orderId = newOrderId,
                        redirectUrl = Url.Action("OrderConfirmation", "Order", new { id = newOrderId })
                    });
                }
                
                _logger.LogInformation("Redirecting to order confirmation page");
                return RedirectToAction("OrderConfirmation", new { id = newOrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = "Có lỗi xảy ra khi đặt hàng", 
                        error = ex.Message
                    });
                }
                
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đặt hàng: " + ex.Message;
                return RedirectToAction("Checkout");
            }
        }

        // GET: Order/AnonymousOrderConfirmation/5
        public async Task<IActionResult> AnonymousOrderConfirmation(int id, string phoneNumber = null)
        {
            _logger.LogInformation("AnonymousOrderConfirmation called with id={0}, phoneNumber={1}", id, phoneNumber);
            
            // Kiểm tra xem có phải đơn hàng được tạo gần đây không (từ TempData)
            if (TempData["LastOrderId"] != null)
            {
                _logger.LogInformation("Found LastOrderId in TempData: {0}", TempData["LastOrderId"]);
                int? lastOrderId = TempData["LastOrderId"] as int?;
                
                if (lastOrderId.HasValue && lastOrderId.Value == id)
                {
                    _logger.LogInformation("LastOrderId matches requested id, querying by id only");
                    
                    // Truy vấn chỉ dựa vào ID nếu đây là đơn hàng mới nhất
                    var newOrder = await _context.Orders
                        .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.MenuItem)
                        .Include(o => o.Table)
                        .FirstOrDefaultAsync(o => o.Id == id);
                    
                    if (newOrder != null)
                    {
                        _logger.LogInformation("Order found by ID: {0}", newOrder.Id);
                        return View("OrderConfirmation", newOrder);
                    }
                }
            }
            
            // Nếu không tìm thấy từ TempData, thử tìm bằng ID và số điện thoại
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MenuItem)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == id && 
                                         (string.IsNullOrEmpty(phoneNumber) || o.PhoneNumber == phoneNumber));

            if (order != null)
            {
                _logger.LogInformation("Order found by ID and phone: {0}", order.Id);
                return View("OrderConfirmation", order);
            }
            
            // Cuối cùng, thử tìm chỉ bằng ID
            order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MenuItem)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order != null)
            {
                _logger.LogInformation("Order found by ID only: {0}", order.Id);
                return View("OrderConfirmation", order);
            }

            _logger.LogWarning("Order not found with id={0}, phoneNumber={1}", id, phoneNumber);
            TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Order/OrderConfirmation/5
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            _logger.LogInformation("OrderConfirmation gọi với id={0}", id);
            
            // Thử lấy từ session trước
            int? sessionOrderId = HttpContext.Session.GetInt32("LastOrderId");
            if (sessionOrderId.HasValue && sessionOrderId.Value == id)
            {
                _logger.LogInformation("Đang sử dụng ID đơn hàng từ session: {0}", id);
            }
            
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                    .Include(o => o.Table)
                    .FirstOrDefaultAsync(o => o.Id == id);
                
                // Kiểm tra đơn hàng có tồn tại không
                if (order == null)
                {
                    _logger.LogWarning("Không tìm thấy đơn hàng với ID: {0}", id);
                    
                    // Kiểm tra LastOrderId trong TempData
                    var lastOrderId = TempData["LastOrderId"] as int?;
                    if (lastOrderId.HasValue && lastOrderId.Value == id)
                    {
                        _logger.LogInformation("Thử tìm đơn hàng bằng LastOrderId: {0}", lastOrderId.Value);
                        
                        // Tìm lại đơn hàng không quan tâm đến UserId
                        order = await _context.Orders
                            .Include(o => o.OrderDetails)
                            .ThenInclude(od => od.MenuItem)
                            .Include(o => o.Table)
                            .FirstOrDefaultAsync(o => o.Id == lastOrderId.Value);
                            
                        if (order != null)
                        {
                            _logger.LogInformation("Đã tìm thấy đơn hàng bằng LastOrderId");
                            return View(order);
                        }
                    }
                    
                    _logger.LogWarning("Đơn hàng vẫn không được tìm thấy sau khi thử với LastOrderId");
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng.";
                    return RedirectToAction("MyOrders");
                }
                
                _logger.LogInformation("Đã tìm thấy đơn hàng, hiển thị trang xác nhận");
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm đơn hàng với ID: {0}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi truy cập thông tin đơn hàng.";
                return RedirectToAction("MyOrders");
            }
        }

        // GET: Order/MyOrders
        public async Task<IActionResult> MyOrders(string phoneNumber = null)
        {
            // Nếu người dùng đăng nhập, lấy danh sách đơn hàng của họ
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return View(new MyOrdersViewModel 
                { 
                    Orders = orders,
                    IsAuthenticated = true,
                    PhoneNumber = phoneNumber
                });
            }
            
            // Nếu người dùng không đăng nhập và đã cung cấp số điện thoại
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                // Tìm đơn hàng theo số điện thoại
                var orders = await _context.Orders
                    .Where(o => o.PhoneNumber == phoneNumber)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return View(new MyOrdersViewModel 
                { 
                    Orders = orders,
                    IsAuthenticated = false,
                    PhoneNumber = phoneNumber,
                    HasSearched = true
                });
            }
            
            // Người dùng chưa đăng nhập và chưa cung cấp số điện thoại
            return View(new MyOrdersViewModel 
            { 
                Orders = new List<Order>(),
                IsAuthenticated = false,
                HasSearched = false
            });
        }

        // POST: Order/MyOrders
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MyOrders(MyOrdersViewModel model)
        {
            if (string.IsNullOrEmpty(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Vui lòng nhập số điện thoại để tra cứu đơn hàng");
                return View(new MyOrdersViewModel 
                { 
                    Orders = new List<Order>(),
                    IsAuthenticated = User.Identity.IsAuthenticated,
                    HasSearched = false
                });
            }
            
            return RedirectToAction(nameof(MyOrders), new { phoneNumber = model.PhoneNumber });
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int id, string phoneNumber = null)
        {
            // Nếu người dùng đã đăng nhập
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                    .Include(o => o.Table)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            // Nếu người dùng chưa đăng nhập và có số điện thoại
            else if (!string.IsNullOrEmpty(phoneNumber))
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                    .Include(o => o.Table)
                    .FirstOrDefaultAsync(o => o.Id == id && o.PhoneNumber == phoneNumber);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng hoặc số điện thoại không đúng.";
                    return RedirectToAction("MyOrders");
                }

                return View(order);
            }
            // Nếu không có thông tin xác thực
            else
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập hoặc cung cấp số điện thoại để xem chi tiết đơn hàng.";
                return RedirectToAction("MyOrders");
            }
        }

        // POST: Order/Cancel/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Chỉ cho phép hủy đơn hàng nếu trạng thái là Pending hoặc Confirmed
            if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Confirmed)
            {
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                
                // Gửi thông báo realtime
                await _hubContext.Clients.All.SendAsync("ReceiveOrderNotification", $"Đơn hàng #{order.Id} đã bị hủy.");
                
                TempData["SuccessMessage"] = "Hủy đơn hàng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng ở trạng thái hiện tại.";
            }

            return RedirectToAction(nameof(MyOrders));
        }

        // GET: Order/EditOrder/5
        [Authorize]
        public async Task<IActionResult> EditOrder(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MenuItem)
                .ThenInclude(m => m.Category)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Chỉ cho phép chỉnh sửa đơn hàng khi trạng thái là Pending hoặc Confirmed
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa đơn hàng ở trạng thái hiện tại.";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }

            // Lấy danh sách các danh mục và món ăn để hiển thị trong form
            ViewBag.Categories = await _context.Categories
                .Include(c => c.MenuItems)
                .Where(c => c.MenuItems.Any(m => m.IsAvailable))
                .ToListAsync();

            return View(order);
        }

        // POST: Order/AddItem
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int orderId, int menuItemId, int quantity)
        {
            if (quantity <= 0)
            {
                return BadRequest(new { success = false, message = "Số lượng phải lớn hơn 0" });
            }

            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Chỉ cho phép chỉnh sửa đơn hàng khi trạng thái là Pending hoặc Confirmed
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            {
                return BadRequest(new { success = false, message = "Không thể chỉnh sửa đơn hàng ở trạng thái hiện tại" });
            }

            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Id == menuItemId && m.IsAvailable);

            if (menuItem == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy món ăn hoặc món ăn không khả dụng" });
            }

            // Kiểm tra xem món ăn đã có trong đơn hàng chưa
            var existingItem = order.OrderDetails.FirstOrDefault(od => od.MenuItemId == menuItemId);
            if (existingItem != null)
            {
                // Nếu đã có, cập nhật số lượng
                existingItem.Quantity += quantity;
            }
            else
            {
                // Nếu chưa có, thêm mới
                order.OrderDetails.Add(new OrderDetail
                {
                    MenuItemId = menuItemId,
                    Quantity = quantity,
                    Price = menuItem.Price
                });
            }

            // Cập nhật tổng tiền
            order.TotalAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Thêm món thành công",
                totalAmount = order.TotalAmount,
                itemName = menuItem.Name,
                itemPrice = menuItem.Price,
                itemQuantity = quantity,
                itemTotal = menuItem.Price * quantity
            });
        }

        // POST: Order/RemoveItem
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int orderId, int orderDetailId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Chỉ cho phép chỉnh sửa đơn hàng khi trạng thái là Pending hoặc Confirmed
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            {
                return BadRequest(new { success = false, message = "Không thể chỉnh sửa đơn hàng ở trạng thái hiện tại" });
            }

            var orderDetail = order.OrderDetails.FirstOrDefault(od => od.Id == orderDetailId);
            if (orderDetail == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy món ăn trong đơn hàng" });
            }

            // Kiểm tra nếu đây là món ăn cuối cùng trong đơn hàng
            if (order.OrderDetails.Count == 1)
            {
                return BadRequest(new { success = false, message = "Không thể xóa món ăn cuối cùng trong đơn hàng. Bạn có thể hủy đơn hàng hoặc thêm món khác trước khi xóa." });
            }

            // Xóa món khỏi đơn hàng
            _context.OrderDetails.Remove(orderDetail);
            
            // Cập nhật tổng tiền
            order.TotalAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity) - (orderDetail.Price * orderDetail.Quantity);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Xóa món thành công",
                totalAmount = order.TotalAmount
            });
        }

        // POST: Order/UpdateQuantity
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int orderId, int orderDetailId, int quantity)
        {
            if (quantity <= 0)
            {
                return BadRequest(new { success = false, message = "Số lượng phải lớn hơn 0" });
            }

            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Chỉ cho phép chỉnh sửa đơn hàng khi trạng thái là Pending hoặc Confirmed
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            {
                return BadRequest(new { success = false, message = "Không thể chỉnh sửa đơn hàng ở trạng thái hiện tại" });
            }

            var orderDetail = order.OrderDetails.FirstOrDefault(od => od.Id == orderDetailId);
            if (orderDetail == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy món ăn trong đơn hàng" });
            }

            // Cập nhật số lượng
            orderDetail.Quantity = quantity;
            
            // Cập nhật tổng tiền
            order.TotalAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Cập nhật số lượng thành công",
                itemTotal = orderDetail.Price * quantity,
                totalAmount = order.TotalAmount
            });
        }

        // POST: Order/UpdateNotes
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotes(int orderId, string notes)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Chỉ cho phép chỉnh sửa đơn hàng khi trạng thái là Pending hoặc Confirmed
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            {
                return BadRequest(new { success = false, message = "Không thể chỉnh sửa đơn hàng ở trạng thái hiện tại" });
            }

            // Cập nhật ghi chú
            order.Notes = notes;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Cập nhật ghi chú thành công"
            });
        }

        // GET: Order/RecentOrderConfirmation
        public async Task<IActionResult> RecentOrderConfirmation()
        {
            // Lấy giá trị từ TempData
            int? lastOrderId = TempData["LastOrderId"] as int?;
            string lastOrderPhone = TempData["LastOrderPhone"] as string;
            
            _logger.LogInformation("RecentOrderConfirmation called, LastOrderId={0}, LastOrderPhone={1}", 
                lastOrderId, lastOrderPhone);

            // Không để mất giá trị TempData khi chuyển trang
            TempData.Keep("LastOrderId");
            TempData.Keep("LastOrderPhone");
            
            if (lastOrderId.HasValue)
            {
                // Tìm đơn hàng gần nhất
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                    .Include(o => o.Table)
                    .FirstOrDefaultAsync(o => o.Id == lastOrderId.Value);
                
                if (order != null)
                {
                    _logger.LogInformation("Found recent order: {0}", order.Id);
                    TempData["SuccessMessage"] = "Đây là đơn hàng gần nhất bạn đã đặt!";
                    return View("OrderConfirmation", order);
                }
            }
            
            // Nếu đã đăng nhập, tìm đơn hàng mới nhất của người dùng
            if (User.Identity.IsAuthenticated)
            {
                string userId = _userManager.GetUserId(User);
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                    .Include(o => o.Table)
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefaultAsync();
                
                if (order != null)
                {
                    _logger.LogInformation("Found most recent order for user: {0}", order.Id);
                    TempData["SuccessMessage"] = "Đây là đơn hàng gần nhất của bạn!";
                    return View("OrderConfirmation", order);
                }
            }
            // Nếu có số điện thoại, tìm đơn hàng mới nhất theo số điện thoại
            else if (!string.IsNullOrEmpty(lastOrderPhone))
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                    .Include(o => o.Table)
                    .Where(o => o.PhoneNumber == lastOrderPhone)
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefaultAsync();
                
                if (order != null)
                {
                    _logger.LogInformation("Found most recent order for phone: {0}", order.Id);
                    TempData["SuccessMessage"] = "Đây là đơn hàng gần nhất của bạn!";
                    return View("OrderConfirmation", order);
                }
            }
            
            // Không tìm thấy đơn hàng
            _logger.LogWarning("No recent orders found");
            TempData["ErrorMessage"] = "Không tìm thấy đơn hàng gần đây nào.";
            return RedirectToAction("MyOrders");
        }

        // Phương thức để lấy thông tin chi tiết của các mục trong giỏ hàng
        private async Task<List<CartItem>> GetCartItemsWithDetails()
        {
            try
            {
                var cartItems = _cartService.GetCart();
                
                if (cartItems == null || !cartItems.Any())
                {
                    _logger.LogWarning("Cart is empty in GetCartItemsWithDetails");
                    return new List<CartItem>();
                }
                
                // Lấy thông tin chi tiết của các món ăn
                foreach (var item in cartItems)
                {
                    if (item != null && item.MenuItemId > 0)
                    {
                        try
                        {
                            item.MenuItem = await _context.MenuItems
                                .Include(m => m.Category)
                                .FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
                            
                            if (item.MenuItem == null)
                            {
                                _logger.LogWarning("MenuItem not found for MenuItemId: {0}", item.MenuItemId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error fetching MenuItem with ID: {0}", item.MenuItemId);
                        }
                    }
                }
                
                return cartItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCartItemsWithDetails");
                return new List<CartItem>();
            }
        }
    }
}