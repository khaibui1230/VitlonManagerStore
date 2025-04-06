using Microsoft.AspNetCore.Mvc;
using QuanVitLonManager.Models;
using QuanVitLonManager.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.ViewModels;  // Add this line at the top with other using statements
using Microsoft.AspNetCore.Authorization;
using Npgsql;

namespace QuanVitLonManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading categories from database");
                
                // Try to create the Categories table if it doesn't exist
                try
                {
                    var categories = await _context.Categories
                        .Include(c => c.MenuItems)
                        .ToListAsync();
                    
                    _logger.LogInformation($"Successfully loaded {categories.Count} categories");
                    return View(categories);
                }
                catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // relation does not exist
                {
                    _logger.LogWarning("Categories table does not exist! Creating it...");
                    
                    try 
                    {
                        await _context.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS ""Categories"" (
    ""Id"" serial NOT NULL,
    ""Name"" text NOT NULL,
    ""Description"" text NULL,
    ""ImageUrl"" text NULL,
    ""DisplayOrder"" integer NOT NULL DEFAULT 0,
    ""IsActive"" boolean NOT NULL DEFAULT true,
    CONSTRAINT ""PK_Categories"" PRIMARY KEY (""Id"")
);

INSERT INTO ""Categories"" (""Name"", ""Description"", ""DisplayOrder"", ""IsActive"")
VALUES 
('Món chính', 'Các món chính trong thực đơn', 1, true),
('Món phụ', 'Các món ăn kèm', 2, true), 
('Đồ uống', 'Các loại đồ uống', 3, true),
('Tráng miệng', 'Các món tráng miệng', 4, true)
ON CONFLICT DO NOTHING;
");
                        _logger.LogInformation("Successfully created Categories table, reloading data");
                        
                        // Try loading again
                        var categories = await _context.Categories.ToListAsync();
                        return View(categories);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating Categories table");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading categories");
                // Return default data with no categories when error occurs
                return View(new List<Category>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Test()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            // Log error details
            _logger.LogError("Error occurred. RequestId: {RequestId}", errorViewModel.RequestId);

            return View(errorViewModel);
        }

        public IActionResult About()
        {
            return View();
        }

        [Authorize]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Send email or save to database
                TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi sớm nhất có thể!";
                return RedirectToAction(nameof(Contact));
            }
            return View(model);
        }
    }
}
