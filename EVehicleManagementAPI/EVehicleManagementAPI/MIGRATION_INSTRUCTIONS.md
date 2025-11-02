# Hướng Dẫn Migration - Cập Nhật Bảng Có Sẵn

## ⚠️ VẤN ĐỀ VÀ GIẢI PHÁP

**Vấn đề:** Khi bạn thay đổi Model (thêm/bớt/sửa cột), các thay đổi không tự động cập nhật vào database.

**Giải pháp:** Bạn cần tạo migration mới mỗi khi thay đổi Model.

## ✅ Quy Trình Khi Thay Đổi Model

### Bước 1: Thay đổi Model trong Code

Ví dụ: Thêm cột `Status` vào bảng `Account`

```csharp
// Models/Account.cs
public class Account
{
    public int AccountId { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Status { get; set; } // ✅ Thêm cột mới
}
```

### Bước 2: Tạo Migration Mới

```bash
cd EVehicleManagementAPI/EVehicleManagementAPI

# Tạo migration mới (EF sẽ tự động detect thay đổi)
dotnet ef migrations add AddStatusToAccount

# Output sẽ hiển thị:
# Done. To undo this action, use 'ef migrations remove'
```

### Bước 3: Kiểm Tra Migration File

File migration sẽ được tạo tại:
```
Migrations/YYYYMMDDHHMMSS_AddStatusToAccount.cs
```

**Mở file và kiểm tra xem có an toàn không:**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ✅ AN TOÀN: Thêm cột nullable
    migrationBuilder.AddColumn<string>(
        name: "Status",
        table: "Accounts",
        nullable: true); // nullable = true → không ảnh hưởng dữ liệu cũ
}
```

**⚠️ CẢNH BÁO: Nếu thấy những dòng này → NGUY HIỂM:**
```csharp
// ❌ NGUY HIỂM: Sẽ mất dữ liệu!
migrationBuilder.DropColumn(...);
migrationBuilder.DropTable(...);
migrationBuilder.DeleteData(...);
```

### Bước 4: Apply Migration (Test trên Local)

```bash
# Apply migration vào database local
dotnet ef database update

# Hoặc chạy app (migration sẽ tự động apply)
dotnet run

# Console sẽ hiển thị:
# ✅ Database migrations applied successfully
```

### Bước 5: Commit và Push Migration

```bash
git add Migrations/
git commit -m "feat: Add Status column to Account table"
git push origin feature/working-branch-20251102
```

## 🎯 Khi Team Members Pull Code Về

Migration sẽ **TỰ ĐỘNG** apply khi họ chạy app:

```bash
# 1. Pull code mới
git pull origin feature/working-branch-20251102

# 2. Chạy app
cd EVehicleManagementAPI/EVehicleManagementAPI
dotnet run

# 3. Migration tự động apply → Bảng được cập nhật!
# Console sẽ hiển thị:
# ✅ Database migrations applied successfully
```

**⚠️ Lưu ý:** Migration chỉ apply nếu:
- File migration đã được commit và push
- Migration chưa được apply trên database của họ
- Connection string đúng

## 📋 Checklist Khi Thay Đổi Model

- [ ] Thay đổi Model trong code (thêm/bớt/sửa property)
- [ ] Tạo migration: `dotnet ef migrations add MigrationName`
- [ ] Review migration file (kiểm tra không có DropTable/DropColumn trên bảng có dữ liệu)
- [ ] Test migration trên local: `dotnet ef database update`
- [ ] Test app chạy OK
- [ ] Commit migration file: `git add Migrations/ && git commit`
- [ ] Push lên GitHub: `git push origin feature/working-branch-20251102`
- [ ] Thông báo team về migration mới

## 🔧 Các Loại Thay Đổi

### 1. Thêm Cột Mới (AN TOÀN)

```csharp
// Model
public string NewColumn { get; set; }

// Migration (tự động) - ✅ AN TOÀN
migrationBuilder.AddColumn<string>(
    name: "NewColumn",
    table: "Accounts",
    nullable: true); // nullable = true → không ảnh hưởng dữ liệu cũ
```

### 2. Xóa Cột (NGUY HIỂM)

```csharp
// Model
// public string OldColumn { get; set; } // ❌ Xóa

// Migration (tự động) - ⚠️ NGUY HIỂM
migrationBuilder.DropColumn(
    name: "OldColumn",
    table: "Accounts"); // → Sẽ mất dữ liệu!
```

**Giải pháp:** Chỉ xóa khi chắc chắn không cần dữ liệu cũ, hoặc migrate dữ liệu trước khi xóa.

### 3. Đổi Kiểu Dữ Liệu (CẨN THẬN)

```csharp
// Model
public int Status { get; set; } // Thay string

// Migration - ⚠️ Cần đảm bảo dữ liệu cũ có thể convert
migrationBuilder.AlterColumn<int>(
    name: "Status",
    table: "Accounts",
    nullable: false);
```

### 4. Thêm NOT NULL Column (CẦN DEFAULT VALUE)

```csharp
// Model
public string Status { get; set; } // NOT NULL

// Migration - ⚠️ Cần có default value
migrationBuilder.AddColumn<string>(
    name: "Status",
    table: "Accounts",
    nullable: false,
    defaultValue: "ACTIVE"); // ✅ Default value → không ảnh hưởng dữ liệu cũ
```

## ⚠️ Lưu Ý Quan Trọng

### Migration Tự Động Apply

Trong `Program.cs` đã có:
```csharp
db.Database.Migrate(); // ✅ Tự động apply pending migrations khi start app
```

### Migration An Toàn

- ✅ Chỉ thêm cột/cột nullable → An toàn
- ✅ Thêm cột có default value → An toàn
- ⚠️ Drop column → Mất dữ liệu (chỉ làm khi chắc chắn)
- ⚠️ Drop table → Mất toàn bộ dữ liệu (cực kỳ nguy hiểm)

### Backup Trước Khi Apply Migration Lớn

```bash
# Backup database (SQL Server)
sqlcmd -S localhost -d EVehicleDB -Q "BACKUP DATABASE EVehicleDB TO DISK = 'backup.bak'"
```

## 🔍 Troubleshooting

### Vấn đề: Migration không được tạo

**Nguyên nhân:**
- Chưa cài EF Core Tools
- Chưa thay đổi Model

**Giải pháp:**
```bash
# Install EF Core Tools
dotnet tool install --global dotnet-ef

# Check xem Model đã thay đổi chưa
```

### Vấn đề: Migration không apply

**Nguyên nhân:**
- Connection string sai
- Migration có lỗi
- Database không tồn tại

**Giải pháp:**
```bash
# 1. Check connection string trong appsettings.json
# 2. Apply migration thủ công
dotnet ef database update --verbose
```

### Vấn đề: Cột không được thêm vào database

**Nguyên nhân:**
- Migration chưa được apply
- Migration file chưa được commit

**Giải pháp:**
```bash
# 1. Check migration đã apply chưa
dotnet ef migrations list

# 2. Apply migration
dotnet ef database update

# 3. Restart app
dotnet run
```

## 📞 Hỗ Trợ

Nếu gặp vấn đề:
1. Check console output khi start app
2. Check file migration có đúng không
3. Liên hệ team lead hoặc tạo issue trên GitHub

