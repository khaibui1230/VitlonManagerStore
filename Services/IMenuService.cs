using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanVitLonManager.Services
{
    public interface IMenuService
    {
        Task<MenuViewModel> GetMenuViewModelAsync();
        Task<IEnumerable<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId);
        Task<MenuItem> GetMenuItemByIdAsync(int id);
        Task<IEnumerable<MenuItem>> GetRelatedMenuItemsAsync(int categoryId, int currentItemId, int count = 4);
        Task<bool> AddMenuItemAsync(MenuItem menuItem);
        Task<bool> UpdateMenuItemAsync(MenuItem menuItem);
        Task<bool> DeleteMenuItemAsync(int id);
        Task<bool> ToggleMenuItemAvailabilityAsync(int id);
    }
} 