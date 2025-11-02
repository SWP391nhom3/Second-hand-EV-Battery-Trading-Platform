# Setup Guide - Hướng dẫn cài đặt cho Team Members

## 📋 Yêu cầu hệ thống

- **.NET SDK:** 8.0 trở lên
- **SQL Server:** 2019 trở lên (hoặc SQL Server Express)
- **Node.js:** 18.x trở lên (cho Frontend)
- **Git:** Bất kỳ phiên bản nào

## 🔧 Setup Backend (API)

### 1. Clone repository
```bash
git clone https://github.com/SWP391nhom3/Second-hand-EV-Battery-Trading-Platform.git
cd Second-hand-EV-Battery-Trading-Platform
```

### 2. Checkout branch làm việc
```bash
git checkout feature/working-branch-20251102
```

### 3. Tạo file cấu hình local
```bash
cd EVehicleManagementAPI/EVehicleManagementAPI
cp appsettings.example.json appsettings.json
cp appsettings.example.json appsettings.Development.json
```

### 4. Cập nhật connection string
Mở `appsettings.json` và `appsettings.Development.json`, thay đổi:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=EVehicleDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

**Lưu ý:**
- `YOUR_SERVER`: Địa chỉ SQL Server của bạn (ví dụ: `localhost,1433` hoặc `.\SQLEXPRESS`)
- `YOUR_USER`: Tên user SQL Server (ví dụ: `sa`)
- `YOUR_PASSWORD`: Password của bạn
- `EVehicleDB`: Tên database (có thể giữ nguyên hoặc đổi tên)

### 5. Restore packages và chạy migration
```bash
dotnet restore
dotnet ef database update
```

Nếu chưa có EF Core tools:
```bash
dotnet tool install --global dotnet-ef
```

### 6. Chạy ứng dụng
```bash
dotnet run
```

API sẽ chạy tại:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

## 🎨 Setup Frontend

### 1. Vào thư mục frontend
```bash
cd Second-hand-EV-Battery-Trading-Platform-FE
```

### 2. Cài đặt dependencies
```bash
npm install
```

### 3. Tạo file .env (nếu chưa có .env.example, tạo file này)
```bash
# Tạo file .env từ .env.example (nếu có)
cp .env.example .env
```

Cập nhật `.env`:
```env
VITE_API_BASE_URL=http://localhost:5000
# hoặc
VITE_API_BASE_URL=https://localhost:5001
```

### 4. Chạy frontend
```bash
npm run dev
```

Frontend sẽ chạy tại: `http://localhost:5173`

## ⚠️ Troubleshooting

### Backend không kết nối được database
1. Kiểm tra SQL Server đang chạy:
   ```bash
   # Windows
   services.msc -> SQL Server
   
   # Mac/Linux
   # Kiểm tra Docker container nếu dùng Docker
   ```

2. Kiểm tra connection string trong `appsettings.json`

3. Thử kết nối bằng SQL Server Management Studio

### CORS errors (Frontend không gọi được API)
1. Kiểm tra CORS config trong `Program.cs`
2. Đảm bảo frontend URL đúng với CORS config
3. Kiểm tra API đang chạy đúng port

### Port đã được sử dụng
1. Đổi port trong `launchSettings.json` (file này không được commit)
2. Hoặc kill process đang dùng port:
   ```bash
   # Windows
   netstat -ano | findstr :5000
   taskkill /PID <PID> /F
   
   # Mac/Linux
   lsof -ti:5000 | xargs kill
   ```

### Migration errors
Xem `MIGRATION_GUIDE.md` để biết chi tiết về migration.

## 🔐 Environment Variables (Khuyến nghị)

Thay vì hardcode trong `appsettings.json`, nên dùng environment variables:

```bash
# Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=EVehicleDB;..."

# Mac/Linux
export ConnectionStrings__DefaultConnection="Server=localhost;Database=EVehicleDB;..."
```

Hoặc tạo file `appsettings.Development.json` (đã được ignore trong git)

## 📝 Checklist trước khi push code

- [ ] Đã chạy `check-compatibility.sh` và không có lỗi
- [ ] Không commit `appsettings.json` hoặc `appsettings.Development.json`
- [ ] Không commit `launchSettings.json`
- [ ] Đã test trên máy của mình
- [ ] Đã test kết nối với database
- [ ] Đã test CORS hoạt động đúng

## 🆘 Liên hệ

Nếu gặp vấn đề, tạo issue trên GitHub hoặc liên hệ team lead.

