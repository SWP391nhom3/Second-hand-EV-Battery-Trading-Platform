# Team Test Guide - Hướng dẫn test code trên máy của bạn

## 🎯 Mục đích

Document này giúp bạn test xem code có chạy được trên máy của bạn không, dù bạn có config riêng.

## ✅ Checklist Test Cơ Bản

### Bước 1: Clone và Setup (Simulate máy mới)

```bash
# 1. Clone repository
git clone https://github.com/SWP391nhom3/Second-hand-EV-Battery-Trading-Platform.git
cd Second-hand-EV-Battery-Trading-Platform

# 2. Checkout branch
git checkout feature/working-branch-20251102

# 3. Setup Backend
cd EVehicleManagementAPI/EVehicleManagementAPI
cp appsettings.example.json appsettings.json

# 4. Cập nhật connection string CỦA BẠN trong appsettings.json
# Thay: YOUR_SERVER, YOUR_USER, YOUR_PASSWORD
```

### Bước 2: Test Backend

```bash
# 1. Restore packages
dotnet restore

# 2. Build project
dotnet build

# 3. Nếu build thành công → ✅ Backend OK
# Nếu build lỗi → ❌ Có vấn đề cần báo
```

### Bước 3: Test Database Connection

```bash
# 1. Kiểm tra database có chạy không
# (Tùy vào SQL Server của bạn)

# 2. Chạy migration
dotnet ef database update

# 3. Nếu migration thành công → ✅ Database OK
# Nếu lỗi → ❌ Connection string sai hoặc database chưa tạo
```

### Bước 4: Test Frontend

```bash
# 1. Vào thư mục frontend
cd ../../Second-hand-EV-Battery-Trading-Platform-FE

# 2. Copy .env.example
cp .env.example .env

# 3. Cập nhật API URL trong .env
# VITE_API_BASE_URL=http://localhost:5000 (hoặc port của bạn)

# 4. Install dependencies
npm install

# 5. Build frontend
npm run build

# 6. Nếu build thành công → ✅ Frontend OK
```

### Bước 5: Test Kết Nối FE - BE

```bash
# Terminal 1: Chạy Backend
cd EVehicleManagementAPI/EVehicleManagementAPI
dotnet run

# Terminal 2: Chạy Frontend
cd Second-hand-EV-Battery-Trading-Platform-FE
npm run dev

# 3. Mở browser: http://localhost:5173
# 4. Kiểm tra console browser - không có CORS error → ✅ Kết nối OK
```

## 🔍 Test Scripts Tự Động

### Chạy Compatibility Test

```bash
# Test tự động
./test-compatibility.sh
```

Script này sẽ:
- ✅ Kiểm tra config files có template không
- ✅ Kiểm tra có thể build được không
- ✅ Kiểm tra CORS configuration
- ✅ Kiểm tra migrations an toàn không

### Chạy Compatibility Checker

```bash
# Check code quality
./check-compatibility.sh
```

## 📋 Report Kết Quả

Sau khi test, điền vào form này và báo team:

```
✅ HOẶC ❌ - Backend build thành công
✅ HOẶC ❌ - Database connection OK
✅ HOẶC ❌ - Frontend build thành công
✅ HOẶC ❌ - FE-BE kết nối được (không CORS error)
✅ HOẶC ❌ - API endpoints hoạt động

Lỗi gặp phải (nếu có):
- ...
- ...

Môi trường:
- OS: [Windows/Mac/Linux]
- .NET Version: [x.x.x]
- Node Version: [x.x.x]
- SQL Server: [Version/Type]
```

## ⚠️ Nếu Gặp Lỗi

### Lỗi Build Backend
- Kiểm tra .NET SDK version (cần 8.0+)
- Kiểm tra packages: `dotnet restore`
- Xem error message chi tiết

### Lỗi Database
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string đúng format
- Kiểm tra user có quyền tạo database không

### Lỗi CORS
- Kiểm tra CORS config trong Program.cs
- Kiểm tra frontend URL trong CORS:AllowedOrigins
- Kiểm tra frontend đang chạy đúng port

### Lỗi Frontend Build
- Kiểm tra Node version (18+)
- Xóa node_modules và reinstall: `rm -rf node_modules && npm install`
- Kiểm tra .env file có đúng format không

## 📞 Báo Lỗi

Nếu code không chạy được trên máy bạn:

1. **Chạy test script và lấy output:**
   ```bash
   ./test-compatibility.sh > test-result.txt 2>&1
   ```

2. **Tạo issue trên GitHub** với:
   - Test result output
   - Error messages chi tiết
   - OS và versions (theo form trên)
   - Steps bạn đã làm

3. **Hoặc báo team lead** với thông tin trên

## 💡 Tips

- Luôn chạy `./check-compatibility.sh` trước khi push
- Nếu có thay đổi config, test lại từ đầu
- Giữ `.env` và `appsettings.json` local, không commit
- Nếu pull code mới, chạy test lại để đảm bảo

