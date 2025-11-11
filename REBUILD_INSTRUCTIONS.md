# Hướng dẫn Rebuild Backend sau khi cập nhật Payment Gateway

## Vấn đề
Backend đã được cập nhật để chỉ chấp nhận PAYOS, nhưng vẫn trả về lỗi "Chỉ chấp nhận VNPAY hoặc MOMO". Điều này xảy ra vì backend đang chạy code cũ.

## Giải pháp

### Nếu đang chạy bằng Docker:

1. **Stop Docker containers:**
```bash
cd BE
docker-compose down
```

2. **Rebuild Docker images:**
```bash
docker-compose build --no-cache
```

3. **Start lại services:**
```bash
docker-compose up -d
```

4. **Kiểm tra logs:**
```bash
docker-compose logs -f api
```

### Nếu đang chạy local (dotnet run):

1. **Stop API server** (Ctrl+C trong terminal đang chạy)

2. **Clean và rebuild:**
```bash
cd BE
dotnet clean
dotnet build
```

3. **Start lại:**
```bash
dotnet run --project src/EVehicle.API
```

### Nếu đang chạy bằng Visual Studio/IDE:

1. **Stop debug/run**
2. **Clean Solution** (Build -> Clean Solution)
3. **Rebuild Solution** (Build -> Rebuild Solution)
4. **Run lại**

## Kiểm tra sau khi rebuild

Sau khi rebuild và restart, kiểm tra:

1. Backend đã khởi động thành công
2. Thử gọi API với `paymentGateway: "PAYOS"` - không còn lỗi validation
3. Kiểm tra logs để đảm bảo không có lỗi

## Lưu ý

- Đảm bảo đã cập nhật tất cả validators:
  - `PaymentCreateRequestValidator.cs` - Chỉ chấp nhận PAYOS
  - `PackagePurchaseRequestValidator.cs` - Chỉ chấp nhận PAYOS

- Các file đã được cập nhật đúng, chỉ cần rebuild và restart backend.

