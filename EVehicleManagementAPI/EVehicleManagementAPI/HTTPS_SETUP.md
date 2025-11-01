# 🔒 Cấu hình HTTPS cho Backend

## ✅ Đã thực hiện

### 1. Thêm HTTPS Profile trong `launchSettings.json`

**Profile mới:**
- **HTTPS:** `https://localhost:5001`
- **HTTP (fallback):** `http://localhost:5000`

```json
"https": {
  "commandName": "Project",
  "dotnetRunMessages": true,
  "launchBrowser": true,
  "launchUrl": "swagger",
  "applicationUrl": "https://localhost:5001;http://localhost:5000",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

### 2. Cập nhật BaseUrl trong `appsettings.json`

```json
"AppSettings": {
  "BaseUrl": "https://localhost:5001"
}
```

### 3. Cập nhật CORS để hỗ trợ cả HTTP và HTTPS frontend

```csharp
policy.WithOrigins(
    "http://localhost:5173",   // HTTP frontend
    "https://localhost:5173"   // HTTPS frontend (nếu có)
)
```

## 🚀 Cách sử dụng

### Chạy với HTTPS:

1. **Trong Visual Studio/Rider:**
   - Chọn profile **"https"** thay vì "http" khi chạy

2. **Hoặc command line:**
   ```bash
   cd EVehicleManagementAPI/EVehicleManagementAPI
   dotnet run --launch-profile https
   ```

3. **Backend sẽ chạy trên:**
   - **HTTPS:** `https://localhost:5001` (chính)
   - **HTTP:** `http://localhost:5000` (fallback)

### Chứng chỉ SSL:

- ASP.NET Core tự động tạo development certificate cho HTTPS
- Lần đầu chạy có thể hỏi trust certificate, chọn **"Yes"**

### Nếu gặp lỗi certificate:

**Windows/Mac:**
```bash
dotnet dev-certs https --trust
```

**Linux:**
- Cần cấu hình certificate thủ công hoặc dùng reverse proxy

## 📝 Lưu ý

1. **Frontend cần cập nhật API URL:**
   - Từ: `http://localhost:5000`
   - Thành: `https://localhost:5001`

2. **Google OAuth Redirect URI:**
   - Nếu frontend chạy HTTPS, cần update redirect URI trong Google Console
   - Hiện tại: `http://localhost:5173/google-callback`
   - Nếu frontend HTTPS: `https://localhost:5173/google-callback`

3. **Environment Variables:**
   - Có thể set `ASPNETCORE_URLS=https://localhost:5001` để force HTTPS

## 🔧 Troubleshooting

### Lỗi "The certificate is invalid"

```bash
# Reset và trust certificate
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Browser warning về certificate

- Development certificate là self-signed
- Chấp nhận warning hoặc trust certificate trong browser

### Port đã được sử dụng

- Kiểm tra process đang dùng port 5001
- Hoặc đổi port trong `launchSettings.json`

## ✅ Checklist

- [x] Thêm HTTPS profile
- [x] Cập nhật BaseUrl
- [x] Cập nhật CORS để hỗ trợ HTTPS frontend
- [ ] Update frontend API URL to `https://localhost:5001`
- [ ] Test Google OAuth với HTTPS (nếu frontend HTTPS)
- [ ] Verify CORS hoạt động với HTTPS

