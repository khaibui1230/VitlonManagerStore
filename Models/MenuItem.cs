using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace QuanVitLonManager.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(4000)]
        public string? DetailedDescription { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        [NotMapped]
        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;

        [Required]
        public int DisplayOrder { get; set; }

        [StringLength(2000)]
        public string? Ingredients { get; set; }

        [StringLength(4000)]
        public string? PreparationInstructions { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Required]
        public int DiscountPercentage { get; set; }

        [NotMapped]
        public decimal CurrentPrice => Price - (Price * DiscountPercentage / 100);
        
        // Add the missing properties
        [Required]
        public bool IsNew { get; set; }
        
        [Required]
        public bool IsPopular { get; set; }
        
        [Required]
        public bool IsOnSale { get; set; }
        
        // Thông tin dinh dưỡng
        public int? Calories { get; set; }
        
        public int? Protein { get; set; }
        
        public int? Fat { get; set; }
        
        public int? Carbs { get; set; }
        
        // Chức năng đánh giá
        [NotMapped]
        public virtual ICollection<Review>? Reviews { get; set; } = new List<Review>();

        // Navigation properties
        public virtual Category? Category { get; set; }
    }
}