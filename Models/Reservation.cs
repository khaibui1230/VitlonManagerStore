using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanVitLonManager.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public int TableId { get; set; }

        [ForeignKey("TableId")]
        public Table Table { get; set; }

        [Required]
        [Display(Name = "Thời gian đặt bàn")]
        public DateTime ReservationTime { get; set; }

        [Required]
        [Display(Name = "Số lượng khách")]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        [Display(Name = "Trạng thái")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }

        [Display(Name = "Thời gian tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    
    public enum ReservationStatus
    {
        [Display(Name = "Đang chờ xác nhận")]
        Pending,

        [Display(Name = "Đã xác nhận")]
        Confirmed,

        [Display(Name = "Đã hoàn thành")]
        Completed,

        [Display(Name = "Đã hủy")]
        Cancelled
    }
}