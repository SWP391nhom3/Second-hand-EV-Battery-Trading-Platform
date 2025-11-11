# Cấu trúc Project EVehicle API

## Tổng quan

Dự án được tổ chức theo Clean Architecture và SOLID principles, chia thành 4 layers chính:

```
EVehicle/
├── src/
│   ├── EVehicle.API/              # Presentation Layer (Controllers, Middleware)
│   ├── EVehicle.Application/      # Application Layer (Business Logic, Use Cases)
│   ├── EVehicle.Domain/           # Domain Layer (Entities, Domain Models)
│   └── EVehicle.Infrastructure/   # Infrastructure Layer (Data Access, External Services)
├── scripts/                       # Utility scripts
├── docker-compose.yml             # Docker Compose configuration
├── Dockerfile                     # Docker image for API
└── README.md                      # Documentation
```

## Chi tiết các Layers

### 1. EVehicle.Domain (Domain Layer)

**Trách nhiệm**: Chứa các domain entities và business logic cốt lõi

**Cấu trúc**:
```
EVehicle.Domain/
├── Entities/          # Domain entities
│   ├── User.cs
│   ├── Role.cs
│   ├── Permission.cs
│   ├── Category.cs
│   ├── Post.cs
│   └── PostImage.cs
└── Common/            # Common base classes
    └── BaseEntity.cs
```

**Nguyên tắc**:
- Không phụ thuộc vào bất kỳ layer nào khác
- Chứa pure domain logic
- Entities không có dependency vào framework

### 2. EVehicle.Application (Application Layer)

**Trách nhiệm**: Chứa business logic, use cases, và application services

**Cấu trúc** (sẽ được mở rộng):
```
EVehicle.Application/
├── Interfaces/        # Application interfaces
├── Services/          # Application services
├── DTOs/              # Data Transfer Objects
└── Mappings/          # AutoMapper profiles
```

**Nguyên tắc**:
- Phụ thuộc vào Domain layer
- Không phụ thuộc vào Infrastructure
- Chứa application-specific business logic

### 3. EVehicle.Infrastructure (Infrastructure Layer)

**Trách nhiệm**: Implement data access, external services

**Cấu trúc**:
```
EVehicle.Infrastructure/
├── Data/              # DbContext, Repositories
│   ├── EVehicleDbContext.cs
│   └── Migrations/    # EF Core migrations
├── InfrastructureExtensions.cs
└── Services/          # External services (sẽ được mở rộng)
```

**Nguyên tắc**:
- Implement interfaces từ Application layer
- Chứa framework-specific code (EF Core, etc.)
- Có thể thay thế bằng implementation khác

### 4. EVehicle.API (Presentation Layer)

**Trách nhiệm**: API endpoints, request/response handling

**Cấu trúc**:
```
EVehicle.API/
├── Controllers/       # API Controllers
│   ├── HealthController.cs
│   └── UsersController.cs
├── Program.cs         # Startup configuration
├── appsettings.json   # Configuration
└── Properties/        # Launch settings
```

**Nguyên tắc**:
- Thin controllers, chỉ xử lý HTTP requests/responses
- Business logic ở Application layer
- Validation, mapping DTOs

## Database Schema

Database được thiết kế theo schema trong tài liệu, sử dụng Entity Framework Core với SQL Server.

### Các bảng chính:

1. **Users**: Thông tin người dùng
2. **Roles**: Vai trò (MEMBER, ADMIN, MODERATOR)
3. **Permissions**: Quyền hạn
4. **User_Roles**: Gán vai trò cho người dùng
5. **Role_Permissions**: Gán quyền cho vai trò
6. **Categories**: Danh mục (Xe điện, Pin)
7. **Posts**: Bài đăng
8. **Post_Images**: Ảnh của bài đăng

## SOLID Principles

### Single Responsibility Principle (SRP)
- Mỗi class chỉ có một lý do để thay đổi
- Controllers chỉ xử lý HTTP, Services xử lý business logic

### Open/Closed Principle (OCP)
- Mở rộng bằng cách thêm mới, không sửa code cũ
- Sử dụng interfaces để mở rộng

### Liskov Substitution Principle (LSP)
- Derived classes có thể thay thế base classes
- Interfaces được implement đúng cách

### Interface Segregation Principle (ISP)
- Interfaces nhỏ, cụ thể
- Không force clients implement methods không cần

### Dependency Inversion Principle (DIP)
- Phụ thuộc vào abstractions (interfaces)
- Dependency Injection qua constructor

## Development Workflow

### 1. Tạo Migration

```bash
# Sử dụng script
./scripts/create-migration.sh MigrationName

# Hoặc dùng Makefile
make migration NAME=MigrationName

# Hoặc trực tiếp
dotnet ef migrations add MigrationName --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API
```

### 2. Apply Migration

```bash
# Sử dụng script
./scripts/update-db.sh

# Hoặc dùng Makefile
make migrate

# Hoặc trực tiếp
dotnet ef database update --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API
```

### 3. Chạy với Docker

```bash
# Build và chạy
docker-compose up -d

# Xem logs
docker-compose logs -f

# Dừng
docker-compose down
```

### 4. Chạy Local Development

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/EVehicle.API
```

## Coding Standards

### Naming Conventions
- **Classes**: PascalCase (User, PostController)
- **Methods**: PascalCase (GetUser, CreatePost)
- **Properties**: PascalCase (UserId, Email)
- **Variables**: camelCase (userId, email)
- **Constants**: UPPER_CASE (MAX_SIZE)

### File Organization
- Một class một file
- File name = Class name
- Namespace = Folder structure

### Async/Await
- Sử dụng async/await cho tất cả I/O operations
- Tên method kết thúc bằng Async (GetUserAsync)

### Error Handling
- Sử dụng try-catch trong controllers
- Log errors properly
- Return appropriate HTTP status codes

### Dependency Injection
- Inject dependencies qua constructor
- Register services trong InfrastructureExtensions
- Sử dụng interfaces, không phụ thuộc vào concrete classes

## Testing Strategy (Sẽ được mở rộng)

### Unit Tests
- Test business logic trong Application layer
- Mock dependencies

### Integration Tests
- Test API endpoints
- Test database operations

### End-to-End Tests
- Test complete workflows
- Test với test database

## Security Considerations

### Authentication & Authorization
- JWT tokens (sẽ được implement)
- Role-based access control (RBAC)
- Permission-based authorization

### Data Protection
- Hash passwords (BCrypt)
- Encrypt sensitive data
- SQL injection protection (EF Core parameterized queries)

### API Security
- CORS configuration
- Rate limiting (sẽ được implement)
- Input validation
- HTTPS in production

## Performance Optimization

### Database
- Indexes trên các columns thường query
- Eager loading vs Lazy loading
- Query optimization

### Caching
- Redis caching (sẽ được implement)
- Response caching
- Distributed caching

### API
- Pagination
- Filtering, Sorting
- Compression

## Monitoring & Logging

### Logging
- Structured logging với Serilog (sẽ được implement)
- Log levels: Debug, Information, Warning, Error
- Log to file, console, và external services

### Monitoring
- Health checks
- Application metrics
- Performance monitoring

## Deployment

### Docker
- Multi-stage build
- Optimized image size
- Health checks

### CI/CD (Sẽ được implement)
- Automated testing
- Automated deployment
- Version tagging

## Next Steps

1. Implement Authentication & Authorization
2. Implement Application Services
3. Implement Repositories pattern
4. Add Unit Tests
5. Add Integration Tests
6. Implement Caching
7. Implement Logging
8. Add API Documentation (Swagger/OpenAPI)
9. Implement Rate Limiting
10. Add Monitoring & Metrics

