using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanVitLonManager.Models
{
    public class UserCart
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
} 