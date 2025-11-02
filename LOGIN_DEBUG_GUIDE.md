# Login Debug Guide - Hướng dẫn debug lỗi đăng nhập

## 🔍 Các vấn đề thường gặp

### 1. CORS Error
**Triệu chứng:**
```
Access to XMLHttpRequest at 'https://localhost:5001/api/Auth/login' from origin 'http://localhost:5173' has been blocked by CORS policy
```

**Giải pháp:**
- Kiểm tra backend đang chạy đúng port (5000 hoặc 5001)
- Kiểm tra CORS config trong `appsettings.json`:
  ```json
  {
    "CORS": {
      "AllowedOrigins": "http://localhost:5173;https://localhost:5173"
    }
  }
  ```
- Đảm bảo frontend URL đúng trong CORS config

### 2. Network Error / Connection Refused
**Triệu chứng:**
```
Network Error
ERR_CONNECTION_REFUSED
```

**Giải pháp:**
- Backend chưa chạy → Chạy `dotnet run`
- URL sai trong frontend → Kiểm tra `.env` file
- Port bị chiếm → Đổi port hoặc kill process

### 3. 401 Unauthorized - "Invalid email or password"
**Triệu chứng:**
```
Status: 401
Message: "Invalid email or password"
```

**Kiểm tra:**
- Email có đúng không
- Password có đúng không
- Account có tồn tại trong database không
- Password hash có đúng format không

### 4. 401 Unauthorized - "Account is not active"
**Triệu chứng:**
```
Status: 401
Message: "Account is not active"
```

**Giải pháp:**
- Kiểm tra `Member.Status = "ACTIVE"` trong database
- Admin/Staff không cần Member record (đã fix)

### 5. 404 Not Found
**Triệu chứng:**
```
Status: 404
```

**Kiểm tra:**
- URL endpoint: `api/Auth/login` (chữ A viết hoa)
- Backend đang chạy đúng không
- Route có đúng không

### 6. Mixed Content Error
**Triệu chứng:**
```
Mixed Content: The page was loaded over HTTPS, but requested an insecure resource
```

**Giải pháp:**
- Cả FE và BE cùng protocol (HTTPS hoặc HTTP)
- Hoặc config FE dùng HTTPS khi BE chạy HTTPS

## 🛠️ Debug Steps

### Bước 1: Kiểm tra Backend
```bash
cd EVehicleManagementAPI/EVehicleManagementAPI
dotnet run --launch-profile https
```

Kiểm tra:
- Backend start thành công
- Swagger: https://localhost:5001/swagger
- Endpoint `/api/Auth/login` có trong Swagger

### Bước 2: Test API trực tiếp
```bash
# Test bằng curl
curl -X POST https://localhost:5001/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.com","password":"Admin@123"}' \
  -k
```

Nếu thành công → API OK, vấn đề ở frontend
Nếu lỗi → Vấn đề ở backend

### Bước 3: Kiểm tra Frontend
Mở browser console (F12) và check:
1. Network tab → Xem request có được gửi không
2. Console tab → Xem có error không
3. Request URL có đúng không

### Bước 4: Kiểm tra Config
**Frontend `.env`:**
```env
VITE_API_BASE_URL=https://localhost:5001
```

**Backend `appsettings.json`:**
```json
{
  "CORS": {
    "AllowedOrigins": "http://localhost:5173;https://localhost:5173"
  },
  "Jwt": {
    "Key": "your-secret-key-here"
  }
}
```

## 📋 Checklist Debug

- [ ] Backend đang chạy
- [ ] Frontend đang chạy
- [ ] API URL đúng trong `.env`
- [ ] CORS config có frontend URL
- [ ] JWT Key có trong `appsettings.json`
- [ ] Account tồn tại trong database
- [ ] Password đúng
- [ ] Browser console không có error
- [ ] Network tab thấy request được gửi

## 🔧 Quick Fixes

### Reset và test lại
```bash
# Backend
cd EVehicleManagementAPI/EVehicleManagementAPI
dotnet run --launch-profile https

# Frontend (terminal mới)
cd Second-hand-EV-Battery-Trading-Platform-FE
rm -rf node_modules .vite
npm install
npm run dev
```

### Test với account mẫu
Nếu có API create-admin:
```bash
curl -X POST https://localhost:5001/api/auth/create-admin \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@test.com","password":"Admin@123"}' \
  -k
```

Sau đó thử login với account này.

