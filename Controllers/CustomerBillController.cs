using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuanVitLonManager.Data;
using QuanVitLonManager.Models;
using QuanVitLonManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Web;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuanVitLonManager.Controllers
{
    public class CustomerBillController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerBillController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CustomerBill/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null)
            {
                return NotFound();
            }
            
            var dishOrders = await _context.DishOrders
                .Where(d => d.OrderId == id && d.Status != DishOrderStatus.Cancelled)
                .ToListAsync();
                
            if (!dishOrders.Any())
            {
                return NotFound("Không tìm thấy món ăn nào trong đơn hàng này.");
            }

            // Lấy thông tin nhà hàng
            var restaurant = await _context.RestaurantInfo.FirstOrDefaultAsync() 
                ?? new RestaurantInfo 
                { 
                    Name = "Quán ",
                    Address = "221B Nguyễn Văn Cừ",
                    Phone = "0379665639",
                    Email = "example@email.com",
                    TaxId = "123456789"
                };

            // Tạo view model chứa thông tin hóa đơn
            var viewModel = new CustomerBillViewModel
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TableNumber = order.TableNumber,
                Notes = order.Notes,
                TotalAmount = order.TotalAmount,
                Items = dishOrders.Select(d => new BillItemViewModel
                {
                    Name = d.Name,
                    Quantity = d.Quantity,
                    Price = d.Price,
                    TotalPrice = d.TotalPrice
                }).ToList(),
                RestaurantInfo = restaurant
            };

            // Tạo URL để tải PDF
            var pdfUrl = Url.Action("DownloadPdf", "CustomerBill", new { id = order.Id }, Request.Scheme);
            
            // Tạo QR code
            viewModel.QrCodeImageBase64 = GenerateQrCode(pdfUrl);

            return View(viewModel);
        }

        // Xử lý tải hóa đơn PDF
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null)
            {
                return NotFound();
            }
            
            var dishOrders = await _context.DishOrders
                .Where(d => d.OrderId == id && d.Status != DishOrderStatus.Cancelled)
                .ToListAsync();
                
            if (!dishOrders.Any())
            {
                return NotFound("Không tìm thấy món ăn nào trong đơn hàng này.");
            }

            // Lấy thông tin nhà hàng
            var restaurant = await _context.RestaurantInfo.FirstOrDefaultAsync() 
                ?? new RestaurantInfo 
                { 
                    Name = "Quán Vịt Lộn",
                    Address = "221B Nguyễn Văn Cừ",
                    Phone = "0379665639",
                    Email = "example@email.com",
                    TaxId = "123456789"
                };

            // Tạo file PDF
            var pdfBytes = GeneratePdfBill(order, dishOrders, restaurant);
            
            // Trả về file PDF
            return File(pdfBytes, "application/pdf", $"hoa-don-{order.Id}.pdf");
        }

        // Tạo mã QR code
        private string GenerateQrCode(string content)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }

        // Tạo file PDF
        private byte[] GeneratePdfBill(Order order, List<DishOrder> dishOrders, RestaurantInfo restaurant)
        {
            // Tạo HTML content cho hóa đơn
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; }");
            html.AppendLine("th, td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Header
            html.AppendLine($"<h1 style='text-align: center;'>{restaurant.Name}</h1>");
            html.AppendLine($"<p style='text-align: center;'>{restaurant.Address}</p>");
            html.AppendLine($"<p style='text-align: center;'>SDT: {restaurant.Phone}</p>");
            
            // Order Info
            html.AppendLine("<div style='margin: 20px 0;'>");
            html.AppendLine($"<p>Số HD: {order.Id}</p>");
            html.AppendLine($"<p>Ngày: {order.OrderDate:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine($"<p>Bàn số: {order.TableNumber}</p>");
            html.AppendLine("</div>");
            
            // Items Table
            html.AppendLine("<table>");
            html.AppendLine("<tr>");
            html.AppendLine("<th>Tên món</th>");
            html.AppendLine("<th>Số lượng</th>");
            html.AppendLine("<th>Đơn giá</th>");
            html.AppendLine("<th>Thành tiền</th>");
            html.AppendLine("</tr>");
            
            foreach (var item in dishOrders)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{item.Name}</td>");
                html.AppendLine($"<td>{item.Quantity}</td>");
                html.AppendLine($"<td>{item.Price:#,##0}</td>");
                html.AppendLine($"<td>{item.TotalPrice:#,##0}</td>");
                html.AppendLine("</tr>");
            }
            html.AppendLine("</table>");
            
            // Total
            html.AppendLine($"<p style='text-align: right; margin-top: 20px;'><strong>Tổng tiền: {order.TotalAmount:#,##0} VND</strong></p>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            // Convert HTML to PDF using browser's print functionality
            return Encoding.UTF8.GetBytes(html.ToString());
        }

        private string ConvertToASCII(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            text = text.Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
                .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
                .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
                .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
                .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
                .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y")
                .Replace("đ", "d")
                .Replace("Á", "A").Replace("À", "A").Replace("Ả", "A").Replace("Ã", "A").Replace("Ạ", "A")
                .Replace("Ă", "A").Replace("Ắ", "A").Replace("Ằ", "A").Replace("Ẳ", "A").Replace("Ẵ", "A").Replace("Ặ", "A")
                .Replace("Â", "A").Replace("Ấ", "A").Replace("Ầ", "A").Replace("Ẩ", "A").Replace("Ẫ", "A").Replace("Ậ", "A")
                .Replace("É", "E").Replace("È", "E").Replace("Ẻ", "E").Replace("Ẽ", "E").Replace("Ẹ", "E")
                .Replace("Ê", "E").Replace("Ế", "E").Replace("Ề", "E").Replace("Ể", "E").Replace("Ễ", "E").Replace("Ệ", "E")
                .Replace("Í", "I").Replace("Ì", "I").Replace("Ỉ", "I").Replace("Ĩ", "I").Replace("Ị", "I")
                .Replace("Ó", "O").Replace("Ò", "O").Replace("Ỏ", "O").Replace("Õ", "O").Replace("Ọ", "O")
                .Replace("Ô", "O").Replace("Ố", "O").Replace("Ồ", "O").Replace("Ổ", "O").Replace("Ỗ", "O").Replace("Ộ", "O")
                .Replace("Ơ", "O").Replace("Ớ", "O").Replace("Ờ", "O").Replace("Ở", "O").Replace("Ỡ", "O").Replace("Ợ", "O")
                .Replace("Ú", "U").Replace("Ù", "U").Replace("Ủ", "U").Replace("Ũ", "U").Replace("Ụ", "U")
                .Replace("Ư", "U").Replace("Ứ", "U").Replace("Ừ", "U").Replace("Ử", "U").Replace("Ữ", "U").Replace("Ự", "U")
                .Replace("Ý", "Y").Replace("Ỳ", "Y").Replace("Ỷ", "Y").Replace("Ỹ", "Y").Replace("Ỵ", "Y")
                .Replace("Đ", "D");
            
            return text;
        }
    }
} 