using QuanVitLonManager.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class MenuViewModel
    {
        [Required]
        public required List<Category> Categories { get; set; }
        
        [Required]
        public required List<MenuItem> MenuItems { get; set; }
        
        public int? SelectedCategoryId { get; set; }
        public string? SearchString { get; set; }
    }
}