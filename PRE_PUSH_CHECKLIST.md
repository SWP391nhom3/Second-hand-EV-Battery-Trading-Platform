# Pre-Push Checklist - Danh sách kiểm tra trước khi push

## ✅ Bắt buộc phải kiểm tra:

### 1. Chạy Compatibility Checker
```bash
./check-compatibility.sh
```
**Yêu cầu:** Phải không có ERROR (warnings có thể chấp nhận được)

### 2. Đảm bảo không commit sensitive data
- [ ] `appsettings.json` KHÔNG được commit (phải trong .gitignore)
- [ ] `appsettings.Development.json` KHÔNG được commit
- [ ] `launchSettings.json` KHÔNG được commit
- [ ] `.env` files (Frontend) KHÔNG được commit
- [ ] Passwords, API keys, secrets KHÔNG được hardcode

### 3. Build test
```bash
# Backend
cd EVehicleManagementAPI/EVehicleManagementAPI
dotnet build

# Frontend
cd Second-hand-EV-Battery-Trading-Platform-FE
npm install
npm run build
```

### 4. Kiểm tra .gitignore
- [ ] `node_modules/` được ignore
- [ ] `bin/`, `obj/` được ignore
- [ ] Config files local được ignore
- [ ] `.env` files được ignore

### 5. Documentation
- [ ] Có `appsettings.example.json` cho backend
- [ ] Có `.env.example` cho frontend
- [ ] README.md được cập nhật (nếu có thay đổi setup)

## ⚠️ Khuyến nghị:

### 1. Test trên máy local
- [ ] Backend chạy được
- [ ] Frontend chạy được
- [ ] Kết nối database OK
- [ ] API endpoints hoạt động
- [ ] CORS hoạt động đúng

### 2. Code quality
- [ ] Không có build errors
- [ ] Không có warnings nghiêm trọng
- [ ] Code format đúng chuẩn

### 3. Migration (nếu có)
- [ ] Migration an toàn (không drop data)
- [ ] Có `MIGRATION_GUIDE.md` nếu thêm migration mới
- [ ] Đã test migration trên local

## 🚫 Không được push nếu:

- ❌ Có hardcoded passwords/keys trong code được commit
- ❌ `node_modules` được track trong git
- ❌ Config files local được commit
- ❌ Build failed
- ❌ Compatibility checker có ERROR

## 📝 Notes

- Warnings trong compatibility checker thường OK, nhưng nên fix khi có thể
- Luôn test trên máy mình trước khi push
- Nếu không chắc, hỏi team lead trước

