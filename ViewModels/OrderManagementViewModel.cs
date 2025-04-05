using Microsoft.AspNetCore.Mvc.Rendering;
using QuanVitLonManager.Models;
using System.Collections.Generic;

namespace QuanVitLonManager.ViewModels
{
    public class OrderManagementViewModel
    {
        public IEnumerable<Order> Orders { get; set; } = new List<Order>();
        public string CurrentStatus { get; set; } = string.Empty;
        public string SearchTerm { get; set; } = string.Empty;
        public List<SelectListItem> StatusList { get; set; } = new List<SelectListItem>();
    }
} 