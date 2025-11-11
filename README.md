# Second-hand-EV-Battery-Trading-Platform

Hệ thống Backend API cho Sàn giao dịch C2C Xe điện và Pin được xây dựng bằng .NET 8.0 và SQL Server.

## Kiến trúc

Dự án được tổ chức theo Clean Architecture và SOLID principles:

- **EVehicle.API**: Presentation layer (Controllers, Middleware)
- **EVehicle.Application**: Application layer (Business logic, Use cases, DTOs, Validators)
- **EVehicle.Domain**: Domain layer (Entities, Domain models)
- **EVehicle.Infrastructure**: Infrastructure layer (Data access, Repositories, External services)

## Yêu cầu

- .NET 8.0 SDK
- Docker và Docker Compose (tùy chọn)
- SQL Server (chạy trên Docker hoặc local)

## Cấu hình và Chạy

### 1. Chạy với Docker Compose (Khuyến nghị)

```bash
# Build và chạy tất cả services
docker-compose up -d

# Xem logs
docker-compose logs -f

# Dừng services
docker-compose down

# Dừng và xóa volumes
docker-compose down -v
```

API sẽ chạy tại: `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`
SQL Server: `localhost:1433`

### 2. Chạy Local Development

```bash
# Restore packages
dotnet restore EVehicle.sln

# Build solution
dotnet build EVehicle.sln

# Chạy migrations
dotnet ef database update --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API

# Chạy API
dotnet run --project src/EVehicle.API
```

## Database Migrations

### Tạo Migration

```bash
dotnet ef migrations add MigrationName --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API
```

### Apply Migration

```bash
dotnet ef database update --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API
```

## Cấu hình

### Connection String

Cấu hình connection string trong `src/EVehicle.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=EVehicleDB;User Id=sa;Password=StrongPass123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Lưu ý**: Thay đổi password trong production!

### Environment Variables

Có thể override connection string qua environment variables:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=EVehicleDB;User Id=sa;Password=StrongPass123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

## Health Check

Kiểm tra trạng thái API và database:

```bash
curl http://localhost:5000/api/health
```

## Cấu trúc Thư mục

```
Second-hand-EV-Battery-Trading-Platform/
├── src/
│   ├── EVehicle.API/              # Presentation layer
│   │   ├── Controllers/           # API Controllers
│   │   ├── Middleware/           # Custom middleware
│   │   ├── Authorization/         # Authorization policies
│   │   ├── Program.cs             # Startup configuration
│   │   └── appsettings.json      # Configuration
│   ├── EVehicle.Application/      # Application layer
│   │   ├── DTOs/                  # Data Transfer Objects
│   │   ├── Services/              # Business logic services
│   │   ├── Validators/            # FluentValidation validators
│   │   └── Interfaces/            # Service interfaces
│   ├── EVehicle.Domain/           # Domain layer
│   │   ├── Entities/              # Domain entities
│   │   └── Common/                # Common base classes
│   └── EVehicle.Infrastructure/   # Infrastructure layer
│       ├── Data/                  # DbContext, Migrations
│       ├── Repositories/          # Repository implementations
│       └── Services/             # Infrastructure services
├── scripts/                       # Shell scripts for automation
├── docker-compose.yml            # Docker Compose configuration
├── Dockerfile                     # Docker image for API
├── Makefile                      # Make commands for common tasks
└── README.md                      # This file
```

## Development Guidelines

### SOLID Principles

- **Single Responsibility**: Mỗi class chỉ có một lý do để thay đổi
- **Open/Closed**: Mở rộng bằng cách thêm mới, không sửa code cũ
- **Liskov Substitution**: Derived classes phải thay thế được base classes
- **Interface Segregation**: Interfaces nhỏ, cụ thể
- **Dependency Inversion**: Phụ thuộc vào abstractions, không phụ thuộc vào concrete classes

### Coding Standards

- Sử dụng async/await cho I/O operations
- Sử dụng dependency injection
- Validate input tại Application layer với FluentValidation
- Xử lý exceptions properly
- Logging cho debugging và monitoring

## Quick Start

### 1. Khởi chạy với Docker (Khuyến nghị)

```bash
# Build và chạy tất cả services
docker-compose up -d

# Xem logs
docker-compose logs -f api

# Kiểm tra health
curl http://localhost:5000/api/health
```

### 2. Tạo Migration và Apply

```bash
# Tạo migration mới
make migration NAME=InitialCreate

# Apply migration
make migrate
```

### 3. Truy cập Swagger UI

Mở trình duyệt và truy cập: `http://localhost:5000/swagger`

## Các lệnh Makefile hữu ích

```bash
make help          # Hiển thị tất cả các lệnh
make build         # Build Docker images
make up            # Start services
make down          # Stop services
make logs          # Xem logs
make migrate       # Apply migrations
make migration NAME=MigrationName  # Tạo migration mới
make clean         # Dừng và xóa volumes
```

## Tài liệu

- [PROJECT_STRUCTURE.md](./PROJECT_STRUCTURE.md) - Chi tiết cấu trúc dự án
- [API_DEVELOPMENT_GUIDE.md](./API_DEVELOPMENT_GUIDE.md) - Hướng dẫn phát triển API
- [PAYOS_UPDATE_SUMMARY.md](./PAYOS_UPDATE_SUMMARY.md) - Tổng hợp cập nhật PayOS
- [REBUILD_INSTRUCTIONS.md](./REBUILD_INSTRUCTIONS.md) - Hướng dẫn rebuild

## License

Copyright (c) 2024
