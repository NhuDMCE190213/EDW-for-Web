# EDW - Electronic Device E-Commerce App
**Môn học:** PRN232
**Nhóm:** PRM393-G2  
**Học kỳ:** Fall 2026  

---

## 📝 Giới thiệu Dự án
Dự án **EDW (Electronic Device Website)** là một hệ thống thương mại điện tử ASP.NET Core MVC chuyên kinh doanh các sản phẩm công nghệ (Điện thoại, Laptop, Phụ kiện...). Frontend gọi Backend Web API để đọc catalog, quản lý giỏ hàng và checkout.

### Hệ thống được chia làm 2 thành phần chính (Client - Server):
1. **Backend (ASP.NET Core Web API):** Hệ thống API cung cấp dữ liệu, xử lý nghiệp vụ, xác thực người dùng (JWT) và tích hợp cổng thanh toán VNPAY. Dành cho việc quản lý danh mục, sản phẩm, đơn hàng và cung cấp endpoint cho Frontend.
2. **Frontend (ASP.NET Core MVC):** Giao diện web cho Customer, Staff và Admin, giao tiếp với Backend bằng REST API.

---

## 🏗️ Kiến trúc Hệ thống
Dự án áp dụng mô hình kiến trúc **Client - Server** kết hợp mô hình phân lớp chuẩn chỉ ở Backend để tách biệt mối quan tâm và tuân thủ các nguyên tắc SOLID:

* **Frontend (ASP.NET Core MVC):** Controllers, Razor Views và typed API clients.
* **Backend (ASP.NET Core API):**
  * **API Layer (Controllers):** Tiếp nhận HTTP Request từ ứng dụng Flutter và trả về dữ liệu định dạng JSON.
  * **BLL (Business Logic Layer):** Lớp xử lý nghiệp vụ, tính toán logic hệ thống. Tầng này độc lập hoàn toàn với API và CSDL.
  * **DAL (Data Access Layer):** Lớp kết nối Cơ sở dữ liệu. Sử dụng Entity Framework Core với Fluent API tách file cấu hình riêng (`IEntityTypeConfiguration`) và tự động chuẩn hóa tên bảng theo định dạng `snake_case`.

---

## 🛠️ Công nghệ Sử dụng (Tech Stack)
* **Frontend Web:** ASP.NET Core MVC (.NET 8), Razor, C#
* **Backend Server:** ASP.NET Core Web API (.NET 8), C#
* **ORM:** Entity Framework Core
* **Database:** SQL Server
* **Naming Convention:** `EFCore.NamingConventions` (snake_case)

---

## 🚀 Hướng dẫn Cài đặt & Chạy Dự án (Dành cho thành viên nhóm)

### 1. Chuẩn bị môi trường
* Đảm bảo máy đã cài **.NET SDK (v8.0)** và **SQL Server**.
* Clone dự án về máy: `git clone <url-repository>`

### 2. Cấu hình Backend API (Web API)
Backend API phải chạy với JWT và database được cấu hình đầy đủ. Khi chạy bằng HTTPS, FE mặc định gọi `https://localhost:7278/`; nếu dùng cổng khác, sửa `ApiSettings:BaseUrl` trong `PresentationMVC/appsettings.json`.

```json
{
  "Jwt": {
    "Key": "a-development-key-with-at-least-32-characters",
    "Issuer": "EDW.Api",
    "Audience": "EDW.Client"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=EDW_ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Chạy Frontend

```bash
dotnet run --project PresentationMVC/PresentationMVC.csproj --launch-profile https
```

Mẫu cấu hình FE nằm ở `PresentationMVC/appsettings.example.json`.

---

## 👥 Thành viên Thực hiện (Project Team)
- Đái Minh Như (Leader) - Email: nhudm.ce190213@gmail.com
- [Thêm tên các thành viên khác vào đây...]

> **Lưu ý:** Khi bạn thêm file cấu hình chứa secret (API keys, connection strings) vào repository cục bộ, tuyệt đối **không** commit các file này lên GitHub — hãy kiểm tra kỹ `.gitignore` trước khi push code.