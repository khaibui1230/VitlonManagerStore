using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.Models
{
    public class RestaurantInfo
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "Quán Hiển - Vịt Lộn-Cút lộn";
        
        [Required]
        [StringLength(255)]
        public string Address { get; set; } = "354 Lê Văn Thọ, phường 11, quận Gò Vấp, TP HCM";
        
        [StringLength(20)]
        public string Phone { get; set; } = "0379665639";
        
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = "vitlonhien@gmail.com";
        
        [StringLength(20)]
        public string TaxId { get; set; } = "";
        
        [StringLength(255)]
        public string LogoUrl { get; set; } = "";
        
        [StringLength(255)]
        public string WelcomeMessage { get; set; } = "Cảm ơn quý khách đã sử dụng dịch vụ!";
        
        [StringLength(255)]
        public string GoodbyeMessage { get; set; } = "Hẹn gặp lại quý khách lần sau!";
    }
} 