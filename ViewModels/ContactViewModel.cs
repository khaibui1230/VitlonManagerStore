using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public required string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [Display(Name = "Tiêu đề")]
        public required string Subject { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        [Display(Name = "Nội dung")]
        public required string Message { get; set; }
    }
}