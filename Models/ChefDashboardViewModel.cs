using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.Models
{
    public class ChefDashboardViewModel
    {
        [Required]
        public required List<DishItemViewModel> DishItems { get; set; } = new();
        
        [Required]
        public required string FilterStatus { get; set; }
        
        public DateTime LastRefreshTime { get; set; } = DateTime.Now;
    }

    public class DishItemViewModel
    {
        [Required]
        public int DishId { get; set; }
        
        [Required]
        public required string DishName { get; set; }
        
        [Required]
        public int TotalQuantity { get; set; }
        
        [Required]
        public required List<string> Notes { get; set; } = new();
        
        [Required]
        public required string Status { get; set; }
        
        [Required]
        public required List<int> OrderIds { get; set; } = new();
        
        [Required]
        public required List<int> DishOrderIds { get; set; } = new();
        
        [Required]
        public DateTime OrderTime { get; set; }
    }
} 