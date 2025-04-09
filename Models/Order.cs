using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanVitLonManager.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int? TableId { get; set; }

        [StringLength(100)]
        public string? TableNumber { get; set; }

        [StringLength(100)]
        public string? CustomerName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Required]
        public OrderType OrderType { get; set; } = OrderType.DineIn;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        
        [ForeignKey("TableId")]
        public virtual Table? Table { get; set; }
        
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
        public virtual ICollection<DishOrder>? DishOrders { get; set; }

        // Thuộc tính để kiểm tra nếu đơn hàng là ẩn danh
        [NotMapped]
        public bool IsAnonymous => string.IsNullOrEmpty(UserId);
    }
} 