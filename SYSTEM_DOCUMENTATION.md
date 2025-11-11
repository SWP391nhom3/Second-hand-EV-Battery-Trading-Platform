# TÀI LIỆU HỆ THỐNG - NỀN TẢNG GIAO DỊCH PIN VÀ XE ĐIỆN QUA SỬ DỤNG

## 📋 THÔNG TIN TỔNG QUAN

**Tên dự án:** Second-hand EV & Battery Trading Platform  
**Mô tả:** Nền tảng giao dịch pin và xe điện qua sử dụng  
**Công nghệ Backend:** ASP.NET Core (.NET 8), Entity Framework Core, SQL Server  
**Công nghệ Frontend:** React 19, Ant Design, Vite  
**Cơ sở dữ liệu:** SQL Server với Entity Framework Core Migrations

---

## 📋 BẢNG USE CASE

| ID | Use Case | Primary Actor | Secondary Actor |
|----|----------|---------------|-----------------|
| 1 | Login | Member, Admin, Staff | - |
| 2 | Register | Guest | - |
| 3 | Đăng ký/Đăng nhập qua Google | Guest, Member | Google Service |
| 4 | Đăng ký/Đăng nhập qua Facebook | Guest, Member | Facebook System |
| 5 | Đăng nhập qua OTP | Guest, Member | Email Service, SMS Service |
| 6 | Quản lý tài khoản | Admin | - |
| 7 | Quản lý vai trò | Admin | - |
| 8 | Tạo/Cập nhật/Xóa tài khoản | Admin | - |
| 9 | Đặt lại mật khẩu | Admin | Email Service |
| 10 | Quản lý hồ sơ cá nhân | Member | - |
| 11 | Quản lý xe cộ (Vehicle) | Member | - |
| 12 | Quản lý pin (Battery) | Member | - |
| 13 | Đăng tin bán | Member | - |
| 14 | Chọn gói đăng bài | Member | Payment Gateway |
| 15 | Thanh toán gói đăng bài | Member | Payment Gateway (PayOS) |
| 16 | Upload hình ảnh sản phẩm | Member | File Storage System |
| 17 | Tìm kiếm sản phẩm | Member, Guest | - |
| 18 | Xem chi tiết sản phẩm | Member, Guest | - |
| 19 | Tạo PostRequest (Yêu cầu mua hàng) | Member (Buyer) | - |
| 20 | Xem PostRequest | Member (Seller) | - |
| 21 | Chấp nhận PostRequest | Member (Seller) | - |
| 22 | Từ chối PostRequest | Member (Seller) | - |
| 23 | Duyệt bài đăng | Admin | - |
| 24 | Từ chối bài đăng | Admin | - |
| 25 | Gán Staff cho bài đăng | Admin | - |
| 26 | Xử lý bài đăng STAFF_ASSISTED | Staff | Buyer, Seller |
| 27 | Liên hệ Buyer/Seller | Staff | Buyer, Seller |
| 28 | Tạo Construct (Hợp đồng) | Staff | Buyer, Seller |
| 29 | Tạo ConstructFee (Chi phí dự phòng) | Staff | Buyer, Seller |
| 30 | Sửa ConstructFee | Staff | Buyer, Seller |
| 31 | Xác nhận ConstructFee | Buyer, Seller | Staff |
| 32 | Thanh toán chi phí dự phòng | Buyer | Payment Gateway (PayOS) |
| 33 | Gặp mặt giao dịch | Buyer, Seller | Staff |
| 34 | Ký hợp đồng mua bán | Buyer, Seller | Staff |
| 35 | Hoàn tất giao dịch xe điện | Buyer, Seller | Staff |
| 36 | Giao dịch trực tiếp (Pin) | Buyer, Seller | - |
| 37 | Thanh toán mua hàng | Buyer | Payment Gateway (COD, PayOS) |
| 38 | Xem lịch sử giao dịch | Member | - |
| 39 | Xem lịch sử thanh toán | Member | - |
| 40 | Quản lý gói đăng bài | Admin | - |
| 41 | Xem thống kê | Admin | - |
| 42 | Phát hiện spam | Admin | - |
| 43 | Cảnh báo thành viên | Admin | - |
| 44 | Chặn đăng bài | Admin | - |
| 45 | Xem dashboard | Admin | - |

---

## 👥 VAI TRÒ VÀ QUYỀN HẠN (ROLES & PERMISSIONS)

### 1. ADMIN (Quản trị viên)
- **Quản lý tài khoản:**
  - Tạo, cập nhật, xóa tài khoản
  - Đặt lại mật khẩu
  - Quản lý trạng thái tài khoản (ACTIVE/INACTIVE)
  
- **Quản lý vai trò:**
  - Tạo, cập nhật vai trò
  - Khởi tạo mặc định: Admin, Staff, Member
  - Quản lý trạng thái vai trò (ACTIVE/INACTIVE)

- **Quản lý bài đăng:**
  - Duyệt bài đăng (approve/reject)
  - Phân công nhân viên cho bài đăng STAFF_ASSISTED
  - Cập nhật, xóa bài đăng
  - Xem tất cả bài đăng với các trạng thái

- **Quản lý gói bài đăng:**
  - Tạo, cập nhật, xóa gói (Cơ Bản, Tiêu Chuẩn, Premium)
  - Quản lý đăng ký gói của thành viên

- **Quản lý thanh toán/phí dịch vụ:**
  - Xem tất cả thanh toán
  - Quản lý phí dịch vụ (ServiceFee)
  - Quản lý hợp đồng (Construct) và phí hợp đồng (ConstructFee)

- **Phân tích/thống kê:**
  - Xem dashboard thống kê
  - Báo cáo doanh thu, số lượng giao dịch

### 2. STAFF (Nhân viên)
- **Đăng nhập:**
  - Đăng nhập qua `/api/auth/login` (bỏ qua Member.Status check)
  - Hoặc đăng nhập qua `/api/auth/staff-login` (chỉ dành cho Staff)

- **Xử lý bài đăng:**
  - Xem các bài đăng STAFF_ASSISTED được Admin chỉ định
  - Cập nhật trạng thái/tiến độ bài đăng
  - Giao tiếp với người mua/người bán

- **Hạn chế:**
  - Không thể quản lý vai trò
  - Không thể xóa tài khoản tùy ý
  - Chỉ xử lý các bài đăng được gán

### 3. MEMBER (Thành viên)
- **Đăng ký & Đăng nhập:**
  - Đăng ký qua email, mật khẩu
  - Đăng nhập qua `/api/auth/login`
  - Google OAuth (tạm thời disabled)
  - OTP verification (tạm thời disabled)

- **Quản lý hồ sơ:**
  - Cập nhật thông tin cá nhân (FullName, AvatarUrl, Address)
  - Quản lý xe cộ (Vehicle) và pin (Battery) của mình
  - Xem lịch sử giao dịch

- **Đăng tin bán:**
  - Tạo bài đăng (Post) với thông tin xe/pin
  - Chọn gói đăng bài (Cơ Bản, Tiêu Chuẩn, Premium)
  - Upload hình ảnh, thông số kỹ thuật

- **Tìm kiếm & Mua:**
  - Tìm kiếm theo hãng, đời, dung lượng pin, giá, tình trạng
  - Theo dõi tin yêu thích
  - So sánh nhiều sản phẩm
  - Tạo PostRequest để mua hàng

- **Giao dịch:**
  - **DIRECT (Trực tiếp):** Liên hệ và thanh toán trực tiếp với người bán
  - **STAFF_ASSISTED (Có nhân viên hỗ trợ):** Admin chỉ định Staff để hỗ trợ

- **Thanh toán:**
  - Thanh toán gói đăng bài
  - Thanh toán mua hàng (COD,PayOS - cần tích hợp)
  - Xem lịch sử thanh toán

### 4. GUEST (Khách)
- Xem danh sách bài đăng công khai
- Xem chi tiết bài đăng
- Đăng ký/Đăng nhập

---

## 📊 MÔ HÌNH DỮ LIỆU (DATABASE MODELS)

### Core Models

#### Account
- `AccountId` (PK)
- `Email` (unique, required)
- `Phone` (optional)
- `PasswordHash` (SHA256)
- `RoleId` (FK → Role)
- `CreatedAt`
- `GoogleId` (optional, cho OAuth)
- `EmailVerified` (boolean)
- `LastLoginAt` (nullable)

**Relationships:**
- 1 Account → 1 Role (N:1)
- 1 Account → 1 Member (1:1, optional)

#### Role
- `RoleId` (PK)
- `Name` (Admin, Staff, Member)
- `Status` (ACTIVE/INACTIVE)

#### Member
- `MemberId` (PK)
- `AccountId` (FK → Account, unique)
- `FullName`
- `AvatarUrl`
- `Address`
- `JoinedAt`
- `Rating` (decimal, default 0)
- `Status` (ACTIVE/INACTIVE)

**Relationships:**
- 1 Member → N Vehicles
- 1 Member → N Batteries
- 1 Member → N Posts
- 1 Member → N PostPackageSubs
- 1 Member → N PostRequests (as Buyer)
- 1 Member → N Payments (as Buyer/Seller)
- 1 Member → N ConstructFees

#### Post (Bài đăng)
- `PostId` (PK)
- `MemberId` (FK → Member, người đăng bài)
- `VehicleId` (FK → Vehicle, nullable)
- `BatteryId` (FK → Battery, nullable)
- `Title`
- `Description`
- `Price` (decimal)
- `PostType` (E-Vehicle, E-Bike, Battery)
- `TransactionType` (DIRECT | STAFF_ASSISTED)
- `StaffId` (FK → Member, nullable, nhân viên được gán)
- `ContactInfo` (nullable, cho DIRECT)
- `Status` (ACTIVE, PENDING_ASSIGN, IN_PROGRESS, SOLD, EXPIRED, REJECTED)
- `CreatedAt`
- `UpdatedAt`
- `ExpiryDate` (nullable, tính từ gói đăng bài)
- `Featured` (boolean, tự động set nếu PriorityLevel >= 3)

**Relationships:**
- 1 Post → 1 Member (N:1)
- 1 Post → 1 Vehicle (optional, N:1)
- 1 Post → 1 Battery (optional, N:1)
- 1 Post → 1 Staff Member (optional, N:1)
- 1 Post → N PostPackageSubs
- 1 Post → N PostRequests

**Business Logic:**
- Nếu `PostType == "e-vehicle"` → `TransactionType = "STAFF_ASSISTED"`, `Status = "PENDING_ASSIGN"`
- Nếu `PostType != "e-vehicle"` → `TransactionType = "DIRECT"`, `Status = "ACTIVE"`

#### PostPackage (Gói đăng bài)
- `PackageId` (PK)
- `Name` (Gói Cơ Bản, Gói Tiêu Chuẩn, Gói Premium)
- `DurationDay` (7, 14, 30)
- `Price` (50000, 90000, 180000)
- `PriorityLevel` (1, 2, 3)
- `Description`

**Default Packages:**
1. **Gói Cơ Bản:** 50,000 VNĐ, 7 ngày, PriorityLevel 1
2. **Gói Tiêu Chuẩn:** 90,000 VNĐ, 14 ngày, PriorityLevel 2
3. **Gói Premium:** 180,000 VNĐ, 30 ngày, PriorityLevel 3

#### PostPackageSub (Đăng ký gói của thành viên)
- `Id` (PK)
- `PostId` (FK → Post, nullable - cho phép mua gói trước khi đăng bài)
- `PackageId` (FK → PostPackage)
- `MemberId` (FK → Member)
- `StartDate`
- `EndDate` (StartDate + DurationDay)
- `PaymentId` (FK → Payment)
- `Status` (ACTIVE, EXPIRED, CANCELLED)

**Business Logic:**
- Cho phép mua gói trước khi đăng bài (`PostId = null`)
- Khi tạo Post, tự động link với gói ACTIVE chưa được gán (`PostId == null`)
- Ưu tiên gói có PriorityLevel cao nhất
- Nếu PriorityLevel >= 3 → Post.Featured = true

#### PostRequest (Yêu cầu mua hàng)
- `Id` (PK)
- `PostId` (FK → Post)
- `BuyerId` (FK → Member)
- `ConstructId` (FK → Construct, nullable)
- `Message`
- `OfferPrice` (decimal)
- `Status` (PENDING, ACCEPTED, REJECTED, CANCELLED)
- `CreatedAt`

**Business Logic:**
- Khi seller accept một request → reject tất cả request khác của cùng Post
- Khi seller accept → Post.Status = "RESERVED" (tạm ẩn khỏi sàn)
- Khi giao dịch hoàn thành → Post.Status = "SOLD"

#### Vehicle (Xe điện)
- `Id` (PK)
- `MemberId` (FK → Member)
- `VehicleModelId` (FK → VehicleModel, nullable)
- `Brand`
- `Model`
- `ManufactureYear`
- `MileageKm`
- `BatteryCapacity` (decimal)
- `Condition` (Good, Fair, Poor)
- `Description`

**Relationships:**
- 1 Vehicle → 1 VehicleModel (optional, N:1)
- 1 Vehicle → N Posts

#### Battery (Pin)
- `BatteryId` (PK)
- `MemberId` (FK → Member)
- `BatteryModelId` (FK → BatteryModel, nullable)
- `Brand`
- `CapacityKWh` (decimal)
- `CycleCount` (int)
- `ManufactureYear`
- `Condition` (Good, Fair, Poor)
- `Description`

**Relationships:**
- 1 Battery → 1 BatteryModel (optional, N:1)
- 1 Battery → N Posts

#### VehicleModel (Mẫu xe chuẩn)
- `VehicleModelId` (PK)
- `Name` (e.g., "VinFast VF e34")
- `Brand` (e.g., "VinFast")
- `Year` (nullable)
- `Type` (SUV, Sedan, E-Bike)
- `MotorPower` (nullable, decimal)
- `BatteryType` (LFP, NMC, Li-ion)
- `Voltage` (nullable, decimal)
- `MaxSpeed` (nullable)
- `Range` (nullable, km)
- `Weight` (nullable, kg)
- `Seats` (nullable)
- `Description`
- `IsCustom` (boolean, false = model chuẩn, true = custom)
- `IsApproved` (boolean, chỉ áp dụng cho custom model)
- `CreatedAt`

#### BatteryModel (Mẫu pin chuẩn)
- `BatteryModelId` (PK)
- `Name` (e.g., "Lithium-ion 48V 20Ah")
- `Brand` (e.g., "LG")
- `Chemistry` (Li-ion, NMC, LFP)
- `Voltage` (nullable, decimal)
- `CapacityKWh` (nullable, decimal)
- `Amperage` (nullable)
- `FormFactor` (Rectangular, Pouch, Prismatic)
- `Weight` (nullable, decimal)
- `Cycles` (nullable, số lần sạc)
- `Description`
- `ImageUrl` (nullable)
- `IsCustom` (boolean)
- `IsApproved` (boolean)
- `CreatedAt`

#### Payment (Thanh toán)
- `Id` (PK)
- `BuyerId` (FK → Member)
- `SellerId` (FK → Member)
- `Amount` (decimal)
- `Method` (Banking, COD, VNPay, PayOS)
- `TransferContent` (nội dung chuyển khoản)
- `Status` (PENDING, COMPLETED, REFUNDED, FAILED)
- `CreatedAt`

**Relationships:**
- 1 Payment → N Constructs
- 1 Payment → N PostPackageSubs

#### Construct (Hợp đồng mua bán)
- `ConstructId` (PK)
- `Name`
- `Address` (địa chỉ gặp mặt)
- `Contact`
- `Type`
- `PaymentId` (FK → Payment)
- `Status` (ACTIVE, COMPLETED, CANCELLED)

**Relationships:**
- 1 Construct → 1 Payment (N:1)
- 1 Construct → N ConstructFees
- 1 Construct → N PostRequests

#### ConstructFee (Chi phí dự phòng)
- `Id` (PK)
- `ConstructId` (FK → Construct)
- `MemberId` (FK → Member)
- `ServiceName`
- `Fee` (decimal)
- `CreatedAt`

**Relationships:**
- 1 ConstructFee → 1 Construct (N:1)
- 1 ConstructFee → 1 Member (N:1)
- 1 ConstructFee → 1 ServiceFee (1:1)

#### ServiceFee (Phí dịch vụ nền tảng)
- `Id` (PK)
- `ConstructFeeId` (FK → ConstructFee, unique)
- `Name`
- `Percentage` (decimal, phần trăm)
- `Status` (ACTIVE/INACTIVE)

**Business Logic:**
- Phí dịch vụ nền tảng là bắt buộc (phải OK)
- Phụ phí thêm có thể không OK (Staff có thể sửa lại)

---

## 🔄 LUỒNG NGHIỆP VỤ CHI TIẾT

### LUỒNG 1: ĐĂNG BÀI (POST CREATION FLOW)

#### Bước 1: Member đăng nhập
- Endpoint: `POST /api/auth/login`
- Input: `{ email, password }`
- Output: JWT token + account info + member info

#### Bước 2: Mua gói đăng bài (OPTIONAL - có thể mua trước)
- Endpoint: `POST /api/postpackage/{packageId}/subscribe`
- Input: `{ memberId, paymentId, postId? }`
- `postId` có thể null nếu mua gói trước khi đăng bài
- Output: PostPackageSub với `PostId = null`, `Status = "ACTIVE"`

#### Bước 3: Tạo bài đăng
- Endpoint: `POST /api/post`
- Input: `CreatePostDto`
  ```json
  {
    "memberId": 1,
    "title": "Bán xe điện VinFast VF e34",
    "description": "...",
    "price": 500000000,
    "postType": "e-vehicle" | "e-bike" | "battery",
    "vehicleModelId": 1,  // hoặc vehicle object hoặc vehicleId
    "batteryModelId": 1,  // hoặc battery object hoặc batteryId
    "vehicleCondition": "Good",
    "vehicleMileageKm": 15000,
    "batteryCycleCount": 500,
    "batteryCondition": "Good"
  }
  ```

**Business Logic:**
- Nếu `postType == "e-vehicle"`:
  - `transactionType = "STAFF_ASSISTED"`
  - `status = "PENDING_ASSIGN"` (chờ admin gán staff)
- Nếu `postType != "e-vehicle"`:
  - `transactionType = "DIRECT"`
  - `status = "ACTIVE"` (tự động active)

**Tự động link với gói:**
- Tìm PostPackageSub ACTIVE chưa được gán (`PostId == null`)
- Ưu tiên gói có PriorityLevel cao nhất
- Link `PostId` với subscription
- Set `Post.ExpiryDate = subscription.EndDate`
- Nếu `PriorityLevel >= 3` → `Post.Featured = true`

#### Bước 4: Admin kiểm duyệt (CHO POST E-VEHICLE)
- Endpoint: `PUT /api/post/{postId}/assign-staff/{staffId}` (gán staff)
- Hoặc: `PUT /api/post/{postId}` (cập nhật status)

**Trạng thái bài đăng:**
- `PENDING_ASSIGN`: Chờ admin gán staff (cho e-vehicle)
- `IN_PROGRESS`: Đang được staff xử lý
- `ACTIVE`: Đã được duyệt và hiển thị công khai
- `RESERVED`: Đã có người mua được seller chấp nhận, tạm ẩn khỏi sàn để thỏa thuận
- `REJECTED`: Bị từ chối (có lý do)
- `SOLD`: Đã bán thành công
- `EXPIRED`: Hết hạn (ExpiryDate < DateTime.Now)

**Spam Detection (cần implement):**
- Nếu spam nhiều → cảnh báo
- Nếu spam quá nhiều → chặn đăng bài
- Cần thêm field: `WarningCount`, `IsBlocked` vào Member model

#### Bước 5: Thanh toán gói (NẾU CHƯA MUA)
- Endpoint: `POST /api/payment`
- Tạo Payment với `status = "PENDING"`
- Endpoint: `POST /api/postpackage/{packageId}/subscribe`
- Link Payment với PostPackageSub

#### Bước 6: Bài đăng được hiển thị
- Sắp xếp theo PriorityLevel (cao → thấp)
- Sau đó theo Featured (true → false)
- Cuối cùng theo CreatedAt (mới → cũ)

---

### LUỒNG 2: MUA HÀNG - XE ĐIỆN (STAFF_ASSISTED)

#### Bước 1: Member (Buyer) xem bài đăng
- Endpoint: `GET /api/post/{id}`
- Xem thông tin: Vehicle, Member (seller), Staff (nếu có), Price, Status
- Post có `transactionType = "STAFF_ASSISTED"` và `status = "ACTIVE"`

#### Bước 2: Buyer điền thông tin và tạo PostRequest
- Buyer tick/chọn các giá sơ bộ (giá đề xuất, điều kiện mua)
- Endpoint: `POST /api/postrequest`
- Input: `{ postId, buyerId, message, offerPrice }`
- `status = "PENDING"` (chờ seller xem và chấp nhận)

#### Bước 3: Seller xem PostRequest và chấp nhận
- Endpoint: `GET /api/postrequest/post/{postId}` - Seller xem các request
- Endpoint: `PUT /api/postrequest/{id}/accept` - Seller chấp nhận request
- **Khi Seller chấp nhận:**
  - PostRequest status = "ACCEPTED"
  - Post status = "RESERVED" (tạm ẩn khỏi sàn)
  - Tất cả PostRequest khác của Post này → status = "REJECTED"
  - Bài đăng không còn hiển thị công khai trên sàn

#### Bước 4: Admin xem request đã được chấp nhận và gán Staff
- Endpoint: `GET /api/postrequest/post/{postId}` - Admin xem request đã ACCEPTED
- Endpoint: `PUT /api/post/{postId}/assign-staff/{staffId}` - Admin gán staff
- Post status chuyển từ `RESERVED` → `IN_PROGRESS`

#### Bước 5: Staff liên hệ 2 bên
- Staff gọi điện cho Buyer và Seller
- Thỏa thuận về:
  - Giá cả
  - Địa điểm gặp mặt
  - Thời gian

#### Bước 6: Staff tạo Construct (Hợp đồng)
- Endpoint: `POST /api/construct`
- Input: `{ name, address, contact, type, paymentId }`
- `status = "ACTIVE"`

#### Bước 7: Staff tạo ConstructFee (Chi phí dự phòng)
- Endpoint: `POST /api/construct/{id}/fees`
- Input: `{ memberId, serviceName, fee }`
- Tạo ConstructFee với các loại:
  - **Chi phí nền tảng (ServiceFee):** Bắt buộc phải OK
  - **Phụ phí thêm:** Có thể không OK (Staff có thể sửa lại)

#### Bước 8: 2 bên xác nhận
- Buyer và Seller xem ConstructFee
- Nếu OK → chuyển sang bước 9
- Nếu không OK → Staff sửa lại ConstructFee → quay lại bước 7

#### Bước 9: Tạo Payment cho chi phí dự phòng
- Endpoint: `POST /api/payment`
- Input: `{ buyerId, sellerId, amount, method, transferContent }`
- `status = "PENDING"`
- Link Payment với Construct

#### Bước 10: Gặp mặt
- Staff hướng dẫn gặp mặt tại `Construct.Address`
- Kiểm tra sản phẩm
- Nếu OK → ký hợp đồng mua bán
- Nếu không OK → hủy giao dịch, Post status = "ACTIVE" (hiển thị lại sàn)

#### Bước 11: Thanh toán
- Nếu OK → Payment status = "COMPLETED"
- Construct status = "COMPLETED"
- PostRequest status = "COMPLETED"
- Post status = "SOLD" (đã bán thành công)

#### Bước 12: Trả tiền chi phí dự phòng
- Payment status = "COMPLETED"
- ConstructFee được thanh toán
- ServiceFee được tính toán dựa trên Percentage

---

### LUỒNG 3: MUA HÀNG - PIN XE ĐIỆN (DIRECT)

#### Bước 1: Member (Buyer) xem bài đăng
- Endpoint: `GET /api/post/{id}`
- Post có `transactionType = "DIRECT"` và `status = "ACTIVE"`

#### Bước 2: Buyer điền thông tin và tạo PostRequest
- Buyer tick/chọn các giá sơ bộ (giá đề xuất, điều kiện mua)
- Endpoint: `POST /api/postrequest`
- Input: `{ postId, buyerId, message, offerPrice }`
- `status = "PENDING"` (chờ seller xem và chấp nhận)

#### Bước 3: Seller xem PostRequest và chấp nhận
- Endpoint: `GET /api/postrequest/post/{postId}` - Seller xem các request
- Endpoint: `PUT /api/postrequest/{id}/accept` - Seller chấp nhận request
- **Khi Seller chấp nhận:**
  - PostRequest status = "ACCEPTED"
  - Post status = "RESERVED" (tạm ẩn khỏi sàn)
  - Tất cả PostRequest khác của Post này → status = "REJECTED"
  - Bài đăng không còn hiển thị công khai trên sàn

#### Bước 4: Buyer và Seller liên hệ trực tiếp
- Qua `Post.ContactInfo` (nếu có)
- Hoặc qua thông tin Member
- Thỏa thuận chi tiết về giá cả, điều kiện giao hàng, thanh toán

#### Bước 5: Thỏa thuận và thanh toán
- 2 bên tự thỏa thuận
- Thanh toán trực tiếp (COD, PayOS)
- Tạo Payment record (nếu cần)

#### Bước 6: Hoàn tất giao dịch
- Nếu thành công:
  - PostRequest status = "COMPLETED"
  - Post status = "SOLD"
- Nếu không thành công:
  - PostRequest status = "CANCELLED"
  - Post status = "ACTIVE" (hiển thị lại sàn)

---

## 🔌 API ENDPOINTS CHI TIẾT

### Authentication (`/api/auth`)
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/login` - Đăng nhập (Member, Admin, Staff)
- `POST /api/auth/staff-login` - Đăng nhập Staff (chỉ Staff)
- `POST /api/auth/change-password` - Đổi mật khẩu
- `POST /api/auth/forgot-password` - Quên mật khẩu
- `POST /api/auth/create-admin` - Tạo Admin (dev only)
- `POST /api/auth/create-staff` - Tạo Staff (dev only)

### Accounts (`/api/account`)
- `GET /api/account` - Lấy tất cả tài khoản (Admin)
- `GET /api/account/{id}` - Lấy tài khoản theo ID
- `POST /api/account` - Tạo tài khoản
- `PUT /api/account/{id}` - Cập nhật tài khoản
- `DELETE /api/account/{id}` - Xóa tài khoản

### Roles (`/api/role`)
- `GET /api/role` - Lấy tất cả vai trò
- `GET /api/role/{id}` - Lấy vai trò theo ID
- `POST /api/role` - Tạo vai trò
- `PUT /api/role/{id}` - Cập nhật vai trò
- `DELETE /api/role/{id}` - Xóa vai trò

### Members (`/api/member`)
- `GET /api/member` - Lấy tất cả thành viên
- `GET /api/member/{id}` - Lấy thành viên theo ID
- `PUT /api/member/{id}` - Cập nhật thông tin thành viên
- `GET /api/member/{id}/vehicles` - Lấy xe của thành viên
- `GET /api/member/{id}/batteries` - Lấy pin của thành viên
- `GET /api/member/{id}/posts` - Lấy bài đăng của thành viên

### Posts (`/api/post`)
- `GET /api/post` - Lấy tất cả bài đăng ACTIVE (sắp xếp theo PriorityLevel)
- `GET /api/post/{id}` - Lấy bài đăng theo ID
- `GET /api/post/member/{memberId}` - Lấy bài đăng theo thành viên
- `POST /api/post` - Tạo bài đăng
- `PUT /api/post/{id}` - Cập nhật bài đăng
- `DELETE /api/post/{id}` - Xóa bài đăng
- `PUT /api/post/{postId}/assign-staff/{staffId}` - Admin gán staff
- `GET /api/post/featured` - Lấy bài đăng nổi bật
- `GET /api/post/direct` - Lấy bài đăng DIRECT
- `GET /api/post/staff-assisted` - Lấy bài đăng STAFF_ASSISTED

### Post Packages (`/api/postpackage`)
- `GET /api/postpackage` - Lấy tất cả gói
- `GET /api/postpackage/{id}` - Lấy gói theo ID
- `POST /api/postpackage` - Tạo gói (Admin)
- `PUT /api/postpackage/{id}` - Cập nhật gói (Admin)
- `DELETE /api/postpackage/{id}` - Xóa gói (Admin)
- `GET /api/postpackage/active` - Lấy gói đang active
- `POST /api/postpackage/{packageId}/subscribe` - Mua gói
- `GET /api/postpackage/{id}/subscriptions` - Lấy đăng ký của gói
- `GET /api/postpackage/statistics` - Thống kê gói

### Post Requests (`/api/postrequest`)
- `GET /api/postrequest` - Lấy tất cả request
- `GET /api/postrequest/{id}` - Lấy request theo ID
- `GET /api/postrequest/post/{postId}` - Lấy request của bài đăng
- `GET /api/postrequest/buyer/{buyerId}` - Lấy request của buyer
- `GET /api/postrequest/status/{status}` - Lấy request theo status
- `POST /api/postrequest` - Tạo request
- `PUT /api/postrequest/{id}` - Cập nhật request
- `PUT /api/postrequest/{id}/status` - Cập nhật status
- `PUT /api/postrequest/{id}/accept` - Chấp nhận request
- `PUT /api/postrequest/{id}/reject` - Từ chối request
- `DELETE /api/postrequest/{id}` - Xóa request
- `GET /api/postrequest/statistics` - Thống kê
- `GET /api/postrequest/negotiations/{postId}` - Lấy đàm phán của bài đăng

### Vehicles (`/api/vehicle`)
- `GET /api/vehicle` - Lấy tất cả xe
- `GET /api/vehicle/{id}` - Lấy xe theo ID
- `POST /api/vehicle` - Tạo xe
- `PUT /api/vehicle/{id}` - Cập nhật xe
- `DELETE /api/vehicle/{id}` - Xóa xe

### Batteries (`/api/battery`)
- `GET /api/battery` - Lấy tất cả pin
- `GET /api/battery/{id}` - Lấy pin theo ID
- `POST /api/battery` - Tạo pin
- `PUT /api/battery/{id}` - Cập nhật pin
- `DELETE /api/battery/{id}` - Xóa pin

### Vehicle Models (`/api/vehiclemodel`)
- `GET /api/vehiclemodel` - Lấy tất cả mẫu xe
- `GET /api/vehiclemodel/{id}` - Lấy mẫu xe theo ID
- `POST /api/vehiclemodel` - Tạo mẫu xe (custom)
- `PUT /api/vehiclemodel/{id}` - Cập nhật mẫu xe
- `DELETE /api/vehiclemodel/{id}` - Xóa mẫu xe
- `GET /api/vehiclemodel/approved` - Lấy mẫu đã duyệt
- `GET /api/vehiclemodel/search` - Tìm kiếm mẫu xe

### Battery Models (`/api/batterymodel`)
- `GET /api/batterymodel` - Lấy tất cả mẫu pin
- `GET /api/batterymodel/{id}` - Lấy mẫu pin theo ID
- `POST /api/batterymodel` - Tạo mẫu pin (custom)
- `PUT /api/batterymodel/{id}` - Cập nhật mẫu pin
- `DELETE /api/batterymodel/{id}` - Xóa mẫu pin
- `GET /api/batterymodel/approved` - Lấy mẫu đã duyệt
- `GET /api/batterymodel/search` - Tìm kiếm mẫu pin

### Payments (`/api/payment`)
- `GET /api/payment` - Lấy tất cả thanh toán
- `GET /api/payment/{id}` - Lấy thanh toán theo ID
- `GET /api/payment/buyer/{buyerId}` - Lấy thanh toán của buyer
- `GET /api/payment/seller/{sellerId}` - Lấy thanh toán của seller
- `GET /api/payment/status/{status}` - Lấy thanh toán theo status
- `POST /api/payment` - Tạo thanh toán
- `PUT /api/payment/{id}` - Cập nhật thanh toán
- `PUT /api/payment/{id}/status` - Cập nhật status
- `DELETE /api/payment/{id}` - Xóa thanh toán
- `POST /api/payment/process/{id}` - Xử lý thanh toán (simulate)
- `GET /api/payment/statistics` - Thống kê

### Constructs (`/api/construct`)
- `GET /api/construct` - Lấy tất cả hợp đồng
- `GET /api/construct/{id}` - Lấy hợp đồng theo ID
- `GET /api/construct/type/{type}` - Lấy hợp đồng theo type
- `GET /api/construct/status/{status}` - Lấy hợp đồng theo status
- `POST /api/construct` - Tạo hợp đồng
- `PUT /api/construct/{id}` - Cập nhật hợp đồng
- `PUT /api/construct/{id}/status` - Cập nhật status
- `DELETE /api/construct/{id}` - Xóa hợp đồng
- `GET /api/construct/{id}/fees` - Lấy chi phí của hợp đồng
- `POST /api/construct/{id}/fees` - Thêm chi phí
- `GET /api/construct/search` - Tìm kiếm hợp đồng
- `GET /api/construct/statistics` - Thống kê
- `GET /api/construct/nearby` - Lấy hợp đồng gần đây

---

## 🔐 XÁC THỰC VÀ PHÂN QUYỀN

### JWT Authentication
- Sử dụng JWT Bearer Token
- Header: `Authorization: Bearer {token}`
- Token chứa: `accountId`, `email`, `role`
- Token validation: Issuer, Audience, Expiration, SigningKey

### Role-based Authorization
- **Admin:** Full access
- **Staff:** Chỉ xử lý bài đăng được gán
- **Member:** Chỉ xử lý tài nguyên của chính mình
- **Guest:** Chỉ xem công khai

### CORS Configuration
- Allowed Origins: Configurable (default: localhost:5173, localhost:3000)
- Methods: Any
- Headers: Any
- Credentials: Allowed

---

## 💳 THANH TOÁN

### Payment Methods
- **COD** (Cash on Delivery)
- **VNPay** (cần tích hợp)
- **PayOS** (cần tích hợp)
- **Banking** (Chuyển khoản ngân hàng)

### Payment Status
- `PENDING`: Chờ xử lý
- `COMPLETED`: Hoàn thành
- `REFUNDED`: Đã hoàn tiền
- `FAILED`: Thất bại

### Payment Flow
1. Tạo Payment với `status = "PENDING"`
2. Xử lý thanh toán (tích hợp gateway)
3. Callback từ gateway → update `status = "COMPLETED"`
4. Hoặc manually update qua `PUT /api/payment/{id}/status`

---

## 📈 SẮP XẾP VÀ ƯU TIÊN

### Post Sorting Logic
1. **PriorityLevel** (từ PostPackage.PriorityLevel) - cao → thấp
2. **Featured** (true → false)
3. **CreatedAt** (mới → cũ)

### Priority Levels
- **Level 1:** Gói Cơ Bản (7 ngày)
- **Level 2:** Gói Tiêu Chuẩn (14 ngày)
- **Level 3:** Gói Premium (30 ngày, Featured)

---

## ⚠️ CẦN BỔ SUNG/IMPLEMENT

### 1. Post Approval System
- Thêm field `ApprovalStatus` vào Post: `PENDING`, `APPROVED`, `REJECTED`
- Thêm field `RejectionReason` vào Post (nullable)
- Admin approve/reject bài đăng
- Chỉ bài đăng APPROVED mới được hiển thị

### 2. Spam Detection & Warning System
- Thêm field `WarningCount` vào Member (default 0)
- Thêm field `IsBlocked` vào Member (default false)
- Logic:
  - Nếu bài đăng bị reject nhiều → tăng WarningCount
  - Nếu WarningCount >= 3 → IsBlocked = true
  - Member bị block không thể đăng bài mới

### 3. Payment Gateway Integration
- Tích hợp VNPay
- Tích hợp PayOS
- Webhook handling cho payment callbacks

### 4. Image Upload
- Đã có `wwwroot/uploads/` directory
- Cần implement upload endpoints
- Link images với Post, Vehicle, Battery

### 5. Search & Filter
- Tìm kiếm theo: hãng, đời, dung lượng pin, giá, tình trạng, số km, năm sản xuất
- Filter theo PostType, TransactionType, Status
- Sort theo: giá, ngày đăng, độ ưu tiên

### 6. Notification System
- Thông báo khi bài đăng được duyệt
- Thông báo khi có request mua hàng
- Thông báo khi staff được gán
- Email/SMS notifications

### 7. Rating & Review System
- Thêm model Review
- Rating cho Buyer và Seller sau giao dịch
- Cập nhật Member.Rating dựa trên reviews

---

## 📝 GHI CHÚ QUAN TRỌNG

1. **PostId nullable trong PostPackageSub:**
   - Cho phép mua gói trước khi đăng bài
   - Khi tạo Post, tự động link với gói ACTIVE chưa được gán

2. **TransactionType tự động:**
   - E-Vehicle → STAFF_ASSISTED → PENDING_ASSIGN
   - E-Bike/Battery → DIRECT → ACTIVE

3. **PriorityLevel tự động set Featured:**
   - PriorityLevel >= 3 → Featured = true

4. **Accept Request:**
   - Khi accept một request → reject tất cả request khác của cùng Post
   - Post status → SOLD

5. **ConstructFee:**
   - Chi phí nền tảng (ServiceFee) bắt buộc phải OK
   - Phụ phí thêm có thể không OK (Staff có thể sửa)

6. **Default Data:**
   - Roles: Admin, Staff, Member (tự động seed)
   - Post Packages: 3 gói (Cơ Bản, Tiêu Chuẩn, Premium)
   - Test accounts: admin@demo.com, staff@demo.com, user1-3@demo.com

---

## 🎯 KẾT LUẬN

Tài liệu này mô tả chi tiết hệ thống dựa trên codebase hiện có. Các luồng nghiệp vụ, models, và API endpoints đã được implement và sẵn sàng cho việc viết SWR (Software Requirements Specification) và các tài liệu kỹ thuật tiếp theo.

**Lưu ý:** Một số tính năng như spam detection, post approval system, và payment gateway integration cần được implement thêm trong tương lai.

