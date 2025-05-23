using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanVitLonManager.Models
{
    public enum CartOrderType
    {
        [Display(Name = "Ăn tại chỗ")]
        DineIn,
        
        [Display(Name = "Mang về")]
        TakeAway
    }

    public class CartItem
    {
        public int CartItemId { get; set; }
        
        public int MenuItemId { get; set; }
        
        [ForeignKey("MenuItemId")]
        public MenuItem? MenuItem { get; set; }
        
        public int Quantity { get; set; }
        
        [Column(TypeName = "nvarchar(max)")]
        public string? Notes { get; set; }

        [NotMapped]
        public CartOrderType OrderType { get; set; } = CartOrderType.DineIn;
        
        [NotMapped]
        public string? TableNumber { get; set; }
        
        [NotMapped]
        public decimal Price => MenuItem?.Price ?? 0;
        
        [NotMapped]
        public decimal SubTotal => MenuItem != null ? MenuItem.Price * Quantity : 0;

        [Required]
        [Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; } = string.Empty;
        
        public int? UserCartId { get; set; }
        
        [ForeignKey("UserCartId")]
        public UserCart? UserCart { get; set; }
    }
}