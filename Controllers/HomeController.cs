using Microsoft.AspNetCore.Mvc;
using QuanVitLonManager.Models;
using QuanVitLonManager.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.ViewModels;  // Add this line at the top with other using statements
using Microsoft.AspNetCore.Authorization;

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
            // Load categories with their menu items
            var categories = await _context.Categories
                .Include(c => c.MenuItems)
                .ToListAsync();
                
            return View(categories);
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
