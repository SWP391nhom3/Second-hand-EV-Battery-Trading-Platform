# Cách Cập Nhật Bảng Có Sẵn - How to Update Existing Tables

## 🔍 Vấn đề

Khi bạn thay đổi Model (thêm/bớt/sửa cột), các thay đổi **KHÔNG TỰ ĐỘNG** cập nhật vào database. Bạn cần tạo migration để cập nhật.

## ✅ Giải pháp: Tạo Migration Mới

### Bước 1: Thay đổi Model

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

### Bước 2: Tạo Migration

```bash
cd EVehicleManagementAPI/EVehicleManagementAPI

# Tạo migration mới (EF sẽ tự detect thay đổi)
dotnet ef migrations add AddStatusToAccount

# Output:
# Build started...
# Build succeeded.
# Done. To undo this action, use 'ef migrations remove'
```

### Bước 3: Kiểm tra Migration File

File migration sẽ được tạo tại:
```
Migrations/YYYYMMDDHHMMSS_AddStatusToAccount.cs
```

**Mở file và kiểm tra:**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ✅ Đây là migration an toàn
    migrationBuilder.AddColumn<string>(
        name: "Status",
        table: "Accounts",
        nullable: true); // nullable = true → không ảnh hưởng dữ liệu cũ
}
```

**⚠️ Lưu ý:**
- Nếu cột là **NOT NULL**, cần có default value hoặc migrate dữ liệu cũ
- Không được có `DropColumn` hoặc `DropTable` trên bảng có dữ liệu

### Bước 4: Apply Migration

```bash
# Cách 1: Apply thủ công
dotnet ef database update

# Cách 2: Chạy app (tự động apply)
dotnet run
```

Khi chạy app, console sẽ hiển thị:
```
✅ Database migrations applied successfully
```

### Bước 5: Commit và Push

```bash
git add Migrations/
git commit -m "feat: Add Status column to Account table"
git push origin feature/working-branch-20251102
```

## 🎯 Khi Team Members Pull Code Về

Migration sẽ **TỰ ĐỘNG** apply khi họ chạy app:

```bash
# 1. Pull code
git pull origin feature/working-branch-20251102

# 2. Chạy app
dotnet run

# 3. Migration tự động apply → Bảng được cập nhật!
```

## 📋 Các Loại Thay Đổi

### 1. Thêm Cột Mới

```csharp
// Model
public string NewColumn { get; set; }

// Migration (tự động)
migrationBuilder.AddColumn<string>(
    name: "NewColumn",
    table: "Accounts",
    nullable: true);
```

**✅ An toàn:** Cột nullable → không ảnh hưởng dữ liệu cũ

### 2. Xóa Cột

```csharp
// Model
// public string OldColumn { get; set; } // ❌ Xóa

// Migration (tự động)
migrationBuilder.DropColumn(
    name: "OldColumn",
    table: "Accounts");
```

**⚠️ Nguy hiểm:** Sẽ mất dữ liệu! Chỉ xóa khi chắc chắn không cần dữ liệu cũ.

### 3. Đổi Tên Cột

```csharp
// Model
public string NewName { get; set; } // Thay OldName

// Migration (tự động tạo Drop + Add - NGUY HIỂM!)
// → Cần sửa migration để dùng RenameColumn
```

**⚠️ Nguy hiểm:** EF có thể tạo Drop + Add → mất dữ liệu!

**Giải pháp:** Sửa migration để dùng `RenameColumn`:
```csharp
migrationBuilder.RenameColumn(
    name: "OldName",
    table: "Accounts",
    newName: "NewName");
```

### 4. Thay Đổi Kiểu Dữ Liệu

```csharp
// Model
public int Status { get; set; } // Thay string

// Migration
migrationBuilder.AlterColumn<int>(
    name: "Status",
    table: "Accounts",
    nullable: false);
```

**⚠️ Cẩn thận:** Cần đảm bảo dữ liệu cũ có thể convert sang kiểu mới.

## 🔧 Troubleshooting

### Vấn đề: Migration không được tạo

**Nguyên nhân:**
- Chưa cài đặt EF Core Tools
- Database connection fail

**Giải pháp:**
```bash
# Install EF Core Tools
dotnet tool install --global dotnet-ef

# Check connection string trong appsettings.json
```

### Vấn đề: Migration không apply

**Nguyên nhân:**
- Migration có lỗi
- Database connection fail
- Conflict với database hiện tại

**Giải pháp:**
```bash
# 1. Check lỗi
dotnet ef database update --verbose

# 2. Check migration list
dotnet ef migrations list

# 3. Nếu cần, rollback
dotnet ef database update PreviousMigrationName
```

### Vấn đề: Cột không được thêm vào database

**Nguyên nhân:**
- Migration chưa được apply
- Migration có lỗi và bị skip

**Giải pháp:**
```bash
# 1. Check migration đã apply chưa
dotnet ef migrations list

# 2. Apply migration thủ công
dotnet ef database update

# 3. Restart app và check console
```

## ✅ Checklist

Khi thay đổi Model:
- [ ] Thay đổi Model trong code
- [ ] Tạo migration: `dotnet ef migrations add MigrationName`
- [ ] Review migration file (kiểm tra an toàn)
- [ ] Test migration: `dotnet ef database update`
- [ ] Test app chạy OK
- [ ] Commit migration file
- [ ] Push lên GitHub

Khi pull code mới:
- [ ] Pull code mới
- [ ] Chạy app → Migration tự động apply
- [ ] Check console: `✅ Database migrations applied successfully`
- [ ] Test app hoạt động OK

## 📚 Tài liệu tham khảo

- Xem `MIGRATION_WORKFLOW.md` để biết quy trình chi tiết
- Xem `MIGRATION_GUIDE.md` để biết về migration VehicleModel/BatteryModel

