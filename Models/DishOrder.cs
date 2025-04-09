using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanVitLonManager.Models
{
    public class DishOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public OrderType OrderType { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public DateTime OrderTime { get; set; }

        [Required]
        public DishOrderStatus Status { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int? OrderId { get; set; }

        public int? MenuItemId { get; set; }

        // Navigation properties
        public virtual Order? Order { get; set; }
        public virtual MenuItem? MenuItem { get; set; }
    }
}