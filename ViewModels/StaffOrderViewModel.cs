using Microsoft.AspNetCore.Mvc.Rendering;
using QuanVitLonManager.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class StaffOrderViewModel
    {
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> MenuItems { get; set; } = new();
        public List<OrderItemViewModel> OrderItems { get; set; } = new();
        public OrderType OrderType { get; set; }
        
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
        
        [Display(Name = "Số bàn")]
        public string? TableNumber { get; set; }
        
        // ID của đơn hàng khi đang chỉnh sửa
        public int OrderId { get; set; }
        
        // Cờ đánh dấu đơn hàng có món đã hoàn thành hay không
        public bool HasCompletedItems { get; set; }
    }

    public class OrderItemViewModel
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ItemNote { get; set; }
        public decimal TotalPrice => Price * Quantity;
        
        // ID của DishOrder tương ứng (dùng để cập nhật món đã hoàn thành)
        public int DishOrderId { get; set; }
        
        // Cờ đánh dấu món ăn đã hoàn thành (không thể thay đổi món)
        public bool IsCompleted { get; set; }
    }
}