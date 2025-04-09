using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuanVitLonManager.Controllers
{
    public class AdminToolsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminToolsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: /AdminTools/GrantAdminRole
        public async Task<IActionResult> GrantAdminRole()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            // Kiểm tra xem người dùng đã có quyền Admin chưa
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                TempData["Error"] = "Bạn đã có quyền quản trị.";
                return RedirectToAction(nameof(CheckAdminStatus));
            }

            // Kiểm tra xem có quản trị viên nào khác không
            // Chỉ cho phép cấp quyền nếu có ít nhất một quản trị viên khác đã tồn tại
            // hoặc nếu đây là người dùng đầu tiên (không có người dùng nào có quyền quản trị)
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count > 0)
            {
                // Nếu đã có quản trị viên, chỉ quản trị viên hiện tại mới có thể cấp quyền
                TempData["Error"] = "Bạn cần yêu cầu một quản trị viên hiện tại để cấp quyền quản trị cho bạn.";
                return RedirectToAction(nameof(CheckAdminStatus));
            }

            // Đảm bảo role Admin tồn tại
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Thêm quyền Admin cho người dùng
            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                // Refresh lại đăng nhập để cập nhật claims
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Đã cấp quyền quản trị thành công!";
            }
            else
            {
                TempData["Error"] = $"Không thể cấp quyền quản trị: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(CheckAdminStatus));
        }

        // GET: /AdminTools/CheckAdminStatus
        public async Task<IActionResult> CheckAdminStatus()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            // Kiểm tra xem người dùng có quyền Admin không
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var roles = await _userManager.GetRolesAsync(user);

            return View("AdminInfo", new AdminInfoViewModel
            {
                IsAdmin = isAdmin,
                UserEmail = user.Email,
                Roles = roles.ToList(),
                Message = isAdmin ? "Bạn có quyền quản trị." : "Bạn không có quyền quản trị."
            });
        }

        // GET: /AdminTools/ManageStaffRoles
        public async Task<IActionResult> ManageStaffRoles()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            // Kiểm tra xem người dùng có quyền Admin không
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            // Đảm bảo các vai trò cần thiết tồn tại
            if (!await _roleManager.RoleExistsAsync("Staff"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Staff"));
            }
            if (!await _roleManager.RoleExistsAsync("Chef"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Chef"));
            }

            // Lấy các vai trò hiện tại của người dùng
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
            var isChef = await _userManager.IsInRoleAsync(user, "Chef");
            var roles = await _userManager.GetRolesAsync(user);

            return View("AdminInfo", new AdminInfoViewModel
            {
                IsAdmin = isAdmin,
                UserEmail = user.Email,
                Roles = roles.ToList(),
                Message = "Quản lý vai trò nhân viên",
                IsStaff = isStaff,
                IsChef = isChef
            });
        }

        // POST: /AdminTools/ToggleStaffRole
        [HttpPost]
        public async Task<IActionResult> ToggleStaffRole(string role)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            // Kiểm tra xem người dùng có quyền Admin không
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Bạn không có quyền thực hiện chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (role != "Staff" && role != "Chef")
            {
                TempData["Error"] = "Vai trò không hợp lệ.";
                return RedirectToAction("ManageStaffRoles");
            }

            // Đảm bảo vai trò tồn tại
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var hasRole = await _userManager.IsInRoleAsync(user, role);
            IdentityResult result;

            if (hasRole)
            {
                result = await _userManager.RemoveFromRoleAsync(user, role);
                if (result.Succeeded)
                {
                    TempData["Success"] = $"Đã xóa vai trò {role} thành công.";
                }
            }
            else
            {
                result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    TempData["Success"] = $"Đã thêm vai trò {role} thành công.";
                }
            }

            if (!result.Succeeded)
            {
                TempData["Error"] = $"Không thể thay đổi vai trò {role}.";
            }

            return RedirectToAction("ManageStaffRoles");
        }
    }
} 