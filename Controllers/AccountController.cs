using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuanVitLonManager.Services;

namespace QuanVitLonManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Đảm bảo sử dụng đúng tên vai trò (phân biệt chữ hoa/thường)
                    await _userManager.AddToRoleAsync(user, "KhachHang");
                    
                    // Đăng nhập người dùng sau khi đăng ký
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    if (user == null)
                    {
                        _logger.LogWarning("User not found after successful login.");
                        return RedirectToAction("Index", "Home");
                    }

                    var roles = await _userManager.GetRolesAsync(user);
                    _logger.LogInformation("User roles: {0}", string.Join(", ", roles));

                    // Kiểm tra nếu người dùng có role Admin hoặc Staff
                    if (roles.Contains("QuanLy"))
                    {
                        _logger.LogInformation("Redirecting to Admin Dashboard");
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                    else if (roles.Contains("NhanVien"))
                    {
                        _logger.LogInformation("Redirecting to Staff Order");
                        return RedirectToAction("Index", "StaffOrder");
                    }
                    else if (roles.Contains("Chef"))
                    {
                        _logger.LogInformation("Redirecting to Chef with pending filter");
                        return RedirectToAction("Index", "Chef", new { filterStatus = "pending" });
                    }
                    else
                    {
                        _logger.LogInformation("Redirecting to Home page");
                        return LocalRedirect(returnUrl ?? Url.Content("~/"));
                    }
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, RememberMe = model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin đăng nhập.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            
            // Xóa giỏ hàng khi người dùng đăng xuất
            var cartService = HttpContext.RequestServices.GetService<ICartService>();
            if (cartService != null)
            {
                cartService.ClearCart();
            }
            
            return RedirectToAction("Index", "Home");
        }
    }
}