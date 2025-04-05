using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace QuanVitLonManager.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public required string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Mô tả chi tiết")]
        [DataType(DataType.MultilineText)]
        public string? DetailedDescription { get; set; }

        [Required]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }
        
        [NotMapped]
        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public virtual required Category Category { get; set; }

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Ingredients")]
        public string? Ingredients { get; set; }

        [Display(Name = "Preparation Instructions")]
        public string? PreparationInstructions { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        [Display(Name = "Giá Gốc")]
        [DataType(DataType.Currency)]
        public decimal OriginalPrice { get; set; }

        [Display(Name = "Giảm Giá (%)")]
        [Range(0, 100)]
        public int DiscountPercentage { get; set; }

        [NotMapped]
        public decimal CurrentPrice => Price - (Price * DiscountPercentage / 100);
        
        // Add the missing properties
        [Display(Name = "Món Mới")]
        public bool IsNew { get; set; }
        
        [Display(Name = "Món Phổ Biến")]
        public bool IsPopular { get; set; }
        
        [Display(Name = "Đang Giảm Giá")]
        public bool IsOnSale { get; set; }
        
        // Thông tin dinh dưỡng
        [Display(Name = "Calories")]
        public int? Calories { get; set; }
        
        [Display(Name = "Protein (g)")]
        public int? Protein { get; set; }
        
        [Display(Name = "Chất béo (g)")]
        public int? Fat { get; set; }
        
        [Display(Name = "Carbs (g)")]
        public int? Carbs { get; set; }
        
        // Chức năng đánh giá
        [NotMapped]
        public virtual ICollection<Review>? Reviews { get; set; } = new List<Review>();
    }
}