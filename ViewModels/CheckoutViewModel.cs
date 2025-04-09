using QuanVitLonManager.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanVitLonManager.ViewModels
{
    public class CheckoutViewModel
    {
        public CheckoutViewModel()
        {
            Order = new Order
            {
                OrderDetails = new List<OrderDetail>(),
                DishOrders = new List<DishOrder>()
            };
            CartItems = new List<CartItem>();
        }

        public Order Order { get; set; }
        public IEnumerable<CartItem> CartItems { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        [Display(Name = "Tên khách hàng")]
        public required string CustomerName { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public required string PhoneNumber { get; set; }
        
        [Required]
        public required List<CartItemViewModel> Items { get; set; } = new();
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        public string? Notes { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        // Xác định có yêu cầu đăng nhập hay không
        public bool RequiresLogin { get; set; } = false;
    }
}