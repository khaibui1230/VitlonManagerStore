using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace QuanVitLonManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public MenuManagementController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Admin/MenuManagement
        public async Task<IActionResult> Index(string searchString, int? categoryId, string sortOrder, int page = 1)
        {
            int pageSize = 10;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSortParam"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["CategorySortParam"] = sortOrder == "category" ? "category_desc" : "category";
            ViewData["SearchString"] = searchString;
            ViewData["CategoryId"] = categoryId;

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            // Lấy tất cả dữ liệu
            var menuItems = await _context.MenuItems
                .Include(m => m.Category)
                .ToListAsync();

            // Áp dụng filter trong bộ nhớ
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                menuItems = menuItems.Where(m => 
                    m.Name.ToLower().Contains(searchString) || 
                    (m.Description != null && m.Description.ToLower().Contains(searchString)))
                    .ToList();
            }

            if (categoryId.HasValue)
            {
                menuItems = menuItems.Where(m => m.CategoryId == categoryId.Value).ToList();
            }

            // Áp dụng sắp xếp trong bộ nhớ
            menuItems = sortOrder switch
            {
                "name_desc" => menuItems.OrderByDescending(m => m.Name).ToList(),
                "price" => menuItems.OrderBy(m => m.Price).ToList(),
                "price_desc" => menuItems.OrderByDescending(m => m.Price).ToList(),
                "category" => menuItems.OrderBy(m => m.Category.Name).ToList(),
                "category_desc" => menuItems.OrderByDescending(m => m.Category.Name).ToList(),
                _ => menuItems.OrderBy(m => m.Name).ToList()
            };

            // Tính toán phân trang
            var count = menuItems.Count;
            var items = menuItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["TotalPages"] = (int)Math.Ceiling(count / (double)pageSize);
            ViewData["CurrentPage"] = page;

            return View(items);
        }

        // GET: Admin/MenuManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (menuItem == null)
            {
                return NotFound();
            }

            // Thiết lập ViewBag.IsFeatured để tránh lỗi null reference
            ViewBag.IsFeatured = false; // Bạn có thể thay đổi giá trị này từ DB nếu cần

            // Có thể thêm thống kê của món ăn nếu cần
            // Ví dụ: số lượng đã bán, doanh thu, v.v.
            ViewBag.Statistics = new
            {
                TotalSold = 0, // Thay bằng giá trị thực từ DB
                TotalRevenue = 0, // Thay bằng giá trị thực từ DB
                LastOrdered = (DateTime?)null // Thay bằng giá trị thực từ DB
            };

            return View(menuItem);
        }

        // GET: Admin/MenuManagement/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
                
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.IsFeatured = false;
            return View();
        }

        // POST: Admin/MenuManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItem menuItem, IFormFile? ImageFile)
        {
            // Kiểm tra và thêm thông báo lỗi cụ thể cho từng trường
            ValidateMenuItemFields(menuItem, ImageFile);
            
            // Xử lý upload ảnh trước khi kiểm tra ModelState
            if (ImageFile != null && ImageFile.Length > 0)
            {
                try
                {
                    // Tạo tên file duy nhất để tránh trùng lặp
                    string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                    string extension = Path.GetExtension(ImageFile.FileName);
                    fileName = $"{fileName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{extension}";
                    
                    // Tạo đường dẫn lưu file
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "menu");
                    
                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    
                    // Lưu file vào thư mục
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }
                    
                    // Cập nhật đường dẫn ảnh vào model
                    menuItem.ImageUrl = $"/images/menu/{fileName}";
                    Console.WriteLine($"New image uploaded: {menuItem.ImageUrl}");
                }
                catch (Exception ex)
                {
                    // Log lỗi và thêm lỗi vào ModelState
                    ModelState.AddModelError("ImageFile", $"Lỗi khi tải lên ảnh: {ex.Message}");
                    Console.WriteLine($"Error uploading image: {ex.Message}");
                }
            }

            // Đặt ModelState.IsValid = true nếu đang bị lỗi do tải ảnh
            if (!ModelState.IsValid && ModelState.ErrorCount == 1 && ModelState.ContainsKey("ImageFile"))
            {
                ModelState.Clear();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(menuItem);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Successfully created menu item");
                    TempData["SuccessMessage"] = "Món ăn đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving menu item: {ex.Message}");
                    TempData["ErrorMessage"] = $"Lỗi khi lưu món ăn: {ex.Message}";
                }
            }
            
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
                
            ViewBag.Categories = new SelectList(categories, "Id", "Name", menuItem.CategoryId);
            ViewBag.IsFeatured = false;
            
            // Thêm thông báo lỗi nếu có vấn đề với ModelState
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                Console.WriteLine($"ModelState errors: {errors}");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi tạo món ăn: {errors}";
            }
            
            return View(menuItem);
        }

        // Phương thức riêng để xác thực dữ liệu của MenuItem
        private void ValidateMenuItemFields(MenuItem menuItem, IFormFile? imageFile, bool isEdit = false)
        {
            // Kiểm tra tên món ăn
            if (string.IsNullOrWhiteSpace(menuItem.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên món ăn");
            }
            
            // Kiểm tra giá
            if (menuItem.Price <= 0)
            {
                ModelState.AddModelError("Price", "Vui lòng nhập giá món ăn hợp lệ (lớn hơn 0)");
            }
            
            // Kiểm tra danh mục
            if (menuItem.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục món ăn");
            }
            
            // Kiểm tra hình ảnh - Chỉ bắt buộc khi tạo mới, khi chỉnh sửa có thể giữ nguyên ảnh cũ
            if (!isEdit && (imageFile == null || imageFile.Length == 0))
            {
                if (string.IsNullOrEmpty(menuItem.ImageUrl))
                {
                    ModelState.AddModelError("ImageFile", "Vui lòng tải lên hình ảnh cho món ăn");
                }
            }
            
            // Kiểm tra định dạng và kích thước hình ảnh nếu có
            if (imageFile != null && imageFile.Length > 0)
            {
                // Kiểm tra kích thước (giới hạn 5MB)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Kích thước hình ảnh không được vượt quá 5MB");
                }
                
                // Kiểm tra định dạng
                string extension = Path.GetExtension(imageFile.FileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Chỉ chấp nhận các định dạng hình ảnh: .jpg, .jpeg, .png, .gif");
                }
            }

            // Kiểm tra các trường khác nếu cần
            if (menuItem.OriginalPrice < 0)
            {
                ModelState.AddModelError("OriginalPrice", "Giá gốc không được âm");
            }
            
            if (menuItem.DiscountPercentage < 0 || menuItem.DiscountPercentage > 100)
            {
                ModelState.AddModelError("DiscountPercentage", "Phần trăm giảm giá phải từ 0 đến 100");
            }
        }

        // GET: Admin/MenuManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }
            
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
                
            ViewBag.Categories = new SelectList(categories, "Id", "Name", menuItem.CategoryId);
            ViewBag.IsFeatured = false; // Bạn có thể thiết lập giá trị mặc định này, hoặc lấy từ DB nếu có
            return View(menuItem);
        }

        // POST: Admin/MenuManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItem menuItem, IFormFile? ImageFile)
        {
            if (id != menuItem.Id)
            {
                return NotFound();
            }

            // Lấy thông tin menu item hiện tại từ database để giữ thông tin ImageUrl nếu không có ảnh mới
            var existingMenuItem = await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (existingMenuItem == null)
            {
                return NotFound();
            }

            // Log để kiểm tra
            Console.WriteLine($"Existing ImageUrl: {existingMenuItem.ImageUrl}");
            Console.WriteLine($"Form ImageUrl: {menuItem.ImageUrl}");

            // Giữ lại đường dẫn ảnh cũ nếu không tải lên ảnh mới
            if (ImageFile == null || ImageFile.Length == 0)
            {
                menuItem.ImageUrl = existingMenuItem.ImageUrl;
                Console.WriteLine($"Using existing ImageUrl: {menuItem.ImageUrl}");
            }
            else 
            {
                // Xử lý upload ảnh mới
                try
                {
                    // Nếu đã có ảnh cũ, xóa ảnh cũ (tuỳ chọn)
                    if (!string.IsNullOrEmpty(existingMenuItem.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingMenuItem.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    
                    // Tạo tên file duy nhất
                    string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                    string extension = Path.GetExtension(ImageFile.FileName);
                    fileName = $"{fileName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{extension}";
                    
                    // Tạo đường dẫn lưu file
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "menu");
                    
                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    
                    // Lưu file vào thư mục
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }
                    
                    // Cập nhật đường dẫn ảnh vào model
                    menuItem.ImageUrl = $"/images/menu/{fileName}";
                    Console.WriteLine($"New ImageUrl: {menuItem.ImageUrl}");
                }
                catch (Exception ex)
                {
                    // Log lỗi và thêm lỗi vào ModelState
                    ModelState.AddModelError("ImageFile", $"Lỗi khi tải lên ảnh: {ex.Message}");
                    // Giữ lại ảnh cũ nếu có lỗi xảy ra
                    menuItem.ImageUrl = existingMenuItem.ImageUrl;
                    Console.WriteLine($"Error uploading image: {ex.Message}");
                }
            }

            // Kiểm tra và thêm thông báo lỗi cụ thể cho các trường
            ValidateMenuItemFields(menuItem, ImageFile, isEdit: true);

            // Đặt ModelState.IsValid = true nếu đang bị lỗi do tải ảnh
            if (!ModelState.IsValid && ModelState.ErrorCount == 1 && ModelState.ContainsKey("ImageFile"))
            {
                ModelState.Clear();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(menuItem);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Successfully updated menu item");
                    TempData["SuccessMessage"] = "Món ăn đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine($"Concurrency error: {ex.Message}");
                    if (!MenuItemExists(menuItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error during update: {ex.Message}");
                    TempData["ErrorMessage"] = $"Lỗi cập nhật: {ex.Message}";
                }
            }
            
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
                
            ViewBag.Categories = new SelectList(categories, "Id", "Name", menuItem.CategoryId);
            ViewBag.IsFeatured = false;
            
            // Thêm thông báo lỗi nếu có vấn đề với ModelState
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                Console.WriteLine($"ModelState errors: {errors}");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi cập nhật món ăn: {errors}";
            }
            
            return View(menuItem);
        }

        // GET: Admin/MenuManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (menuItem == null)
            {
                return NotFound();
            }

            // Kiểm tra xem món ăn đã được sử dụng trong đơn hàng nào chưa
            bool hasOrders = await _context.OrderDetails.AnyAsync(od => od.MenuItemId == id);
            ViewBag.HasOrders = hasOrders;
            ViewBag.IsFeatured = false; // Thiết lập cho trang Delete cũng nếu cần
            return View(menuItem);
        }

        // POST: Admin/MenuManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                // Xóa file ảnh nếu có
                if (!string.IsNullOrEmpty(menuItem.ImageUrl))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, menuItem.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                
                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Món ăn đã được xóa thành công!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/MenuManagement/ToggleAvailability/5
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            menuItem.IsAvailable = !menuItem.IsAvailable;
            _context.Update(menuItem);
            await _context.SaveChangesAsync();
            
            string status = menuItem.IsAvailable ? "có sẵn" : "hết hàng";
            TempData["SuccessMessage"] = $"Món \"{menuItem.Name}\" đã được chuyển sang trạng thái {status}!";
            
            return RedirectToAction(nameof(Index));
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
} 