using System.Collections.Generic;

namespace QuanVitLonManager.ViewModels
{
    public class AdminInfoViewModel
    {
        public bool IsAdmin { get; set; }
        public string UserEmail { get; set; }
        public string Message { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsStaff { get; set; }
        public bool IsChef { get; set; }
    }
} 