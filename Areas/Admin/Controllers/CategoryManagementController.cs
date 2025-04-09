using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanVitLonManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/CategoryManagement
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DisplayOrderSortParam"] = sortOrder == "display_order" ? "display_order_desc" : "display_order";
            ViewData["SearchString"] = searchString;

            var categories = from c in _context.Categories
                           select c;

            // Áp dụng filter
            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.Name.Contains(searchString) || 
                                               c.Description.Contains(searchString));
            }

            // Áp dụng sắp xếp
            switch (sortOrder)
            {
                case "name_desc":
                    categories = categories.OrderByDescending(c => c.Name);
                    break;
                case "display_order":
                    categories = categories.OrderBy(c => c.DisplayOrder);
                    break;
                case "display_order_desc":
                    categories = categories.OrderByDescending(c => c.DisplayOrder);
                    break;
                default:
                    categories = categories.OrderBy(c => c.Name);
                    break;
            }

            return View(await categories.ToListAsync());
        }

        // GET: Admin/CategoryManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/CategoryManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/CategoryManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Danh mục đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/CategoryManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/CategoryManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Danh mục đã được cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/CategoryManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (category == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có bất kỳ món ăn nào trong danh mục này không
            var menuItemCount = await _context.MenuItems.CountAsync(m => m.CategoryId == id);
            ViewData["MenuItemCount"] = menuItemCount;

            return View(category);
        }

        // POST: Admin/CategoryManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra xem có bất kỳ món ăn nào trong danh mục này không
            var menuItemCount = await _context.MenuItems.CountAsync(m => m.CategoryId == id);
            if (menuItemCount > 0)
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục này vì có món ăn liên quan!";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Danh mục đã được xóa thành công!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/CategoryManagement/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();
            
            var status = category.IsActive ? "kích hoạt" : "vô hiệu hóa";
            TempData["SuccessMessage"] = $"Danh mục đã được {status} thành công!";
            
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
} 