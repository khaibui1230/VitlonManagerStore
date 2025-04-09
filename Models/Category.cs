using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis; // Add this for AllowNull attribute

namespace QuanVitLonManager.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        [AllowNull]  // Changed from just AllowNull to the proper attribute
        public string? ImageUrl { get; set; } = "/images/categories/default.jpg";

        // Navigation properties
        public virtual ICollection<MenuItem>? MenuItems { get; set; }
    }
}