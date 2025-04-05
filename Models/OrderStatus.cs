using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Đang chờ xác nhận")]
        Pending,
        
        [Display(Name = "Đã xác nhận")]
        Confirmed,
        
        [Display(Name = "Đang chuẩn bị")]
        Preparing,
        
        [Display(Name = "Đang giao hàng")]
        Delivering,
        
        [Display(Name = "Đang tính tiền")]
        Billing,
        
        [Display(Name = "Đã hoàn thành")]
        Completed,
        
        [Display(Name = "Đã hủy")]
        Cancelled
    }
} 