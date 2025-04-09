using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class CartItemViewModel
    {
        [Required]
        public int MenuItemId { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        public string? Notes { get; set; }
    }
} 