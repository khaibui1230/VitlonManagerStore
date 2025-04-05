using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.Models
{
    public class Table
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Số bàn")]
        public string TableNumber { get; set; }

        [Display(Name = "Khu vực")]
        public string Area { get; set; }

        [Display(Name = "Trạng thái")]
        public TableStatus Status { get; set; } = TableStatus.Available;

        [Display(Name = "Sức chứa")]
        [Range(1, 20)]
        public int Capacity { get; set; }

        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; }
    }
    
    public enum TableStatus
    {
        [Display(Name = "Trống")]
        Available,
        
        [Display(Name = "Đã đặt")]
        Reserved,
        
        [Display(Name = "Đang phục vụ")]
        Occupied
    }
}