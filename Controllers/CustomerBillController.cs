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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;
using System.Web;
using Font = iTextSharp.text.Font;
using Rectangle = iTextSharp.text.Rectangle;
using System.Drawing.Imaging;

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
            var restaurantInfo = new RestaurantInfo
            {
                Name = "Quán Hiển,Vịt Lộn - Cút lộn",
                Description = "Quán vịt lộn ngon nhất thành phố",
                Address = "354 Lê Văn Thọ, phường 11, quận Gò Vấp, TP HCM",
                Phone = "0379665639",
                Email = "quanvitlon@gmail.com",
                LogoUrl = "/images/logo.png",
                WelcomeMessage = "Chào mừng quý khách đến với Quán Vịt Lộn",
                GoodbyeMessage = "Cảm ơn quý khách đã ghé thăm",
                TaxId = "1234567890"
            };

            // Tạo URL để tải PDF
            var pdfUrl = Url.Action("DownloadPdf", "CustomerBill", new { id = order.Id }, Request.Scheme);
            
            // Tạo view model chứa thông tin hóa đơn
            var viewModel = new CustomerBillViewModel
            {
                OrderId = order.Id.ToString(),
                CustomerName = order.CustomerName ?? "Khách vãng lai",
                PhoneNumber = order.PhoneNumber ?? "Không có",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Items = dishOrders.Select(d => new BillItemViewModel
                {
                    Name = d.Name,
                    Quantity = d.Quantity,
                    UnitPrice = d.Price,
                    TotalPrice = d.TotalPrice
                }).ToList(),
                RestaurantInfo = restaurantInfo,
                QrCodeImageBase64 = GenerateQrCode(pdfUrl ?? ""),
                TableNumber = order.TableNumber,
                Notes = order.Notes
            };

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
            var restaurantInfo = new RestaurantInfo
            {
                Name = "Quán Vịt Lộn",
                Description = "Quán vịt lộn ngon nhất thành phố",
                Address = "354 Lê Văn Thọ, phường 11, quận Gò Vấp, TP HCM",
                Phone = "0379665639",
                Email = "contact@quanvitlon.com",
                LogoUrl = "/images/logo.png",
                WelcomeMessage = "Chào mừng quý khách đến với Quán Vịt Lộn",
                GoodbyeMessage = "Cảm ơn quý khách đã ghé thăm",
                TaxId = "1234567890"
            };

            // Tạo file PDF
            var pdfBytes = GeneratePdfBill(order, dishOrders, restaurantInfo);
            
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
            using MemoryStream ms = new MemoryStream();
            Document document = new Document(PageSize.A4, 36, 36, 54, 36);
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            
            document.Open();
            
            // Thiết lập font chữ với encoding CP1252 (WINANSI)
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font titleFont = new Font(bf, 16, Font.BOLD);
            Font subtitleFont = new Font(bf, 14, Font.BOLD);
            Font headerFont = new Font(bf, 12, Font.BOLD);
            Font normalFont = new Font(bf, 11);
            Font smallFont = new Font(bf, 10);
            
            // Hàm chuyển đổi tiếng Việt sang không dấu để hiển thị đúng
            string ConvertToASCII(string text)
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
            
            // --- TIÊU ĐỀ HÓA ĐƠN ---
            PdfPTable headerTable = new PdfPTable(1);
            headerTable.WidthPercentage = 100;
            headerTable.SpacingAfter = 10;
            
            PdfPCell headerCell = new PdfPCell(new Phrase(ConvertToASCII("HÓA ĐƠN THANH TOÁN"), titleFont));
            headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
            headerCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            headerCell.PaddingBottom = 10;
            headerTable.AddCell(headerCell);
            
            document.Add(headerTable);
            
            // --- THÔNG TIN NHÀ HÀNG VÀ ĐƠN HÀNG ---
            PdfPTable infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            float[] columnWidths = new float[] { 50f, 50f };
            infoTable.SetWidths(columnWidths);
            infoTable.SpacingAfter = 20;
            
            // Cột bên trái: Thông tin nhà hàng
            PdfPCell restaurantCell = new PdfPCell();
            restaurantCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            restaurantCell.PaddingBottom = 10;
            
            Paragraph restName = new Paragraph(ConvertToASCII("QUÁN VỊT LỘN"), headerFont);
            restaurantCell.AddElement(restName);
            
            Paragraph restAddress = new Paragraph(ConvertToASCII("354 Lê Văn Thọ, phường 11, quận Gò Vấp, TP HCM"), normalFont);
            restaurantCell.AddElement(restAddress);
            
            Paragraph restContact = new Paragraph(ConvertToASCII($"SĐT: {restaurant.Phone}"), normalFont);
            restaurantCell.AddElement(restContact);
            
            Paragraph restEmail = new Paragraph($"Email: {restaurant.Email}", normalFont);
            restaurantCell.AddElement(restEmail);
            
            infoTable.AddCell(restaurantCell);
            
            // Cột bên phải: Thông tin đơn hàng
            PdfPCell orderInfoCell = new PdfPCell();
            orderInfoCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            orderInfoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            
            Paragraph orderTitle = new Paragraph(ConvertToASCII("THÔNG TIN ĐƠN HÀNG"), headerFont);
            orderTitle.Alignment = Element.ALIGN_RIGHT;
            orderInfoCell.AddElement(orderTitle);
            
            Paragraph orderNum = new Paragraph(ConvertToASCII($"Số hóa đơn: #{order.Id}"), normalFont);
            orderNum.Alignment = Element.ALIGN_RIGHT;
            orderInfoCell.AddElement(orderNum);
            
            Paragraph orderDate = new Paragraph(ConvertToASCII($"Ngày: {order.OrderDate:dd/MM/yyyy HH:mm}"), normalFont);
            orderDate.Alignment = Element.ALIGN_RIGHT;
            orderInfoCell.AddElement(orderDate);
            
            Paragraph tableNum = new Paragraph(ConvertToASCII($"Bàn: {order.TableNumber}"), normalFont);
            tableNum.Alignment = Element.ALIGN_RIGHT;
            orderInfoCell.AddElement(tableNum);
            
            infoTable.AddCell(orderInfoCell);
            
            document.Add(infoTable);
            
            // --- BẢNG CHI TIẾT MÓN ĂN ---
            PdfPTable itemsTable = new PdfPTable(5);
            itemsTable.WidthPercentage = 100;
            float[] itemsWidths = new float[] { 10f, 40f, 15f, 15f, 20f };
            itemsTable.SetWidths(itemsWidths);
            itemsTable.SpacingAfter = 15;
            
            // Header của bảng
            BaseColor headerBgColor = new BaseColor(210, 226, 242); // Màu xanh nhạt tương đương table-primary
            
            // STT
            PdfPCell headerCell1 = new PdfPCell(new Phrase("#", headerFont));
            headerCell1.HorizontalAlignment = Element.ALIGN_CENTER;
            headerCell1.VerticalAlignment = Element.ALIGN_MIDDLE;
            headerCell1.BackgroundColor = headerBgColor;
            headerCell1.PaddingTop = 8;
            headerCell1.PaddingBottom = 8;
            itemsTable.AddCell(headerCell1);
            
            // Món ăn
            PdfPCell headerCell2 = new PdfPCell(new Phrase(ConvertToASCII("Món ăn"), headerFont));
            headerCell2.HorizontalAlignment = Element.ALIGN_CENTER;
            headerCell2.VerticalAlignment = Element.ALIGN_MIDDLE;
            headerCell2.BackgroundColor = headerBgColor;
            headerCell2.PaddingTop = 8;
            headerCell2.PaddingBottom = 8;
            itemsTable.AddCell(headerCell2);
            
            // SL
            PdfPCell headerCell3 = new PdfPCell(new Phrase("SL", headerFont));
            headerCell3.HorizontalAlignment = Element.ALIGN_CENTER;
            headerCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
            headerCell3.BackgroundColor = headerBgColor;
            headerCell3.PaddingTop = 8;
            headerCell3.PaddingBottom = 8;
            itemsTable.AddCell(headerCell3);
            
            // Đơn giá
            PdfPCell headerCell4 = new PdfPCell(new Phrase(ConvertToASCII("Đơn giá"), headerFont));
            headerCell4.HorizontalAlignment = Element.ALIGN_CENTER;
            headerCell4.VerticalAlignment = Element.ALIGN_MIDDLE;
            headerCell4.BackgroundColor = headerBgColor;
            headerCell4.PaddingTop = 8;
            headerCell4.PaddingBottom = 8;
            itemsTable.AddCell(headerCell4);
            
            // Thành tiền
            PdfPCell headerCell5 = new PdfPCell(new Phrase(ConvertToASCII("Thành tiền"), headerFont));
            headerCell5.HorizontalAlignment = Element.ALIGN_CENTER;
            headerCell5.VerticalAlignment = Element.ALIGN_MIDDLE;
            headerCell5.BackgroundColor = headerBgColor;
            headerCell5.PaddingTop = 8;
            headerCell5.PaddingBottom = 8;
            itemsTable.AddCell(headerCell5);
            
            // Chi tiết món ăn
            decimal total = 0;
            BaseColor stripedColor = new BaseColor(249, 249, 249); // Màu xám rất nhạt cho dòng sọc
            
            for (int i = 0; i < dishOrders.Count; i++)
            {
                var item = dishOrders[i];
                BaseColor rowColor = (i % 2 == 1) ? stripedColor : BaseColor.WHITE;
                
                // STT
                PdfPCell itemCell1 = new PdfPCell(new Phrase((i + 1).ToString(), normalFont));
                itemCell1.HorizontalAlignment = Element.ALIGN_CENTER;
                itemCell1.VerticalAlignment = Element.ALIGN_MIDDLE;
                itemCell1.PaddingTop = 6;
                itemCell1.PaddingBottom = 6;
                itemCell1.BackgroundColor = rowColor;
                itemsTable.AddCell(itemCell1);
                
                // Tên món
                PdfPCell itemCell2 = new PdfPCell(new Phrase(ConvertToASCII(item.Name), normalFont));
                itemCell2.VerticalAlignment = Element.ALIGN_MIDDLE;
                itemCell2.PaddingTop = 6;
                itemCell2.PaddingBottom = 6;
                itemCell2.BackgroundColor = rowColor;
                itemsTable.AddCell(itemCell2);
                
                // Số lượng
                PdfPCell itemCell3 = new PdfPCell(new Phrase(item.Quantity.ToString(), normalFont));
                itemCell3.HorizontalAlignment = Element.ALIGN_CENTER;
                itemCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
                itemCell3.PaddingTop = 6;
                itemCell3.PaddingBottom = 6;
                itemCell3.BackgroundColor = rowColor;
                itemsTable.AddCell(itemCell3);
                
                // Đơn giá
                PdfPCell itemCell4 = new PdfPCell(new Phrase($"{item.Price:N0}d", normalFont));
                itemCell4.HorizontalAlignment = Element.ALIGN_RIGHT;
                itemCell4.VerticalAlignment = Element.ALIGN_MIDDLE;
                itemCell4.PaddingTop = 6;
                itemCell4.PaddingBottom = 6;
                itemCell4.BackgroundColor = rowColor;
                itemsTable.AddCell(itemCell4);
                
                // Thành tiền
                PdfPCell itemCell5 = new PdfPCell(new Phrase($"{item.TotalPrice:N0}d", normalFont));
                itemCell5.HorizontalAlignment = Element.ALIGN_RIGHT;
                itemCell5.VerticalAlignment = Element.ALIGN_MIDDLE;
                itemCell5.PaddingTop = 6;
                itemCell5.PaddingBottom = 6;
                itemCell5.BackgroundColor = rowColor;
                itemsTable.AddCell(itemCell5);
                
                total += item.TotalPrice;
            }
            
            // Dòng tổng tiền
            PdfPCell totalLabelCell = new PdfPCell(new Phrase(ConvertToASCII("Tổng tiền:"), headerFont));
            totalLabelCell.Colspan = 4;
            totalLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalLabelCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            totalLabelCell.PaddingTop = 8;
            totalLabelCell.PaddingBottom = 8;
            totalLabelCell.Border = iTextSharp.text.Rectangle.TOP_BORDER;
            itemsTable.AddCell(totalLabelCell);
            
            PdfPCell totalValueCell = new PdfPCell(new Phrase($"{total:N0}d", headerFont));
            totalValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalValueCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            totalValueCell.PaddingTop = 8;
            totalValueCell.PaddingBottom = 8;
            totalValueCell.Border = iTextSharp.text.Rectangle.TOP_BORDER;
            itemsTable.AddCell(totalValueCell);
            
            document.Add(itemsTable);
            
            // --- GHI CHÚ ---
            if (!string.IsNullOrEmpty(order.Notes))
            {
                PdfPTable notesTable = new PdfPTable(1);
                notesTable.WidthPercentage = 100;
                notesTable.SpacingAfter = 20;
                
                PdfPCell notesCell = new PdfPCell(new Phrase(ConvertToASCII($"Ghi chú: {order.Notes}"), normalFont));
                notesCell.Border = iTextSharp.text.Rectangle.BOX;
                notesCell.BackgroundColor = new BaseColor(248, 249, 250); // Màu xám nhạt giống alert-light
                notesCell.PaddingTop = 8;
                notesCell.PaddingBottom = 8;
                notesCell.PaddingLeft = 10;
                notesCell.PaddingRight = 10;
                notesTable.AddCell(notesCell);
                
                document.Add(notesTable);
            }
            
            // --- FOOTER: LỜI CẢM ƠN ---
            PdfPTable footerTable = new PdfPTable(1);
            footerTable.WidthPercentage = 100;
            footerTable.SpacingBefore = 10;
            
            PdfPCell thankYouCell = new PdfPCell();
            thankYouCell.Border = iTextSharp.text.Rectangle.TOP_BORDER;
            thankYouCell.PaddingTop = 10;
            thankYouCell.PaddingBottom = 5;
            
            Paragraph thanks = new Paragraph(ConvertToASCII("Cảm ơn quý khách đã ghé quán!"), normalFont);
            thanks.Alignment = Element.ALIGN_CENTER;
            thankYouCell.AddElement(thanks);
            
            Paragraph goodbye = new Paragraph(ConvertToASCII("Hẹn gặp lại quý khách!"), normalFont);
            goodbye.Alignment = Element.ALIGN_CENTER;
            thankYouCell.AddElement(goodbye);
            
            footerTable.AddCell(thankYouCell);
            document.Add(footerTable);
            
            // Đóng document
            document.Close();
            
            return ms.ToArray();
        }
    }
} 