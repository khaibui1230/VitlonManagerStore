using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Display(Name = "Vai trò")]
        public List<string> UserRoles { get; set; } = new List<string>();
        
        public List<string> AllRoles { get; set; } = new List<string>();
        
        [Display(Name = "Vai trò được chọn")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}