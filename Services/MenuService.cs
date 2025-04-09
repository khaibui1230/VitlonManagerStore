using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanVitLonManager.Services
{
    public class MenuService : IMenuService
    {
        private readonly ApplicationDbContext _context;

        public MenuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MenuViewModel> GetMenuViewModelAsync()
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

            return viewModel;
        }

        public async Task<IEnumerable<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
        {
            return await _context.MenuItems
                .Where(m => m.CategoryId == categoryId && m.IsAvailable)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        public async Task<MenuItem> GetMenuItemByIdAsync(int id)
        {
            return await _context.MenuItems
                .Include(m => m.Category)
                .Include(m => m.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<MenuItem>> GetRelatedMenuItemsAsync(int categoryId, int currentItemId, int count = 4)
        {
            return await _context.MenuItems
                .Where(m => m.CategoryId == categoryId && m.Id != currentItemId && m.IsAvailable)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> AddMenuItemAsync(MenuItem menuItem)
        {
            _context.MenuItems.Add(menuItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateMenuItemAsync(MenuItem menuItem)
        {
            _context.MenuItems.Update(menuItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
                return false;

            _context.MenuItems.Remove(menuItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleMenuItemAvailabilityAsync(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
                return false;

            menuItem.IsAvailable = !menuItem.IsAvailable;
            _context.MenuItems.Update(menuItem);
            return await _context.SaveChangesAsync() > 0;
        }
    }
} 