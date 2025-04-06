using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuanVitLonManager.Services;
using Npgsql;
using QuanVitLonManager.Data;
using Microsoft.EntityFrameworkCore;

namespace QuanVitLonManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _roleManager = roleManager;
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
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation($"Attempting to sign in user {model.UserName}");
                    
                    // Kiểm tra nếu người dùng tồn tại trước khi đăng nhập
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    if (user == null)
                    {
                        _logger.LogWarning($"User with username {model.UserName} not found");
                        ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
                        return View(model);
                    }
                    
                    // Tiến hành đăng nhập
                    var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"User {model.UserName} logged in successfully");
                        return LocalRedirect(returnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        _logger.LogInformation($"User {model.UserName} requires two-factor authentication");
                        return RedirectToPage("/Account/LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning($"User {model.UserName} account locked out");
                        return RedirectToPage("/Account/Lockout");
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid login attempt for {model.UserName}");
                        ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    var npgsqlException = ex.InnerException as Npgsql.PostgresException;
                    if (npgsqlException != null)
                    {
                        _logger.LogError($"PostgreSQL error during login: {npgsqlException.Message}, SQL State: {npgsqlException.SqlState}, Detail: {npgsqlException.Detail}");
                        
                        // Kiểm tra lỗi cột không tồn tại
                        if (npgsqlException.SqlState == "42703") // column does not exist
                        {
                            _logger.LogError($"Column does not exist error: {npgsqlException.MessageText}");
                            // Thực hiện chức năng tạo bảng tại đây nếu cần
                            await RecreateUserTablesAsync();
                            TempData["ErrorMessage"] = "Đăng nhập thất bại do lỗi cấu trúc dữ liệu. Hệ thống đã tự động sửa lỗi, vui lòng thử lại.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = $"Đăng nhập thất bại do lỗi cơ sở dữ liệu: {npgsqlException.MessageText}";
                        }
                    }
                    else
                    {
                        _logger.LogError(ex, "Error during login process");
                        TempData["ErrorMessage"] = "Đăng nhập thất bại do lỗi hệ thống. Vui lòng thử lại sau.";
                    }
                    
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private async Task RecreateUserTablesAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to recreate user tables...");
                
                var connection = _context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();
                    
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
-- Recreate AspNetUsers table with all columns
CREATE TABLE IF NOT EXISTS ""AspNetUsers_New"" (
    ""Id"" text NOT NULL,
    ""UserName"" character varying(256) NULL,
    ""NormalizedUserName"" character varying(256) NULL,
    ""Email"" character varying(256) NULL,
    ""NormalizedEmail"" character varying(256) NULL,
    ""EmailConfirmed"" boolean NOT NULL,
    ""PasswordHash"" text NULL,
    ""SecurityStamp"" text NULL,
    ""ConcurrencyStamp"" text NULL,
    ""PhoneNumber"" text NULL,
    ""PhoneNumberConfirmed"" boolean NOT NULL,
    ""TwoFactorEnabled"" boolean NOT NULL,
    ""LockoutEnd"" timestamp with time zone NULL,
    ""LockoutEnabled"" boolean NOT NULL,
    ""AccessFailedCount"" integer NOT NULL,
    ""FirstName"" text NULL,
    ""LastName"" text NULL,
    ""Address"" text NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ""PK_AspNetUsers_New"" PRIMARY KEY (""Id"")
);

-- Copy data if the original table exists
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'AspNetUsers') THEN
        BEGIN
            INSERT INTO ""AspNetUsers_New"" (
                ""Id"", ""UserName"", ""NormalizedUserName"", ""Email"", ""NormalizedEmail"", 
                ""EmailConfirmed"", ""PasswordHash"", ""SecurityStamp"", ""ConcurrencyStamp"", 
                ""PhoneNumber"", ""PhoneNumberConfirmed"", ""TwoFactorEnabled"", 
                ""LockoutEnd"", ""LockoutEnabled"", ""AccessFailedCount"")
            SELECT 
                ""Id"", ""UserName"", ""NormalizedUserName"", ""Email"", ""NormalizedEmail"", 
                ""EmailConfirmed"", ""PasswordHash"", ""SecurityStamp"", ""ConcurrencyStamp"", 
                ""PhoneNumber"", ""PhoneNumberConfirmed"", ""TwoFactorEnabled"", 
                ""LockoutEnd"", ""LockoutEnabled"", ""AccessFailedCount""
            FROM ""AspNetUsers"";
            
            -- Drop references to old table
            ALTER TABLE IF EXISTS ""AspNetUserClaims"" DROP CONSTRAINT IF EXISTS ""FK_AspNetUserClaims_AspNetUsers_UserId"";
            ALTER TABLE IF EXISTS ""AspNetUserLogins"" DROP CONSTRAINT IF EXISTS ""FK_AspNetUserLogins_AspNetUsers_UserId"";
            ALTER TABLE IF EXISTS ""AspNetUserRoles"" DROP CONSTRAINT IF EXISTS ""FK_AspNetUserRoles_AspNetUsers_UserId"";
            ALTER TABLE IF EXISTS ""AspNetUserTokens"" DROP CONSTRAINT IF EXISTS ""FK_AspNetUserTokens_AspNetUsers_UserId"";
            
            -- Drop old table and rename new one
            DROP TABLE IF EXISTS ""AspNetUsers"";
            ALTER TABLE ""AspNetUsers_New"" RENAME TO ""AspNetUsers"";
            
            -- Recreate indexes
            CREATE INDEX IF NOT EXISTS ""EmailIndex"" ON ""AspNetUsers"" (""NormalizedEmail"");
            CREATE UNIQUE INDEX IF NOT EXISTS ""UserNameIndex"" ON ""AspNetUsers"" (""NormalizedUserName"") WHERE ""NormalizedUserName"" IS NOT NULL;
            
            -- Recreate foreign key constraints
            ALTER TABLE ""AspNetUserClaims"" ADD CONSTRAINT ""FK_AspNetUserClaims_AspNetUsers_UserId"" 
                FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE;
                
            ALTER TABLE ""AspNetUserLogins"" ADD CONSTRAINT ""FK_AspNetUserLogins_AspNetUsers_UserId"" 
                FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE;
                
            ALTER TABLE ""AspNetUserRoles"" ADD CONSTRAINT ""FK_AspNetUserRoles_AspNetUsers_UserId"" 
                FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE;
                
            ALTER TABLE ""AspNetUserTokens"" ADD CONSTRAINT ""FK_AspNetUserTokens_AspNetUsers_UserId"" 
                FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE;
        EXCEPTION
            WHEN OTHERS THEN
                RAISE NOTICE 'Error occurred during table migration: %', SQLERRM;
        END;
    ELSE
        ALTER TABLE ""AspNetUsers_New"" RENAME TO ""AspNetUsers"";
        
        -- Create indexes
        CREATE INDEX IF NOT EXISTS ""EmailIndex"" ON ""AspNetUsers"" (""NormalizedEmail"");
        CREATE UNIQUE INDEX IF NOT EXISTS ""UserNameIndex"" ON ""AspNetUsers"" (""NormalizedUserName"") WHERE ""NormalizedUserName"" IS NOT NULL;
    END IF;
END $$;";

                    command.ExecuteNonQuery();
                    _logger.LogInformation("User tables recreated successfully");
                    
                    // Tạo tài khoản admin mặc định nếu chưa có
                    var adminUser = await _userManager.FindByEmailAsync("admin@example.com");
                    if (adminUser == null)
                    {
                        var admin = new ApplicationUser
                        {
                            UserName = "admin@example.com",
                            Email = "admin@example.com",
                            EmailConfirmed = true,
                            PhoneNumber = "0123456789",
                            PhoneNumberConfirmed = true,
                            FirstName = "Admin",
                            LastName = "System"
                        };
                        
                        var result = await _userManager.CreateAsync(admin, "Admin@123");
                        if (result.Succeeded)
                        {
                            var roleExists = await _roleManager.RoleExistsAsync("Admin");
                            if (!roleExists)
                            {
                                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                            }
                            
                            await _userManager.AddToRoleAsync(admin, "Admin");
                            _logger.LogInformation("Default admin user recreated successfully");
                        }
                        else
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            _logger.LogError($"Error recreating admin user: {errors}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user tables recreation");
            }
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