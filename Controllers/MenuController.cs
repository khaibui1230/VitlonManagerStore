using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using System.Linq;
using System.Threading.Tasks;
using QuanVitLonManager.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace QuanVitLonManager.Controllers;

public class MenuController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MenuController> _logger;

    public MenuController(ApplicationDbContext context, ILogger<MenuController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var viewModel = new MenuViewModel
            {
                Categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync(),

                MenuItems = await _context.MenuItems
                    .Include(m => m.Category)
                    .Where(m => m.IsAvailable)
                    .OrderBy(m => m.Category.DisplayOrder)
                    .ThenBy(m => m.DisplayOrder)
                    .ToListAsync()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            var pgEx = ex.InnerException as PostgresException;
            if (pgEx != null && pgEx.SqlState == "42P01") // relation does not exist
            {
                _logger.LogError(pgEx, "Table does not exist error when accessing menu");
                await CreateMenuItemsTableAsync();
                
                // Trả về view với danh sách rỗng
                var emptyViewModel = new MenuViewModel
                {
                    Categories = await _context.Categories
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.DisplayOrder)
                        .ToListAsync(),
                    MenuItems = new List<MenuItem>()
                };
                
                ViewBag.ErrorMessage = "Hệ thống đang được khởi tạo, vui lòng thử lại sau.";
                return View(emptyViewModel);
            }
            
            _logger.LogError(ex, "Error loading menu");
            var fallbackViewModel = new MenuViewModel
            {
                Categories = new List<Category>(),
                MenuItems = new List<MenuItem>()
            };
            ViewBag.ErrorMessage = "Không thể tải thực đơn. Vui lòng thử lại sau.";
            return View(fallbackViewModel);
        }
    }

    private async Task CreateMenuItemsTableAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to create MenuItems table...");
            
            var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();
                
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
-- MenuItems table
CREATE TABLE IF NOT EXISTS ""MenuItems"" (
    ""Id"" serial NOT NULL,
    ""Name"" text NOT NULL,
    ""Description"" text NULL,
    ""DetailedDescription"" text NULL,
    ""Price"" numeric(18,2) NOT NULL,
    ""OriginalPrice"" numeric(18,2) NOT NULL,
    ""DiscountPercentage"" integer NOT NULL DEFAULT 0,
    ""CategoryId"" integer NOT NULL,
    ""ImageUrl"" text NULL,
    ""Ingredients"" text NULL,
    ""PreparationInstructions"" text NULL,
    ""IsAvailable"" boolean NOT NULL DEFAULT true,
    ""DisplayOrder"" integer NOT NULL DEFAULT 0,
    ""IsPopular"" boolean NOT NULL DEFAULT false,
    ""IsNew"" boolean NOT NULL DEFAULT false,
    ""IsOnSale"" boolean NOT NULL DEFAULT false,
    ""Calories"" integer NULL,
    ""Protein"" numeric(10,2) NULL,
    ""Fat"" numeric(10,2) NULL,
    ""Carbs"" numeric(10,2) NULL,
    CONSTRAINT ""PK_MenuItems"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_MenuItems_Categories_CategoryId"" FOREIGN KEY (""CategoryId"") REFERENCES ""Categories"" (""Id"") ON DELETE CASCADE
);

-- Index for CategoryId in MenuItems
CREATE INDEX IF NOT EXISTS ""IX_MenuItems_CategoryId"" ON ""MenuItems"" (""CategoryId"");

-- Create sample menu items if needed
INSERT INTO ""MenuItems"" (""Name"", ""Description"", ""Price"", ""OriginalPrice"", ""CategoryId"", ""IsPopular"", ""DisplayOrder"")
SELECT 'Trứng vịt lộn luộc', 'Trứng vịt lộn luộc chín kỹ, giữ nguyên hương vị', 8000, 8000, c.""Id"", true, 1
FROM ""Categories"" c
WHERE c.""Name"" = 'Trứng vịt lộn' AND NOT EXISTS (SELECT 1 FROM ""MenuItems"" m WHERE m.""Name"" = 'Trứng vịt lộn luộc')
LIMIT 1;

INSERT INTO ""MenuItems"" (""Name"", ""Description"", ""Price"", ""OriginalPrice"", ""CategoryId"", ""IsPopular"", ""DisplayOrder"")
SELECT 'Trứng vịt lộn xào me', 'Trứng vịt lộn xào với nước me chua ngọt', 15000, 15000, c.""Id"", true, 2
FROM ""Categories"" c
WHERE c.""Name"" = 'Trứng vịt lộn' AND NOT EXISTS (SELECT 1 FROM ""MenuItems"" m WHERE m.""Name"" = 'Trứng vịt lộn xào me')
LIMIT 1;

INSERT INTO ""MenuItems"" (""Name"", ""Description"", ""Price"", ""OriginalPrice"", ""CategoryId"", ""IsPopular"", ""DisplayOrder"")
SELECT 'Trứng vịt lộn nướng', 'Trứng vịt lộn nướng với muối tiêu chanh', 12000, 12000, c.""Id"", true, 3
FROM ""Categories"" c
WHERE c.""Name"" = 'Trứng vịt lộn' AND NOT EXISTS (SELECT 1 FROM ""MenuItems"" m WHERE m.""Name"" = 'Trứng vịt lộn nướng')
LIMIT 1;
";
                command.ExecuteNonQuery();
                _logger.LogInformation("MenuItems table created and populated with sample data");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MenuItems table");
        }
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