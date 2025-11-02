# Migration Workflow - Quy trình làm việc với Migration

## 🔍 Vấn đề hiện tại

Khi thay đổi model (thêm/bớt/sửa cột), migration không tự động cập nhật bảng trong database.

## ✅ Giải pháp

### Quy trình khi có thay đổi Model

#### Bước 1: Thay đổi Model trong code
```csharp
// Ví dụ: Thêm cột mới vào Account model
public class Account
{
    public int AccountId { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string NewColumn { get; set; } // ✅ Cột mới
}
```

#### Bước 2: Tạo migration mới
```bash
cd EVehicleManagementAPI/EVehicleManagementAPI

# Tạo migration mới (EF sẽ tự động detect thay đổi)
dotnet ef migrations add AddNewColumnToAccount

# Kiểm tra migration file được tạo
# Xem file: Migrations/YYYYMMDDHHMMSS_AddNewColumnToAccount.cs
```

#### Bước 3: Review migration file
**QUAN TRỌNG:** Kiểm tra file migration để đảm bảo:
- ✅ Chỉ thêm/xóa/sửa cột (không xóa dữ liệu)
- ✅ Không có `DropTable`, `DropColumn` trên bảng có dữ liệu
- ✅ Cột mới là nullable hoặc có default value

**Ví dụ migration AN TOÀN:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ✅ AN TOÀN: Thêm cột nullable
    migrationBuilder.AddColumn<string>(
        name: "NewColumn",
        table: "Accounts",
        nullable: true); // nullable = true → không mất dữ liệu

    // ✅ AN TOÀN: Thêm cột có default value
    migrationBuilder.AddColumn<int>(
        name: "Status",
        table: "Accounts",
        nullable: false,
        defaultValue: 1); // default value → không ảnh hưởng dữ liệu cũ
}
```

**Ví dụ migration NGUY HIỂM:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ❌ NGUY HIỂM: Drop column → mất dữ liệu
    migrationBuilder.DropColumn(
        name: "OldColumn",
        table: "Accounts");

    // ❌ NGUY HIỂM: Drop table → mất toàn bộ dữ liệu
    migrationBuilder.DropTable("Accounts");
}
```

#### Bước 4: Apply migration
```bash
# Apply migration vào database
dotnet ef database update

# Hoặc khi chạy app, migration sẽ tự động apply
dotnet run
```

#### Bước 5: Commit và push
```bash
git add Migrations/
git commit -m "feat: Add new column to Account table"
git push origin feature/working-branch-20251102
```

### Khi team members pull code về

**Quan trọng:** Migration sẽ **TỰ ĐỘNG** apply khi chạy app!

```bash
# 1. Pull code mới
git pull origin feature/working-branch-20251102

# 2. Chạy app → Migration tự động apply
cd EVehicleManagementAPI/EVehicleManagementAPI
dotnet run

# Console sẽ hiển thị:
# ✅ Database migrations applied successfully
```

**Nếu muốn apply migration thủ công:**
```bash
dotnet ef database update
```

## 📋 Best Practices

### 1. Luôn tạo migration khi thay đổi Model
- ✅ Thay đổi property trong Model → Tạo migration
- ✅ Thêm/bớt property → Tạo migration
- ✅ Thay đổi relationship → Tạo migration

### 2. Đặt tên migration rõ ràng
```bash
# ✅ Tốt
dotnet ef migrations add AddStatusToAccount
dotnet ef migrations add RemoveOldColumnFromPost
dotnet ef migrations add UpdateMemberTableStructure

# ❌ Không tốt
dotnet ef migrations add Update1
dotnet ef migrations add Migration
```

### 3. Review migration trước khi commit
- Kiểm tra `Up()` method có an toàn không
- Đảm bảo không mất dữ liệu
- Test trên database dev trước

### 4. Backup database trước khi apply migration lớn
```bash
# Backup database (SQL Server)
sqlcmd -S localhost -d EVehicleDB -Q "BACKUP DATABASE EVehicleDB TO DISK = 'backup.bak'"
```

## ⚠️ Lưu ý quan trọng

### Migration tự động trong Program.cs
Migration đã được config để tự động apply khi start app:
```csharp
db.Database.Migrate(); // ✅ Tự động apply pending migrations
```

### Nếu migration fail
- App vẫn có thể chạy (error được catch)
- Check console để xem error message
- Fix migration và apply lại

### Rollback migration
```bash
# Rollback về migration trước
dotnet ef database update PreviousMigrationName

# Hoặc xóa migration chưa apply
dotnet ef migrations remove
```

## 🔧 Troubleshooting

### Vấn đề: Migration không apply
**Nguyên nhân:**
- Database connection fail
- Migration file bị lỗi
- Conflict với database hiện tại

**Giải pháp:**
```bash
# 1. Check connection string
# 2. Check migration có lỗi không
dotnet ef migrations list
# 3. Apply thủ công
dotnet ef database update --verbose
```

### Vấn đề: Cột không được thêm
**Nguyên nhân:**
- Migration chưa được tạo
- Migration chưa được apply
- Migration có lỗi

**Giải pháp:**
```bash
# 1. Check migration đã tạo chưa
ls Migrations/ | grep AddColumn

# 2. Check migration đã apply chưa
dotnet ef migrations list

# 3. Apply migration
dotnet ef database update
```

## 📝 Checklist cho Developer

Khi thay đổi Model:
- [ ] Thay đổi Model trong code
- [ ] Tạo migration: `dotnet ef migrations add MigrationName`
- [ ] Review migration file (đảm bảo an toàn)
- [ ] Test migration trên local: `dotnet ef database update`
- [ ] Test app chạy OK
- [ ] Commit và push migration file
- [ ] Thông báo team về migration mới

Khi pull code mới:
- [ ] Pull code mới
- [ ] Chạy app → Migration tự động apply
- [ ] Kiểm tra console: `✅ Database migrations applied successfully`
- [ ] Test app hoạt động OK

