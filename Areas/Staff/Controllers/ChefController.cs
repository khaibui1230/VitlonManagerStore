using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanVitLonManager.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin,Staff,Chef")]
    public class ChefController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChefController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Staff/Chef
        public async Task<IActionResult> Index(string statusFilter)
        {
            ViewBag.CurrentFilter = statusFilter;

            var query = _context.DishOrders.AsQueryable();

            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<DishOrderStatus>(statusFilter, out var status))
                {
                    query = query.Where(d => d.Status == status);
                }
            }
            else
            {
                // Mặc định chỉ hiển thị những món đang chờ hoặc đang chuẩn bị
                query = query.Where(d => d.Status == DishOrderStatus.Pending || d.Status == DishOrderStatus.Preparing);
            }

            // Sắp xếp và lấy dữ liệu
            var dishOrders = await query
                .Include(d => d.Order)
                .Include(d => d.MenuItem)
                .OrderBy(d => d.Status)
                .ThenBy(d => d.OrderTime)
                .ToListAsync();

            // Nhóm các món giống nhau
            var groupedDishes = dishOrders
                .GroupBy(d => new { d.Name, d.Notes })
                .Select(g => new 
                {
                    DishName = g.Key.Name,
                    Notes = g.Key.Notes,
                    TotalQuantity = g.Sum(d => d.Quantity),
                    Items = g.ToList(),
                    FirstItem = g.First()
                })
                .ToList();

            ViewBag.GroupedDishes = groupedDishes;

            return View(dishOrders);
        }

        // GET: Staff/Chef/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dishOrder = await _context.DishOrders
                .Include(d => d.Order)
                .Include(d => d.MenuItem)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dishOrder == null)
            {
                return NotFound();
            }

            return View(dishOrder);
        }

        // POST: Staff/Chef/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, DishOrderStatus status)
        {
            var dishOrder = await _context.DishOrders.FindAsync(id);
            
            if (dishOrder == null)
            {
                return NotFound();
            }

            dishOrder.Status = status;
            _context.Update(dishOrder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Staff/Chef/UpdateGroupStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGroupStatus(string dishName, string notes, DishOrderStatus status)
        {
            var dishOrders = await _context.DishOrders
                .Where(d => d.Name == dishName && 
                       (notes == null ? d.Notes == null : d.Notes == notes) && 
                       (d.Status == DishOrderStatus.Pending || d.Status == DishOrderStatus.Preparing))
                .ToListAsync();
            
            if (dishOrders == null || !dishOrders.Any())
            {
                return NotFound();
            }

            foreach (var dish in dishOrders)
            {
                dish.Status = status;
                _context.Update(dish);
            }
            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Staff/Chef/Orders
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MenuItem)
                .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Preparing)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
} 