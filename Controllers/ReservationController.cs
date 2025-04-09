using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;

namespace QuanVitLonManager.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservation
        public async Task<IActionResult> Index()
        {
            // Hiển thị trang đặt bàn
            var availableTables = await _context.Tables
                .Where(t => t.Status == TableStatus.Available)
                .ToListAsync();

            var viewModel = new ReservationViewModel
            {
                AvailableTables = availableTables,
                Reservation = new Reservation
                {
                    ReservationTime = DateTime.Now.AddHours(1),
                    UserId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : string.Empty,
                    TableId = 0,
                    NumberOfGuests = 1,
                    Notes = "Không có ghi chú",
                    Status = ReservationStatus.Pending,
                    CreatedAt = DateTime.Now
                }
            };

            return View(viewModel);
        }

        // GET: Reservation/MyReservations
        [Authorize]
        public async Task<IActionResult> MyReservations()
        {
            var userId = _userManager.GetUserId(User);
            var reservations = await _context.Reservations
                .Include(r => r.Table)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();

            return View(reservations);
        }

        // POST: Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var selectedTable = await _context.Tables.FindAsync(model.Reservation.TableId);
                if (selectedTable == null || selectedTable.Status != TableStatus.Available)
                {
                    ModelState.AddModelError("", "Bàn đã được đặt hoặc không tồn tại.");
                    return View("Index", model);
                }

                // Kiểm tra xem bàn đã được đặt trong khoảng thời gian này chưa
                var requestedTime = model.Reservation.ReservationTime;
                var requestedEndTime = requestedTime.AddHours(2); // Giả sử mỗi lần đặt bàn kéo dài 2 giờ
                
                var hasConflict = await _context.Reservations
                    .Where(r => r.TableId == model.Reservation.TableId)
                    .Where(r => r.Status != ReservationStatus.Cancelled)
                    .Where(r => (r.ReservationTime <= requestedTime && requestedTime <= r.ReservationTime.AddHours(2)) ||
                                (r.ReservationTime <= requestedEndTime && requestedEndTime <= r.ReservationTime.AddHours(2)))
                    .AnyAsync();

                if (hasConflict)
                {
                    ModelState.AddModelError("", "Bàn đã được đặt trong khoảng thời gian này.");
                    return View("Index", model);
                }

                if (User.Identity.IsAuthenticated)
                {
                    var userId = _userManager.GetUserId(User);
                    var user = await _userManager.FindByIdAsync(userId);
                    
                    if (user == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy thông tin người dùng.");
                        return View("Index", model);
                    }

                    var reservation = new Reservation
                    {
                        UserId = userId,
                        User = user,
                        TableId = model.Reservation.TableId,
                        Table = selectedTable,
                        ReservationTime = model.Reservation.ReservationTime,
                        NumberOfGuests = model.Reservation.NumberOfGuests,
                        Notes = model.Reservation.Notes ?? "Không có ghi chú",
                        Status = ReservationStatus.Pending,
                        CreatedAt = DateTime.Now
                    };

                    _context.Add(reservation);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Đặt bàn thành công! Chúng tôi sẽ xác nhận sớm.";
                    return RedirectToAction(nameof(MyReservations));
                }
                else
                {
                    // Lưu thông tin đặt bàn vào session và chuyển hướng đến trang đăng nhập
                    TempData["ReservationData"] = System.Text.Json.JsonSerializer.Serialize(model.Reservation);
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("CompleteReservation", "Reservation") });
                }
            }

            // Nếu ModelState không hợp lệ, hiển thị lại form với lỗi
            var availableTables = await _context.Tables
                .Where(t => t.Status == TableStatus.Available)
                .ToListAsync();
            model.AvailableTables = availableTables;

            return View("Index", model);
        }

        // GET: Reservation/CompleteReservation
        [Authorize]
        public async Task<IActionResult> CompleteReservation()
        {
            // Lấy thông tin đặt bàn từ session sau khi đăng nhập
            if (TempData["ReservationData"] is string reservationJson)
            {
                var reservationData = System.Text.Json.JsonSerializer.Deserialize<Reservation>(reservationJson);
                
                // Kiểm tra lại xem bàn còn trống không
                var selectedTable = await _context.Tables.FindAsync(reservationData.TableId);
                if (selectedTable == null || selectedTable.Status != TableStatus.Available)
                {
                    TempData["ErrorMessage"] = "Bàn đã được đặt hoặc không tồn tại.";
                    return RedirectToAction(nameof(Index));
                }

                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                var reservation = new Reservation
                {
                    UserId = userId,
                    TableId = reservationData.TableId,
                    ReservationTime = reservationData.ReservationTime,
                    NumberOfGuests = reservationData.NumberOfGuests,
                    Notes = reservationData.Notes ?? "Không có ghi chú",
                    Status = ReservationStatus.Pending,
                    CreatedAt = DateTime.Now
                };

                _context.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đặt bàn thành công! Chúng tôi sẽ xác nhận sớm.";
                return RedirectToAction(nameof(MyReservations));
            }

            TempData["ErrorMessage"] = "Không tìm thấy thông tin đặt bàn.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Reservation/Cancel/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reservation == null)
            {
                return NotFound();
            }

            // Chỉ cho phép hủy đặt bàn nếu trạng thái là Pending hoặc Confirmed
            if (reservation.Status == ReservationStatus.Pending || reservation.Status == ReservationStatus.Confirmed)
            {
                reservation.Status = ReservationStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hủy đặt bàn thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đặt bàn ở trạng thái hiện tại.";
            }

            return RedirectToAction(nameof(MyReservations));
        }

        // GET: Reservation/CheckAvailability
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(DateTime date, int guests)
        {
            // Tìm các bàn có sức chứa phù hợp
            var tables = await _context.Tables
                .Where(t => t.Capacity >= guests)
                .ToListAsync();

            var availableTables = new List<Table>();

            foreach (var table in tables)
            {
                // Kiểm tra xem bàn có đặt trước trong ngày này không
                var isReserved = await _context.Reservations
                    .Where(r => r.TableId == table.Id)
                    .Where(r => r.Status != ReservationStatus.Cancelled)
                    .Where(r => r.ReservationTime.Date == date.Date)
                    .AnyAsync();

                if (!isReserved)
                {
                    availableTables.Add(table);
                }
            }

            return Json(new { success = true, tables = availableTables });
        }
    }
}