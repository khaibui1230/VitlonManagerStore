using QuanVitLonManager.Models;

namespace QuanVitLonManager.ViewModels
{
    public class MenuViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<MenuItem> MenuItems { get; set; }
    }
}