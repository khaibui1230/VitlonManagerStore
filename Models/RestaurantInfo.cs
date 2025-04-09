using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.Models
{
    public class RestaurantInfo
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Name { get; set; } = "Quán Hiển - Vịt Lộn-Cút lộn";
        
        [Required]
        [StringLength(255)]
        public required string Address { get; set; } = "354 Lê Văn Thọ, phường 11, quận Gò Vấp, TP HCM";
        
        [StringLength(20)]
        public required string Phone { get; set; } = "0379665639";
        
        [StringLength(100)]
        [EmailAddress]
        public required string Email { get; set; } = "vitlonhien@gmail.com";
        
        [Required]
        public required string Description { get; set; }
        
        [StringLength(20)]
        [Required]
        public required string TaxId { get; set; } = "";
        
        [StringLength(255)]
        [Required]
        public required string LogoUrl { get; set; } = "";
        
        [StringLength(255)]
        [Required]
        public required string WelcomeMessage { get; set; } = "Cảm ơn quý khách đã sử dụng dịch vụ!";
        
        [StringLength(255)]
        [Required]
        public required string GoodbyeMessage { get; set; } = "Hẹn gặp lại quý khách lần sau!";
    }
} 