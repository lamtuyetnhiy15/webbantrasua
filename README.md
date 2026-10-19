# 🧋 TN Tea - Ứng Dụng Web Bán Trà Sữa (ASP.NET Core MVC)

Dự án Website bán trà sữa **TN Tea** được xây dựng chuẩn kiến trúc **ASP.NET Core MVC** (.NET 8) kết hợp **Entity Framework Core Code-First**, video banner `vdbaner.mp4` tự động phát mượt mà không âm thanh, cùng tông màu nhận diện thương hiệu Pastel Hồng `#F8D7DA` & màu mận ấm `#4A2E35`.

---

## 🎯 Tính Năng Chính

1. **Khách hàng & Đăng nhập (`AccountController`)**:
   - Đăng nhập bảo mật với xác thực Session (`Session["User"]`).
   - Đăng xuất làm mới phiên làm việc.
   - Tài khoản mẫu sẵn có:
     - Quản trị viên: `admin` / `123`
     - Khách hàng: `khachhang` / `123`

2. **Menu trà sữa (`ProductController`)**:
   - `Index`: Hero Banner chính thức (`baner.jpg`) chuẩn phong cách Nhật Bản sắc nét, danh sách món trà sữa dạng lưới (Grid), hiển thị ảnh, tên, giá tiền VNĐ, nút xem chi tiết và nút "Xem giỏ hàng".
   - `Details`: Xem chi tiết từng món trà sữa (ảnh lớn, thành phần, giá, form số lượng, nút "Cho vào giỏ hàng" và "Xem giỏ hàng").

3. **Giỏ hàng (`CartController`)**:
   - Quản lý phiên giỏ hàng qua Session `List<CartItem>`.
   - `AddToCart`: Thêm món hoặc tăng số lượng món trong giỏ.
   - `RemoveFromCart`: Xóa từng món ra khỏi giỏ.
   - `Index`: Bảng danh sách món, đơn giá, số lượng, thành tiền, tổng tiền đơn hàng và nút "Tiến hành thanh toán".
   - `Checkout`: Kiểm tra đăng nhập, lưu dữ liệu vào bảng `Orders` và `OrderDetails`, dọn sạch giỏ hàng trong session và chuyển tiếp sang trang Hóa đơn.

4. **Hóa đơn thanh toán (`OrderController`)**:
   - `Invoice(int id)`: Hiển thị hóa đơn thanh toán gồm Mã đơn hàng, Tên khách hàng, Ngày đặt, bảng danh sách món, đơn giá, tổng tiền và nút "Tiếp tục mua hàng" hoặc "In hóa đơn".

---

## 📂 Cấu Trúc Thư Mục Dự Án

```
WebTraSua/
│
├── Controllers/
│   ├── AccountController.cs       # Đăng nhập, đăng xuất, kiểm tra Session
│   ├── ProductController.cs       # Danh sách sản phẩm, chi tiết món
│   ├── CartController.cs          # Quản lý giỏ hàng Session, thanh toán Checkout
│   └── OrderController.cs         # Xuất hóa đơn thanh toán Invoice
│
├── Models/
│   ├── User.cs                   # Thực thể người dùng
│   ├── Product.cs                # Thực thể sản phẩm trà sữa
│   ├── Order.cs                  # Thực thể đơn hàng
│   ├── OrderDetail.cs            # Thực thể chi tiết đơn hàng
│   ├── CartItem.cs               # Model giỏ hàng lưu Session
│   └── AppDbContext.cs           # DbContext Code First + Quan hệ + Seed Data
│
├── Helpers/
│   └── SessionHelper.cs          # Extension method Serialize/Deserialize JSON vào Session
│
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml        # Header TN Tea, logo, menu, badge giỏ hàng, footer
│   │   └── _ViewImports.cshtml   # Namespace imports và Tag Helpers
│   ├── Account/
│   │   └── Login.cshtml          # Màn hình đăng nhập tài khoản
│   ├── Product/
│   │   ├── Index.cshtml          # Menu trà sữa + video banner + nút xem giỏ hàng
│   │   └── Details.cshtml        # Chi tiết món + form thêm giỏ hàng
│   ├── Cart/
│   │   └── Index.cshtml          # Bảng giỏ hàng, tổng tiền, nút thanh toán
│   └── Order/
│       └── Invoice.cshtml        # Hóa đơn thanh toán hoàn chỉnh
│
├── wwwroot/
│   ├── css/
│   │   └── site.css              # Giao diện Pastel Pink & Plum đặc trưng TN Tea
│   ├── videos/
│   │   └── vdbaner.mp4           # Video banner không âm thanh
│   └── vdbaner.mp4               # Video banner static root
│
├── appsettings.json              # Chuỗi kết nối SQLite (tntea.db)
├── Program.cs                    # Cấu hình Services, Session, DbContext, Routing
└── WebTraSua.csproj              # Project file .NET 8
```

---

## 🚀 Hướng Dẫn Chạy Ứng Dụng

Yêu cầu máy tính đã cài đặt [.NET SDK 8.0+](https://dotnet.microsoft.com/download/dotnet/8.0).

Mở terminal tại thư mục dự án và chạy lệnh:

```bash
dotnet run
```

Sau đó mở trình duyệt và truy cập:
👉 **http://localhost:5200** hoặc URL hiển thị trên terminal.

> **Ghi chú cơ sở dữ liệu:** Hệ thống sử dụng SQLite Code-First (`tntea.db`), khi khởi chạy lần đầu sẽ **tự động tạo toàn bộ bảng và dữ liệu mẫu** (không cần cài SQL Server phức tạp).
