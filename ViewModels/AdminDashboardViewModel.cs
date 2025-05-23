using QuanVitLonManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalMenuItems { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<TopSellingItemViewModel> TopSellingItems { get; set; } = new List<TopSellingItemViewModel>();
        public List<DailyRevenueViewModel> DailyRevenue { get; set; } = new List<DailyRevenueViewModel>();

        [Required]
        public required string MenuItemName { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
    }

    public partial class TopSellingItemViewModel
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
} 