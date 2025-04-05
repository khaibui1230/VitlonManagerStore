using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace QuanVitLonManager.Models
{
    public class Review
    {
        public int Id { get; set; }
        
        [Required]
        public int MenuItemId { get; set; }
        public virtual MenuItem MenuItem { get; set; } = null!;
        
        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Comment { get; set; } = null!;
        
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Thông tin bổ sung
        public bool IsVerifiedPurchase { get; set; } = false;
        
        [NotMapped]
        public string UserName => User?.UserName ?? "Khách hàng ẩn danh";
    }
} 