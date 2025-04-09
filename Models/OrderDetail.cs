using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanVitLonManager.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;
        
        [Required]
        public int MenuItemId { get; set; }
        
        [ForeignKey("MenuItemId")]
        public MenuItem MenuItem { get; set; } = null!;
        
        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
        
        // Ghi chú cho món ăn
        public string? Notes { get; set; }

        // Computed properties for view
        [NotMapped]
        public string Name => MenuItem?.Name ?? string.Empty;

        [NotMapped]
        public decimal TotalPrice => Subtotal;
    }
}