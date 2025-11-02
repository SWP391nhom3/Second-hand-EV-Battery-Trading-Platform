# Hướng dẫn Migration VehicleModel và BatteryModel

## ⚠️ LƯU Ý QUAN TRỌNG

Migration này **AN TOÀN** - không ghi đè dữ liệu hiện có:
- ✅ Chỉ thêm 2 bảng mới: `VehicleModels` và `BatteryModels`
- ✅ Thêm foreign key **nullable** vào bảng `Vehicles` và `Batteries` (không bắt buộc)
- ✅ Không có DropTable, DropColumn, hoặc DeleteData
- ✅ Dữ liệu hiện tại sẽ được giữ nguyên 100%

## Cách chạy migration

### 1. Backup database (khuyến nghị):
```bash
# Backup database trước khi chạy migration
sqlcmd -S localhost,1433 -d EVehicleDB -U sa -P StrongPass123! -Q "BACKUP DATABASE EVehicleDB TO DISK = 'C:\Backup\EVehicleDB_backup_$(date +%Y%m%d).bak'"
```

### 2. Pull code mới nhất:
```bash
git fetch origin
git checkout feature/working-branch-20251102
git pull origin feature/working-branch-20251102
```

### 3. Chạy migration:
```bash
cd EVehicleManagementAPI/EVehicleManagementAPI
dotnet ef database update
```

Hoặc khi chạy ứng dụng, migration sẽ tự động chạy (nếu có `db.Database.Migrate()` trong Program.cs)

## Kiểm tra sau migration

### Kiểm tra bảng mới đã được tạo:
```sql
SELECT * FROM VehicleModels;
SELECT * FROM BatteryModels;
```

### Kiểm tra foreign key đã được thêm (nullable):
```sql
-- Kiểm tra cột VehicleModelId và BatteryModelId đã được thêm
SELECT 
    COLUMN_NAME, 
    IS_NULLABLE, 
    DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Vehicles' 
    AND COLUMN_NAME IN ('VehicleModelId');

SELECT 
    COLUMN_NAME, 
    IS_NULLABLE, 
    DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Batteries' 
    AND COLUMN_NAME IN ('BatteryModelId');
```

## Nếu có lỗi migration

### Rollback migration:
```bash
dotnet ef database update [PreviousMigrationName]
```

### Hoặc xóa migration nếu chưa apply:
```bash
dotnet ef migrations remove
```

## Tính năng mới

Sau khi migration thành công, các API mới sẽ hoạt động:

- `GET /api/VehicleModel/list` - Lọc và tìm kiếm vehicle models
- `GET /api/VehicleModel/{id}` - Chi tiết vehicle model
- `POST /api/VehicleModel/custom` - Tạo custom vehicle model
- `GET /api/VehicleModel/all-filters` - Lấy các giá trị filter
- `GET /api/VehicleModel/search` - Tìm kiếm vehicle model

- `GET /api/BatteryModel/list` - Lọc và tìm kiếm battery models
- `GET /api/BatteryModel/{id}` - Chi tiết battery model
- `POST /api/BatteryModel/custom` - Tạo custom battery model
- `GET /api/BatteryModel/all-filters` - Lấy các giá trị filter
- `GET /api/BatteryModel/search` - Tìm kiếm battery model

## Lưu ý cho team

1. **Foreign key là nullable**: Vehicles và Batteries hiện tại không bắt buộc phải có VehicleModelId/BatteryModelId. Code cũ vẫn hoạt động bình thường.

2. **Migration idempotent**: Có thể chạy nhiều lần mà không gây lỗi.

3. **Không ảnh hưởng đến seed data**: Seeding roles và accounts vẫn hoạt động như cũ.

4. **Test trước khi deploy**: Nên test trên môi trường dev/staging trước.

## Liên hệ

Nếu có vấn đề khi chạy migration, liên hệ team lead hoặc tạo issue trên GitHub.

