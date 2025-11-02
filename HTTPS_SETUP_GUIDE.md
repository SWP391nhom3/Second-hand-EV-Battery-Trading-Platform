# HTTPS Setup Guide - Hướng dẫn cấu hình HTTPS

## 📋 Tình trạng hiện tại

### Backend (API)
- ✅ **Hỗ trợ cả HTTP và HTTPS** trong development
- ✅ Profile "https": `https://localhost:5001;http://localhost:5000`
- ✅ Profile "http": `http://localhost:5000`
- ⚠️ HTTPS redirection chỉ bật trong **Production**

### Frontend
- ✅ **Mặc định dùng HTTPS**: `https://localhost:5001`
- ✅ Có thể config qua environment variable: `VITE_API_BASE_URL`

## 🔧 Cấu hình để Frontend chạy với HTTPS

### Option 1: Backend chạy HTTPS (Khuyến nghị)

#### Bước 1: Chạy backend với profile HTTPS
```bash
cd EVehicleManagementAPI/EVehicleManagementAPI
dotnet run --launch-profile https
```

Backend sẽ chạy tại:
- HTTPS: `https://localhost:5001` ✅
- HTTP: `http://localhost:5000` (fallback)

#### Bước 2: Frontend config
Tạo file `.env` trong `Second-hand-EV-Battery-Trading-Platform-FE/`:
```env
VITE_API_BASE_URL=https://localhost:5001
```

#### Bước 3: Chạy frontend
```bash
cd Second-hand-EV-Battery-Trading-Platform-FE
npm run dev
```

Frontend sẽ gọi API qua HTTPS ✅

### Option 2: Cả hai đều HTTP (Development)

#### Bước 1: Backend chạy HTTP
```bash
cd EVehicleManagementAPI/EVehicleManagementAPI
dotnet run --launch-profile http
# hoặc
dotnet run
```

#### Bước 2: Frontend config HTTP
Tạo file `.env`:
```env
VITE_API_BASE_URL=http://localhost:5000
```

#### Bước 3: Cập nhật CORS (nếu cần)
Đảm bảo `appsettings.json` có:
```json
{
  "CORS": {
    "AllowedOrigins": "http://localhost:5173;https://localhost:5173"
  }
}
```

## ⚠️ Vấn đề thường gặp

### 1. Mixed Content Error
**Lỗi:** Frontend HTTPS gọi API HTTP → Browser chặn

**Giải pháp:**
- Cả hai đều dùng HTTPS, hoặc
- Cả hai đều dùng HTTP trong development

### 2. CORS Error
**Lỗi:** "Access-Control-Allow-Origin" error

**Giải pháp:**
- Kiểm tra `CORS:AllowedOrigins` trong `appsettings.json`
- Đảm bảo frontend URL đúng trong config
- Ví dụ: Nếu frontend chạy `http://localhost:5173`, config phải có URL đó

### 3. SSL Certificate Error
**Lỗi:** "certificate is not trusted" khi dùng HTTPS

**Giải pháp:**
- Development: Chấp nhận certificate không tin cậy (browser sẽ hỏi)
- Production: Dùng certificate hợp lệ

## 🎯 Best Practices

### Development
```bash
# Backend: HTTPS profile
dotnet run --launch-profile https

# Frontend: .env
VITE_API_BASE_URL=https://localhost:5001
```

### Production
- Backend: Luôn dùng HTTPS (có certificate hợp lệ)
- Frontend: Config API URL qua environment variable
- CORS: Chỉ cho phép production domain

## 📝 Quick Reference

### Backend URLs
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger` (khi chạy HTTPS)

### Frontend URLs
- Dev server: `http://localhost:5173` hoặc `https://localhost:5173` (nếu config)

### Config Files
- Backend: `appsettings.json` → `CORS:AllowedOrigins`
- Frontend: `.env` → `VITE_API_BASE_URL`

## ✅ Checklist

- [ ] Backend đang chạy (HTTPS hoặc HTTP)
- [ ] Frontend `.env` file có `VITE_API_BASE_URL` đúng
- [ ] CORS config có frontend URL
- [ ] Không có mixed content (HTTPS/HTTP mismatch)
- [ ] Test API call từ browser console

## 🔍 Test Connection

### Test từ Browser Console
```javascript
// Test API connection
fetch('https://localhost:5001/api/health')
  .then(r => r.json())
  .then(console.log)
  .catch(console.error);
```

### Test từ Terminal
```bash
# Test HTTPS endpoint
curl -k https://localhost:5001/swagger

# Test HTTP endpoint
curl http://localhost:5000/swagger
```

## 📞 Troubleshooting

1. **Backend không start được HTTPS:**
   - Kiểm tra port 5001 có bị chiếm không
   - Kiểm tra `launchSettings.json` có profile "https"

2. **Frontend không kết nối được:**
   - Kiểm tra `.env` file có đúng không
   - Kiểm tra CORS config
   - Kiểm tra browser console để xem error

3. **Mixed content error:**
   - Đảm bảo cả FE và BE cùng protocol (HTTP hoặc HTTPS)

