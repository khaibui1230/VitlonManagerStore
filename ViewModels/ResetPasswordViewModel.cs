using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự và tối đa {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;
        
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}