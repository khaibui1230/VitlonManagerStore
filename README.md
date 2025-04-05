
# 🍗 Quán Vịt Lộn Manager

![.NET Core](https://img.shields.io/badge/.NET%20Core-9.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-production-green)

Hệ thống quản lý nhà hàng chuyên nghiệp dành cho quán vịt lộn, được xây dựng bằng ASP.NET Core MVC.

## ✨ Tính năng chính

### 👥 Quản lý người dùng
- Đăng nhập/Đăng ký (hỗ trợ Google, Zalo)
- Phân quyền: Admin, Nhân viên, Đầu bếp, Khách hàng
- Quản lý thông tin cá nhân

### 🍽️ Quản lý thực đơn
- Thêm/sửa/xóa món ăn
- Phân loại món ăn
- Upload hình ảnh món ăn
- Đánh dấu món đặc biệt/bán chạy

### 🛒 Giỏ hàng & Đặt món
- Thêm món vào giỏ hàng
- Chỉnh sửa số lượng
- Ghi chú cho từng món
- Chọn hình thức: Tại quán/Mang về

### 📋 Quản lý đơn hàng
- Theo dõi trạng thái đơn hàng
- Thông báo realtime cho đầu bếp
- In hóa đơn
- Lịch sử đơn hàng

### 🪑 Quản lý bàn
- Đặt bàn trước
- Theo dõi tình trạng bàn
- Quản lý đặt chỗ

### 📊 Báo cáo & Thống kê
- Doanh thu theo thời gian
- Món ăn bán chạy
- Thống kê khách hàng
- Báo cáo chi tiết

## 🚀 Công nghệ sử dụng

- **Backend:** ASP.NET Core 9.0, Entity Framework Core
- **Frontend:** Bootstrap 5, jQuery, SignalR
- **Database:** SQL Server
- **Authentication:** ASP.NET Core Identity, Google OAuth, Zalo OAuth
- **Cloud Storage:** Azure Blob Storage (hình ảnh)
- **Realtime:** SignalR
- **Payment:** VNPay (đang phát triển)

## 📦 Cài đặt

1. Clone repository:
```bash
git clone https://github.com/khaibui1230/VitlonManagerStore.git
```

2. Cài đặt dependencies:
```bash
dotnet restore
```

3. Cập nhật database:
```bash
dotnet ef database update
```

4. Chạy ứng dụng:
```bash
dotnet run
```

## 🔧 Cấu hình

1. Cập nhật connection string trong `appsettings.json`
2. Cấu hình OAuth (Google, Zalo) trong `appsettings.json`
3. Thiết lập các biến môi trường cần thiết

## 👥 Vai trò người dùng

### 🎩 Admin
- Quản lý toàn bộ hệ thống
- Xem báo cáo thống kê
- Quản lý nhân viên

### 👨‍🍳 Đầu bếp
- Nhận thông báo đơn hàng mới
- Cập nhật trạng thái món
- Quản lý queue đơn hàng

### 💼 Nhân viên
- Tiếp nhận đơn hàng
- Quản lý bàn
- In hóa đơn

### 🧑‍🤝‍🧑 Khách hàng
- Đặt món
- Theo dõi đơn hàng
- Đặt bàn

## 📱 Giao diện

<div style="display: flex; gap: 10px;">
    <img src="screenshots/home.png" width="200" alt="Trang chủ">
    <img src="screenshots/menu.png" width="200" alt="Thực đơn">
    <img src="screenshots/cart.png" width="200" alt="Giỏ hàng">
    <img src="screenshots/admin.png" width="200" alt="Trang admin">
</div>

## 🤝 Đóng góp

Mọi đóng góp đều được chào đón! Hãy:

1. Fork dự án
2. Tạo branch mới (`git checkout -b feature/AmazingFeature`)
3. Commit thay đổi (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📝 License

[MIT License](LICENSE)

## 📧 Liên hệ

Khải Bùi - [@khaibui1230](https://github.com/khaibui1230) - khaibui0402@gmail.com

Project Link: [https://github.com/khaibui1230/VitlonManagerStore](https://github.com/khaibui1230/VitlonManagerStore) 
=======

