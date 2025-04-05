using QuanVitLonManager.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class MyOrdersViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        
        public bool IsAuthenticated { get; set; }
        
        public bool HasSearched { get; set; }
        
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }
    }
} 