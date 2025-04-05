using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis; // Add this for AllowNull attribute

namespace QuanVitLonManager.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public required string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        [AllowNull]  // Changed from just AllowNull to the proper attribute
        public string ImageUrl { get; set; } = "/images/categories/default.jpg";
    }
}