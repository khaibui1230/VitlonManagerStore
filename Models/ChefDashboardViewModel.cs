using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.Models
{
    public class ChefDashboardViewModel
    {
        public List<ChefDishItem> DishItems { get; set; }
        public string FilterStatus { get; set; }
        public DateTime LastRefreshTime { get; set; } = DateTime.Now;
    }

    public class ChefDishItem
    {
        public int DishId { get; set; }
        public string DishName { get; set; }
        public int TotalQuantity { get; set; }
        public List<string> Notes { get; set; }
        public string Status { get; set; }
        public List<int> OrderIds { get; set; }
        public List<int> DishOrderIds { get; set; }
        public DateTime OrderTime { get; set; }
    }
} 