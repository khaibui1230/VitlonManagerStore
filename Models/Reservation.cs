using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanVitLonManager.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int TableId { get; set; }

        [Required]
        public DateTime ReservationTime { get; set; }

        [Required]
        public int NumberOfGuests { get; set; }

        [Required]
        public ReservationStatus Status { get; set; }

        [Required]
        [StringLength(1000)]
        public string Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Table? Table { get; set; }
    }
}