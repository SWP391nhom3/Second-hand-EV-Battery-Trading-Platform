<!-- 0fa9543d-6689-43b7-93e4-a400cbee86ea 641ec5a1-e488-42e6-8a44-0974891946d2 -->
# Kế hoạch nhập backend mới vào dự án EVehicleManagementAPI

## Giai đoạn 1 – Chuẩn bị kiến trúc

- audit-structure: Tạo mới các thư mục `Application`, `Domain`, `Infrastructure`, `API` (nếu cần) và cập nhật solution/csproj để hỗ trợ kiến trúc phân lớp.
- migrate-config: Di chuyển cấu hình Startup/Program sang pattern host builder và thêm các extension đăng ký DI theo phong cách dự án mới.

## Giai đoạn 2 – Domain & Persistence

- port-entities: Đồng bộ toàn bộ domain entities từ dự án mới (Appointments, Leads, Contracts, Orders, Bids, Chats, Notifications, Packages, Payments, Posts, Ratings...
- update-dbcontext: Thay `EVehicleDbContext` bằng phiên bản mới (ở layer Infrastructure), cấu hình Fluent API & relationships, thêm seeding sử dụng `DbSeeder`.
- import-migrations: Port các migration hiện có hoặc tái tạo migration tương đương cho schema mới.

## Giai đoạn 3 – Application layer

- port-dtos-validators: Sao chép DTOs và FluentValidators cho từng module (Auth, Posts, Packages, Orders...).
- port-services: Thêm service interfaces + implementations trong Application cho từng nghiệp vụ (PostService, PaymentService, AppointmentService, etc.), cấu hình DI.
- implement-repositories: Thêm repository interfaces + implementations trong Infrastructure cho các entity.

## Giai đoạn 4 – API layer

- port-controllers: Thay thế / mở rộng controllers hiện có với các controller từ dự án mới (AdminPosts, Payments, Orders, Users...). Kích hoạt middleware, attribute filter như RoleValidation.
- configure-middleware: Đăng ký middleware (AuthenticationDebug, RoleValidation), policy provider, health endpoints, static file hosting nếu cần.
- update-routing: Cập nhật endpoint routes, API versioning (nếu áp dụng), Swagger cấu hình mới.

## Giai đoạn 5 – Hạ tầng & tiện ích

- setup-integration: Thêm PayOS service mới, email/OTP, file storage, AI price service interfaces, background jobs nếu có.
- add-devops: Bổ sung Dockerfile, docker-compose, Makefile, scripts rebuild/migration theo dự án mới.
- add-docs: Sao chép tài liệu hướng dẫn (`API_DEVELOPMENT_GUIDE.md`, `PROJECT_STRUCTURE.md`, `PAYOS_UPDATE_SUMMARY.md`, etc.) và cập nhật README.

## Giai đoạn 6 – Kiểm thử & hoàn thiện

- update-tests: Nếu có unit/integration tests từ dự án mới, port chúng và đảm bảo build.
- run-migrations: Chạy `dotnet ef database update` với schema mới, validate seeding.
- smoke-test: Thực hiện kiểm thử thủ công cho các flow chính (auth, post, package purchase & PayOS checkout, appointments, leads...).
- finalize-commits: Chuẩn bị các commit nhỏ theo từng bước trên, ghi chú rõ ràng để dễ review.