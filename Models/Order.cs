using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanVitLonManager.Models
{
    public enum PaymentMethod
    {
        [Display(Name = "Tiền mặt")]
        Cash,
        
        [Display(Name = "Thẻ tín dụng/ghi nợ")]
        Card,
        
        [Display(Name = "Ví MoMo")]
        MoMo,
        
        [Display(Name = "Chuyển khoản")]
        Banking
    }
    
    public enum PaymentStatus
    {
        [Display(Name = "Chưa thanh toán")]
        Unpaid,
        
        [Display(Name = "Đã thanh toán")]
        Paid
    }
    
    public class Order
    {
        public int Id { get; set; }
        
        // Cho phép UserId là null (đơn hàng ẩn danh)
        public string? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        
        public int? TableId { get; set; }
        
        [ForeignKey("TableId")]
        public Table? Table { get; set; }
        
        [Display(Name = "Số bàn")]
        public string? TableNumber { get; set; }
        
        // Thêm thông tin liên hệ cho đơn hàng ẩn danh
        [Display(Name = "Tên khách hàng")]
        public string? CustomerName { get; set; }
        
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }
        
        [Required]
        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [Required]
        [Display(Name = "Tổng tiền")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
        
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        
        [Display(Name = "Trạng thái thanh toán")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        
        [Display(Name = "Thời gian thanh toán")]
        public DateTime? PaymentDate { get; set; }
        
        [Display(Name = "Loại đơn hàng")]
        public OrderType OrderType { get; set; } = OrderType.DineIn;
        
        // Navigation property
        public ICollection<OrderDetail> OrderDetails { get; set; }
        
        // Navigation property cho DishOrders
        public ICollection<DishOrder> DishOrders { get; set; }
        
        // Thuộc tính để kiểm tra nếu đơn hàng là ẩn danh
        [NotMapped]
        public bool IsAnonymous => string.IsNullOrEmpty(UserId);
    }
}