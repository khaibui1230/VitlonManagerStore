using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using System.Security.Claims;

namespace QuanVitLonManager.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int menuItemId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                ModelState.AddModelError("Rating", "Đánh giá phải từ 1 đến 5 sao");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                ModelState.AddModelError("Comment", "Vui lòng nhập nội dung đánh giá");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi đánh giá. Vui lòng kiểm tra lại thông tin.";
                return RedirectToAction("Details", "Menu", new { id = menuItemId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Kiểm tra xem người dùng đã đánh giá món ăn này chưa
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.MenuItemId == menuItemId && r.UserId == userId);

            if (existingReview != null)
            {
                // Cập nhật đánh giá hiện có
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.CreatedDate = DateTime.Now;
                
                _context.Update(existingReview);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Cảm ơn bạn đã cập nhật đánh giá!";
            }
            else
            {
                // Kiểm tra xem món ăn có tồn tại không
                var menuItem = await _context.MenuItems.FindAsync(menuItemId);
                if (menuItem == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem người dùng đã mua món ăn này chưa
                bool hasOrdered = await _context.OrderDetails
                    .Include(od => od.Order)
                    .AnyAsync(od => od.MenuItemId == menuItemId && od.Order.UserId == userId);

                // Tạo đánh giá mới
                var review = new Review
                {
                    MenuItemId = menuItemId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedDate = DateTime.Now,
                    IsVerifiedPurchase = hasOrdered
                };

                _context.Add(review);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá món ăn!";
            }

            return RedirectToAction("Details", "Menu", new { id = menuItemId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Kiểm tra xem người dùng có quyền xóa đánh giá này không
            if (review.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            int menuItemId = review.MenuItemId;
            
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Đã xóa đánh giá thành công!";
            
            return RedirectToAction("Details", "Menu", new { id = menuItemId });
        }
    }
} 