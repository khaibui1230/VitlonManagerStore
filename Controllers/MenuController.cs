using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using System.Linq;
using System.Threading.Tasks;
using QuanVitLonManager.ViewModels;
namespace QuanVitLonManager.Controllers;

public class MenuController : Controller
{
    private readonly ApplicationDbContext _context;

    public MenuController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Menu
    public IActionResult Index()
    {
        var viewModel = new MenuViewModel
        {
            Categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToList(),

            MenuItems = _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.IsAvailable)
                .OrderBy(m => m.Category.DisplayOrder)
                .ThenBy(m => m.DisplayOrder)
                .ToList()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> MenuByCategory(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        var menuItems = await _context.MenuItems
            .Where(m => m.CategoryId == id && m.IsAvailable)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        ViewBag.CategoryName = category.Name;
        ViewBag.CategoryId = category.Id;

        return View(menuItems);
    }

    // GET: Menu/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Category)
            .Include(m => m.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menuItem == null)
        {
            return NotFound();
        }

        // Get related items from the same category
        var relatedItems = await _context.MenuItems
            .Where(m => m.CategoryId == menuItem.CategoryId && m.Id != menuItem.Id && m.IsAvailable)
            .Take(4)
            .ToListAsync();

        ViewBag.RelatedItems = relatedItems;

        return View(menuItem);
    }

    // GET: Menu/Create
    [Authorize(Roles = "QuanLy")]
    public async Task<IActionResult> Create(int categoryId)
    {
        ViewBag.CategoryId = categoryId;
        ViewBag.CategoryName = await _context.Categories
            .Where(c => c.Id == categoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();

        return View();
    }

    // POST: Menu/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "QuanLy")]
    public async Task<IActionResult> Create(MenuItem menuItem)
    {
        if (ModelState.IsValid)
        {
            _context.Add(menuItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MenuByCategory), new { id = menuItem.CategoryId });
        }
        ViewBag.CategoryId = menuItem.CategoryId;
        ViewBag.CategoryName = await _context.Categories
            .Where(c => c.Id == menuItem.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
        return View(menuItem);
    }

    // GET: Menu/Edit/5
    [Authorize(Roles = "QuanLy")]
    public async Task<IActionResult> Edit(int id)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menuItem == null)
        {
            return NotFound();
        }

        ViewBag.CategoryId = menuItem.CategoryId;
        ViewBag.CategoryName = menuItem.Category.Name;

        return View(menuItem);
    }

    // POST: Menu/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "QuanLy")]
    public async Task<IActionResult> Edit(int id, MenuItem menuItem)
    {
        if (id != menuItem.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(menuItem);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(menuItem.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(MenuByCategory), new { id = menuItem.CategoryId });
        }
        ViewBag.CategoryId = menuItem.CategoryId;
        ViewBag.CategoryName = await _context.Categories
            .Where(c => c.Id == menuItem.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
        return View(menuItem);
    }

    // GET: Menu/Delete/5
    [Authorize(Roles = "QuanLy")]
    public async Task<IActionResult> Delete(int id)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menuItem == null)
        {
            return NotFound();
        }

        return View(menuItem);
    }

    // POST: Menu/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    private bool MenuItemExists(int id)
    {
        return _context.MenuItems.Any(e => e.Id == id);
    }
} 