# Default Seed Data - Thông tin dữ liệu mặc định

## 📋 Tổng quan

Khi chạy app lần đầu, hệ thống sẽ tự động tạo các dữ liệu mặc định để thuận tiện cho việc test.

## 🔐 Default Accounts

### Admin Account
- **Email:** `admin@demo.com`
- **Password:** `Admin@123`
- **Role:** Admin
- **Phone:** 0901234567

### Staff Account
- **Email:** `staff@demo.com`
- **Password:** `Staff@123`
- **Role:** Staff
- **Phone:** 0901234568

### Test Member Accounts (3 accounts)
1. **User 1**
   - Email: `user1@demo.com`
   - Password: `User1@123`
   - Name: Nguyễn Văn A
   - Phone: 0901111111
   - Status: ACTIVE

2. **User 2**
   - Email: `user2@demo.com`
   - Password: `User2@123`
   - Name: Trần Thị B
   - Phone: 0902222222
   - Status: ACTIVE

3. **User 3**
   - Email: `user3@demo.com`
   - Password: `User3@123`
   - Name: Lê Văn C
   - Phone: 0903333333
   - Status: ACTIVE

## 📦 Post Packages

1. **Gói Cơ Bản**
   - Duration: 7 ngày
   - Price: 50,000 VNĐ
   - Priority: 1

2. **Gói Tiêu Chuẩn**
   - Duration: 14 ngày
   - Price: 90,000 VNĐ
   - Priority: 2

3. **Gói Premium**
   - Duration: 30 ngày
   - Price: 180,000 VNĐ
   - Priority: 3

## 🚗 Vehicle Models (3 mẫu)

1. **VinFast VF e34**
   - Brand: VinFast
   - Type: SUV
   - Year: 2021
   - Motor Power: 110 kW
   - Range: 285 km

2. **Tesla Model 3**
   - Brand: Tesla
   - Type: Sedan
   - Year: 2023
   - Motor Power: 283 kW
   - Range: 547 km

3. **PEGA CITY**
   - Brand: PEGA
   - Type: E-Bike
   - Year: 2022
   - Motor Power: 1.5 kW
   - Range: 60 km

## 🔋 Battery Models (3 mẫu)

1. **Lithium-ion 48V 20Ah**
   - Brand: LG
   - Chemistry: Li-ion
   - Voltage: 48V
   - Capacity: 0.96 kWh
   - Cycles: 2000

2. **NMC 400V 60kWh**
   - Brand: CATL
   - Chemistry: NMC
   - Voltage: 400V
   - Capacity: 60 kWh
   - Cycles: 1500

3. **LFP 51.2V 100Ah**
   - Brand: BYD
   - Chemistry: LFP
   - Voltage: 51.2V
   - Capacity: 5.12 kWh
   - Cycles: 3000

## ✅ Tính năng

- **Idempotent:** Chỉ tạo nếu chưa có, không ghi đè dữ liệu hiện có
- **Tự động:** Chạy mỗi lần app start
- **An toàn:** Không xóa hoặc modify dữ liệu có sẵn

## 🎯 Sử dụng cho Testing

Bạn có thể sử dụng các accounts trên để:
- Test login với các role khác nhau
- Test các API endpoints
- Test các tính năng của hệ thống

