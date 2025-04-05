using QuanVitLonManager.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class ReservationViewModel
    {
        [Required]
        public required Reservation Reservation { get; set; }

        [Required]
        public required List<Table> AvailableTables { get; set; } = new List<Table>();
    }
}