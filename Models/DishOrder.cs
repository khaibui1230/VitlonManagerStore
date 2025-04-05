using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanVitLonManager.Models
{
    public class DishOrder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên món")]
        [Display(Name = "Tên món")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá tiền")]
        [Display(Name = "Giá tiền")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tiền phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức")]
        [Display(Name = "Hình thức")]
        public OrderType OrderType { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Thời gian đặt")]
        public DateTime OrderTime { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        public DishOrderStatus Status { get; set; } = DishOrderStatus.Pending;

        // Add this property to the DishOrder class
        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 1;

        // Liên kết với Order (nếu có)
        public int? OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        
        // Có thể thêm liên kết với MenuItem nếu cần
        public int? MenuItemId { get; set; }
        
        [ForeignKey("MenuItemId")]
        public MenuItem? MenuItem { get; set; }
    }

    public enum OrderType
    {
        [Display(Name = "Ăn tại quán")]
        DineIn,
        
        [Display(Name = "Mang về")]
        TakeAway
    }

    public enum DishOrderStatus
    {
        [Display(Name = "Đang chờ")]
        Pending,
        
        [Display(Name = "Đang chuẩn bị")]
        Preparing,
        
        [Display(Name = "Hoàn thành")]
        Completed,
        
        [Display(Name = "Đã hủy")]
        Cancelled
    }
}