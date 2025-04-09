using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class AdminInfoViewModel
    {
        public bool IsAdmin { get; set; }
        [Required]
        public required string UserEmail { get; set; }
        [Required]
        public required string Message { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsStaff { get; set; }
        public bool IsChef { get; set; }
    }
} 