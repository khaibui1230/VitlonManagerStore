using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Web;

namespace QuanVitLonManager.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StaffOrderController> _logger;

        public StaffOrderController(ApplicationDbContext context, ILogger<StaffOrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Loading staff orders index");
            
            var orders = await _context.DishOrders
                .Include(o => o.Order)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            if (!orders.Any())
            {
                _logger.LogInformation("No orders found in database");
            }
            else
            {
                _logger.LogInformation($"Found {orders.Count} orders");
                foreach (var order in orders.Take(5))
                {
                    _logger.LogInformation($"Order: {order.Id}, {order.Name}, {order.Status}");
                }
            }

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new StaffOrderViewModel
            {
                Categories = await _context.Categories
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffOrderViewModel viewModel)
        {
            _logger.LogInformation("Create method called with model state valid: {isValid}", ModelState.IsValid);
            _logger.LogInformation("OrderItems count: {count}", viewModel.OrderItems?.Count ?? 0);
            
            // Kiểm tra ModelState và OrderItems
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid");
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("ModelState error: {error}", modelError.ErrorMessage);
                }
            }
            
            if (viewModel.OrderItems == null || !viewModel.OrderItems.Any())
            {
                _logger.LogWarning("No order items provided");
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một món ăn vào đơn hàng");
                return await ReloadCreateView(viewModel);
            }

            try
            {
                _logger.LogInformation("Creating new staff order with {count} items", viewModel.OrderItems.Count);
                
                // Find current user - if not found, use a system user
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    // Find an admin user to assign the order to
                    var adminUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserName == "admin@example.com");
                    
                    if (adminUser != null)
                    {
                        userId = adminUser.Id;
                    }
                    else
                    {
                        // Get the first user from the database
                        var firstUser = await _context.Users.FirstOrDefaultAsync();
                        userId = firstUser?.Id ?? "unknown";
                    }
                }
                
                // Kiểm tra định dạng của TableNumber khi là DineIn
                if (viewModel.OrderType == OrderType.DineIn && string.IsNullOrWhiteSpace(viewModel.TableNumber))
                {
                    ModelState.AddModelError("TableNumber", "Vui lòng nhập số bàn khi chọn ăn tại quán");
                    return await ReloadCreateView(viewModel);
                }
                
                // Create a new Order entity to group all dish orders
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TableNumber = viewModel.TableNumber ?? "Mang về",
                    Notes = viewModel.Note,
                    Status = OrderStatus.Confirmed,
                    TotalAmount = viewModel.OrderItems.Sum(item => item.Quantity * item.Price)
                };
                
                // Save the order first to get an ID
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new order with ID {orderId} for table {tableNumber}", 
                    order.Id, viewModel.TableNumber);
                
                // Log chi tiết từng item
                foreach (var item in viewModel.OrderItems)
                {
                    _logger.LogInformation("OrderItem: MenuItemId={id}, Name={name}, Quantity={quantity}, Price={price}", 
                        item.MenuItemId, item.Name, item.Quantity, item.Price);
                    
                    var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                    if (menuItem == null) 
                    {
                        _logger.LogWarning("Menu item with ID {id} not found", item.MenuItemId);
                        continue;
                    }

                    // Tạo đơn hàng mới với thông tin từ item - xử lý ghi chú một cách an toàn
                    string itemNotes = "";
                    
                    // Xử lý cả khi viewModel.Note hoặc item.ItemNote là null
                    if (!string.IsNullOrEmpty(viewModel.Note) && !string.IsNullOrEmpty(item.ItemNote))
                    {
                        itemNotes = $"{viewModel.Note} - {item.ItemNote}";
                    }
                    else if (!string.IsNullOrEmpty(viewModel.Note))
                    {
                        itemNotes = viewModel.Note;
                    }
                    else if (!string.IsNullOrEmpty(item.ItemNote))
                    {
                        itemNotes = item.ItemNote;
                    }

                    var dishOrder = new DishOrder
                    {
                        Name = menuItem.Name,
                        Price = menuItem.Price,
                        Quantity = item.Quantity,
                        TotalPrice = item.Quantity * menuItem.Price,
                        OrderType = viewModel.OrderType,
                        Notes = itemNotes,
                        OrderTime = DateTime.Now,
                        Status = DishOrderStatus.Pending,
                        OrderId = order.Id,
                        MenuItemId = menuItem.Id
                    };

                    _context.Add(dishOrder);
                    _logger.LogInformation("Added dish order: {name}, Quantity: {quantity}, Note: {note}, OrderId: {orderId}", 
                        dishOrder.Name, dishOrder.Quantity, dishOrder.Notes, dishOrder.OrderId);
                }
                
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("SaveChangesAsync completed with {count} changes", result);
                
                if (result <= 0)
                {
                    _logger.LogWarning("No changes were saved to database");
                    ModelState.AddModelError("", "Không có thay đổi nào được lưu vào cơ sở dữ liệu");
                    return await ReloadCreateView(viewModel);
                }
                
                TempData["SuccessMessage"] = "Đơn hàng đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff order");
                
                // Log inner exception details
                var innerEx = ex.InnerException;
                while (innerEx != null) 
                {
                    _logger.LogError(innerEx, "Inner exception: {message}", innerEx.Message);
                    innerEx = innerEx.InnerException;
                }
                
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo đơn hàng: " + ex.Message);
                return await ReloadCreateView(viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status) || !Enum.TryParse(status, out DishOrderStatus statusEnum))
            {
                return Json(new { success = false, message = "Trạng thái không hợp lệ" });
            }

            var dishOrder = await _context.DishOrders.FindAsync(id);
            if (dishOrder == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Không cho phép hủy món đã hoàn thành
            if (dishOrder.Status == DishOrderStatus.Completed && statusEnum == DishOrderStatus.Cancelled)
            {
                return Json(new { success = false, message = "Không thể hủy món đã hoàn thành" });
            }

            dishOrder.Status = statusEnum;
            _context.Update(dishOrder);
            await _context.SaveChangesAsync();

            var completed = false;
            var printUrl = "";
            var customerBillUrl = "";

            // Kiểm tra nếu đã hoàn thành tất cả các món ăn trong đơn hàng
            if (statusEnum == DishOrderStatus.Completed)
            {
                var orderId = dishOrder.OrderId;
                if (orderId.HasValue)
                {
                    var order = await _context.Orders.FindAsync(orderId.Value);
                    var allDishesCompleted = await _context.DishOrders
                        .Where(d => d.OrderId == orderId.Value && d.Status != DishOrderStatus.Cancelled)
                        .AllAsync(d => d.Status == DishOrderStatus.Completed);

                    if (allDishesCompleted && order != null)
                {
                    order.Status = OrderStatus.Completed;
                        _context.Update(order);
                        await _context.SaveChangesAsync();
                        completed = true;
                        printUrl = Url.Action("PrintBill", new { id = orderId.Value });
                        customerBillUrl = Url.Action("Details", "CustomerBill", new { id = orderId.Value });
                        
                        // Đảm bảo thông tin nhà hàng đã được thiết lập
                        var existingInfo = await _context.RestaurantInfo.FirstOrDefaultAsync();
                        if (existingInfo == null)
                        {
                            var restaurantInfo = new RestaurantInfo
                            {
                                Name = "Quán Hiển - Vịt Lộn-Cút lộn",
                                Description = "Chuyên các món vịt lộn, cút lộn ngon tại Gò Vấp",
                                Address = "354 Lê Văn Thọ, phường 11, quận Gò Vấp, TP HCM",
                                Phone = "0379665639",
                                Email = "vitlonhien@gmail.com",
                                TaxId = "0123456789",
                                LogoUrl = "/images/logo.png",
                                WelcomeMessage = "Cảm ơn quý khách đã sử dụng dịch vụ!",
                                GoodbyeMessage = "Hẹn gặp lại quý khách lần sau!"
                            };
                            _context.RestaurantInfo.Add(restaurantInfo);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            return Json(new { success = true, completed, printUrl, customerBillUrl });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

                // Parse string status to OrderStatus enum
                if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
                {
                    order.Status = orderStatus;
                    
                // Nếu chuyển sang trạng thái tính tiền, cập nhật tất cả các DishOrder của Order này
                    if (orderStatus == OrderStatus.Billing)
                {
                    var dishOrders = await _context.DishOrders
                        .Where(d => d.OrderId == id && d.Status != DishOrderStatus.Cancelled)
                        .ToListAsync();
                    
                    foreach (var dishOrder in dishOrders)
                    {
                        if (dishOrder.Status != DishOrderStatus.Completed)
                        {
                            dishOrder.Status = DishOrderStatus.Completed;
                        }
                    }
                    
                    _logger.LogInformation($"Chuyển {dishOrders.Count} món ăn của đơn hàng #{id} sang trạng thái Hoàn thành");
                    }
                    
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Đã cập nhật đơn hàng #{id} sang trạng thái {status}");
                    
                // Nếu thanh toán hoàn tất, thêm thông tin cho việc in hóa đơn vào response
                if (orderStatus == OrderStatus.Completed)
                {
                    return Json(new { 
                        success = true,
                        completed = true,
                        orderId = id,
                        printUrl = Url.Action("PrintBill", new { id = id }),
                        customerBillUrl = Url.Action("Details", "CustomerBill", new { id = id })
                    });
                }
                
                return Json(new { success = true });
                }
                
                return Json(new { success = false, message = "Trạng thái không hợp lệ" });
        }

        private async Task<IActionResult> ReloadCreateView(StaffOrderViewModel viewModel)
        {
            // Reload categories and menu items for dropdown
            viewModel.Categories = await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
                
            viewModel.MenuItems = await _context.MenuItems
                .Where(m => m.IsAvailable)
                .OrderBy(m => m.Category.DisplayOrder)
                .ThenBy(m => m.DisplayOrder)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.Name} - {m.Price:N0}đ"
                })
                .ToListAsync();

            return View("Create", viewModel);
        }

        // Get price for selected menu item (for AJAX)
        [HttpGet]
        public async Task<IActionResult> GetMenuItemPrice(int menuItemId)
        {
            var menuItem = await _context.MenuItems.FindAsync(menuItemId);
            if (menuItem == null)
            {
                return NotFound();
            }

            return Json(new { price = menuItem.Price });
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestOrders()
        {
            var orders = await _context.DishOrders
                .Include(o => o.Order)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();
            
            return PartialView("_OrdersList", orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems(int? categoryId, string? searchTerm)
        {
            var query = _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.IsAvailable);
        
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(m => m.CategoryId == categoryId);
            }
        
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(searchTerm));
            }
        
            var menuItems = await query
                .OrderBy(m => m.Category.DisplayOrder)
                .ThenBy(m => m.DisplayOrder)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    price = m.Price,
                    category = m.Category.Name
                })
                .ToListAsync();
        
            return Json(menuItems);
        }

        [HttpGet]
        public async Task<IActionResult> PrintBill(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null)
            {
                return NotFound();
            }
            
            var dishOrders = await _context.DishOrders
                .Where(d => d.OrderId == id && d.Status != DishOrderStatus.Cancelled)
                .ToListAsync();
                
            if (!dishOrders.Any())
            {
                return NotFound("Không tìm thấy món ăn nào trong đơn hàng này.");
            }
            
            var billContent = await GenerateBill(order, dishOrders);
            
            // Trả về view hiển thị hóa đơn có thể in
            return View("Bill", billContent);
        }
        
        private async Task<string> GenerateBill(Order order, List<DishOrder> dishOrders)
        {
            var restaurant = await _context.RestaurantInfo.FirstOrDefaultAsync() 
                ?? new RestaurantInfo 
                { 
                    Name = "Quán Vịt Lộn",
                    Address = "221B Nguyễn Văn Cừ",
                    Phone = "0379665639",
                    Description = "Quán vịt lộn ngon nhất Sài Gòn",
                    LogoUrl = "/images/logo.png", 
                    WelcomeMessage = "Chào mừng quý khách",
                    GoodbyeMessage = "Cảm ơn quý khách",
                    Email = "example@email.com",
                    TaxId = "123456789"
                };
                
            var sb = new System.Text.StringBuilder();
            
            // Thêm meta charset UTF-8 để đảm bảo hiển thị đúng tiếng Việt
            sb.AppendLine("<meta charset=\"UTF-8\">");
            
            // Thông tin nhà hàng
            sb.AppendLine($"<h4 class=\"text-center\">{HttpUtility.HtmlEncode(restaurant.Name)}</h4>");
            sb.AppendLine($"<p class=\"text-center\">{HttpUtility.HtmlEncode(restaurant.Address)}</p>");
            sb.AppendLine($"<p class=\"text-center\">SĐT: {HttpUtility.HtmlEncode(restaurant.Phone)}</p>");
            sb.AppendLine("<hr>");
            
            // Thông tin hóa đơn
            sb.AppendLine("<div class=\"row mb-2\">");
            sb.AppendLine($"<div class=\"col-6\"><strong>Số HĐ:</strong> #{order.Id}</div>");
            sb.AppendLine($"<div class=\"col-6 text-end\"><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</div>");
            sb.AppendLine("</div>");
            
            sb.AppendLine($"<div class=\"mb-2\"><strong>Bàn:</strong> {HttpUtility.HtmlEncode(order.TableNumber)}</div>");
            sb.AppendLine("<hr>");
            
            // Chi tiết món ăn
            sb.AppendLine("<table class=\"table table-sm table-striped\">");
            sb.AppendLine("<thead><tr><th>Món ăn</th><th class=\"text-center\">SL</th><th class=\"text-end\">Đơn giá</th><th class=\"text-end\">Thành tiền</th></tr></thead>");
            sb.AppendLine("<tbody>");
            
            decimal total = 0;
            foreach (var item in dishOrders)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{HttpUtility.HtmlEncode(item.Name)}</td>");
                sb.AppendLine($"<td class=\"text-center\">{item.Quantity}</td>");
                sb.AppendLine($"<td class=\"text-end\">{item.Price:N0}đ</td>");
                sb.AppendLine($"<td class=\"text-end\">{item.TotalPrice:N0}đ</td>");
                sb.AppendLine("</tr>");
                
                total += item.TotalPrice;
            }
            
            sb.AppendLine("</tbody>");
            sb.AppendLine("<tfoot>");
            sb.AppendLine("<tr class=\"fw-bold\">");
            sb.AppendLine("<td colspan=\"3\" class=\"text-end\">Tổng tiền:</td>");
            sb.AppendLine($"<td class=\"text-end\">{total:N0}đ</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</tfoot>");
            sb.AppendLine("</table>");
            
            // Ghi chú
            if (!string.IsNullOrEmpty(order.Notes))
            {
                sb.AppendLine("<div class=\"mt-3 p-2 bg-light rounded\">");
                sb.AppendLine($"<p class=\"mb-0\"><strong>Ghi chú:</strong> {HttpUtility.HtmlEncode(order.Notes)}</p>");
                sb.AppendLine("</div>");
            }
            
            // Lời cảm ơn
            sb.AppendLine("<div class=\"text-center mt-4\">");
            sb.AppendLine($"<p>{HttpUtility.HtmlEncode(restaurant.WelcomeMessage)}</p>");
            sb.AppendLine($"<p>{HttpUtility.HtmlEncode(restaurant.GoodbyeMessage)}</p>");
            sb.AppendLine("</div>");
            
            return sb.ToString();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            
            // Kiểm tra nếu đơn hàng đã hoàn thành thì không cho phép sửa
            if (order.Status == OrderStatus.Completed)
            {
                TempData["ErrorMessage"] = "Không thể sửa đơn hàng đã hoàn thành thanh toán.";
                return RedirectToAction(nameof(Index));
            }
            
            // Lấy danh sách món ăn hiện tại của đơn hàng (bao gồm cả món đã hoàn thành)
            var dishOrders = await _context.DishOrders
                .Where(d => d.OrderId == id && d.Status != DishOrderStatus.Cancelled)
                .ToListAsync();

            // Kiểm tra xem có món đã hoàn thành chưa, để hiển thị cảnh báo
            var hasCompletedItems = dishOrders.Any(d => d.Status == DishOrderStatus.Completed);
                
            // Tạo view model để hiển thị
            var viewModel = new StaffOrderViewModel
            {
                Categories = await _context.Categories
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync(),
                OrderType = dishOrders.FirstOrDefault()?.OrderType ?? OrderType.DineIn,
                TableNumber = order.TableNumber,
                Note = order.Notes,
                OrderItems = dishOrders.Select(d => new OrderItemViewModel
                {
                    MenuItemId = d.MenuItemId ?? 0,
                    Name = d.Name,
                    Price = d.Price,
                    Quantity = d.Quantity,
                    ItemNote = d.Notes,
                    IsCompleted = d.Status == DishOrderStatus.Completed,
                    DishOrderId = d.Id
                }).ToList(),
                OrderId = id,
                HasCompletedItems = hasCompletedItems
            };
            
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StaffOrderViewModel viewModel)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            
            // Kiểm tra nếu đơn hàng đã hoàn thành thì không cho phép sửa
            if (order.Status == OrderStatus.Completed)
            {
                TempData["ErrorMessage"] = "Không thể sửa đơn hàng đã hoàn thành thanh toán.";
                return RedirectToAction(nameof(Index));
            }
            
            if (viewModel.OrderItems == null || !viewModel.OrderItems.Any())
            {
                _logger.LogWarning("No order items provided");
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một món ăn vào đơn hàng");
                return await ReloadEditView(id, viewModel);
            }
            
            try
            {
                _logger.LogInformation("Updating staff order {orderId} with {count} items", id, viewModel.OrderItems.Count);
                
                // Cập nhật thông tin của order
                order.TableNumber = viewModel.TableNumber ?? "Mang về";
                order.Notes = viewModel.Note;
                
                // Lấy danh sách các món hiện tại
                var existingDishOrders = await _context.DishOrders
                    .Where(d => d.OrderId == id)
                    .ToListAsync();
                
                // Lưu các món đã bị cancel để không xóa chúng
                var cancelledDishOrders = existingDishOrders
                    .Where(d => d.Status == DishOrderStatus.Cancelled)
                    .ToList();
                
                // Lưu các món đã hoàn thành để không xóa chúng
                var completedDishOrders = existingDishOrders
                    .Where(d => d.Status == DishOrderStatus.Completed)
                    .ToList();
                
                // Xóa các món không bị cancel và chưa hoàn thành (sẽ được thay thế bằng danh sách mới)
                var dishOrdersToRemove = existingDishOrders
                    .Where(d => d.Status != DishOrderStatus.Cancelled && d.Status != DishOrderStatus.Completed)
                    .ToList();
                
                _context.DishOrders.RemoveRange(dishOrdersToRemove);
                
                // Thêm các món mới từ viewModel (chỉ những món không phải đã hoàn thành)
                var newItemsToAdd = viewModel.OrderItems.Where(item => !item.IsCompleted).ToList();

                foreach (var item in newItemsToAdd)
                {
                    var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                    if (menuItem == null) 
                    {
                        _logger.LogWarning("Menu item with ID {id} not found", item.MenuItemId);
                        continue;
                    }

                    // Xử lý ghi chú
                    string itemNotes = "";
                    if (!string.IsNullOrEmpty(viewModel.Note) && !string.IsNullOrEmpty(item.ItemNote))
                    {
                        itemNotes = $"{viewModel.Note} - {item.ItemNote}";
                    }
                    else if (!string.IsNullOrEmpty(viewModel.Note))
                    {
                        itemNotes = viewModel.Note;
                    }
                    else if (!string.IsNullOrEmpty(item.ItemNote))
                    {
                        itemNotes = item.ItemNote;
                    }

                    var dishOrder = new DishOrder
                    {
                        Name = menuItem.Name,
                        Price = menuItem.Price,
                        Quantity = item.Quantity,
                        TotalPrice = item.Quantity * menuItem.Price,
                        OrderType = viewModel.OrderType,
                        Notes = itemNotes,
                        OrderTime = DateTime.Now,
                        Status = DishOrderStatus.Pending,
                        OrderId = id,
                        MenuItemId = menuItem.Id
                    };

                    _context.Add(dishOrder);
                    _logger.LogInformation("Added dish order: {name}, Quantity: {quantity}, Note: {note}, OrderId: {orderId}", 
                        dishOrder.Name, dishOrder.Quantity, dishOrder.Notes, dishOrder.OrderId);
                }

                // Cập nhật món đã hoàn thành từ form (chỉ cập nhật số lượng và ghi chú)
                var completedItemsFromForm = viewModel.OrderItems.Where(item => item.IsCompleted).ToList();
                foreach (var item in completedItemsFromForm)
                {
                    if (item.DishOrderId <= 0) continue;
                    
                    var existingItem = completedDishOrders.FirstOrDefault(d => d.Id == item.DishOrderId);
                    if (existingItem != null)
                    {
                        // Chỉ cập nhật số lượng và ghi chú, không thay đổi món
                        existingItem.Quantity = item.Quantity;
                        existingItem.TotalPrice = item.Quantity * existingItem.Price;
                        
                        // Xử lý ghi chú
                        string itemNotes = "";
                        if (!string.IsNullOrEmpty(viewModel.Note) && !string.IsNullOrEmpty(item.ItemNote))
                        {
                            itemNotes = $"{viewModel.Note} - {item.ItemNote}";
                        }
                        else if (!string.IsNullOrEmpty(viewModel.Note))
                        {
                            itemNotes = viewModel.Note;
                        }
                        else if (!string.IsNullOrEmpty(item.ItemNote))
                        {
                            itemNotes = item.ItemNote;
                        }
                        
                        existingItem.Notes = itemNotes;
                    }
                }
                
                // Cập nhật tổng tiền
                order.TotalAmount = completedDishOrders.Sum(d => d.TotalPrice) + 
                                    newItemsToAdd.Sum(item => item.Quantity * item.Price);
                
                // Tiến hành lưu các thay đổi
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("SaveChangesAsync completed with {count} changes", result);
                
                if (result <= 0)
                {
                    _logger.LogWarning("No changes were saved to database");
                    ModelState.AddModelError("", "Không có thay đổi nào được lưu vào cơ sở dữ liệu");
                    return await ReloadEditView(id, viewModel);
                }
                
                TempData["SuccessMessage"] = "Đơn hàng đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff order");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật đơn hàng: " + ex.Message);
                return await ReloadEditView(id, viewModel);
            }
        }
        
        private async Task<IActionResult> ReloadEditView(int orderId, StaffOrderViewModel viewModel)
        {
            // Reload categories cho dropdown
            viewModel.Categories = await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
                
            viewModel.OrderId = orderId;
            return View("Edit", viewModel);
        }
    }
} 