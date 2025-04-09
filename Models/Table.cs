using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanVitLonManager.Models
{
    public class Table
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TableNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Area { get; set; }

        [Required]
        public TableStatus Status { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        public virtual ICollection<Reservation>? Reservations { get; set; }
    }
}