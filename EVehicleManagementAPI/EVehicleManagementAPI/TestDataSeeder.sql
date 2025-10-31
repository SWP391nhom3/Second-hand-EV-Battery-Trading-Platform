-- Script seed data để test VehicleModel và BatteryModel APIs
-- Chạy script này trước khi test

USE EVehicleDB;
GO

-- 1. Đảm bảo có Member (Account + Role)
IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Email = 'test@example.com')
BEGIN
    -- Tạo Role nếu chưa có
    IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleId = 2)
    BEGIN
        INSERT INTO Roles (RoleId, Name, Status) VALUES (2, 'Member', 'ACTIVE');
    END
    
    -- Tạo Account
    DECLARE @AccountId INT;
    INSERT INTO Accounts (Email, PasswordHash, RoleId, EmailVerified, CreatedAt)
    VALUES ('test@example.com', '$2a$11$TestHashForTesting', 2, 1, GETDATE());
    SET @AccountId = SCOPE_IDENTITY();
    
    -- Tạo Member
    INSERT INTO Members (AccountId, FullName, AvatarUrl, Address, Status, JoinedAt)
    VALUES (@AccountId, 'Test User', 'https://example.com/avatar.jpg', '123 Test Street', 'ACTIVE', GETDATE());
END
GO

-- 2. Seed VehicleModels (chuẩn và custom)
IF NOT EXISTS (SELECT 1 FROM VehicleModels WHERE Name = 'VF 8')
BEGIN
    -- VinFast VF 8
    INSERT INTO VehicleModels (Name, Brand, Year, Type, MotorPower, BatteryType, Voltage, MaxSpeed, Range, Weight, Seats, Description, IsCustom, IsApproved, CreatedAt)
    VALUES ('VF 8', 'VinFast', 2023, 'SUV', 300, 'LFP', 400, 180, 450, 2200, 7, 'VinFast VF 8 - SUV điện hạng sang', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM VehicleModels WHERE Name = 'VF 9')
BEGIN
    -- VinFast VF 9
    INSERT INTO VehicleModels (Name, Brand, Year, Type, MotorPower, BatteryType, Voltage, MaxSpeed, Range, Weight, Seats, Description, IsCustom, IsApproved, CreatedAt)
    VALUES ('VF 9', 'VinFast', 2024, 'SUV', 350, 'LFP', 400, 200, 500, 2400, 7, 'VinFast VF 9 - SUV điện cao cấp', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM VehicleModels WHERE Name = 'Model 3')
BEGIN
    -- Tesla Model 3
    INSERT INTO VehicleModels (Name, Brand, Year, Type, MotorPower, BatteryType, Voltage, MaxSpeed, Range, Weight, Seats, Description, IsCustom, IsApproved, CreatedAt)
    VALUES ('Model 3', 'Tesla', 2023, 'Sedan', 283, 'NMC', 375, 225, 567, 1847, 5, 'Tesla Model 3 - Sedan điện phổ biến', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM VehicleModels WHERE Name = 'Atto 3')
BEGIN
    -- BYD Atto 3
    INSERT INTO VehicleModels (Name, Brand, Year, Type, MotorPower, BatteryType, Voltage, MaxSpeed, Range, Weight, Seats, Description, IsCustom, IsApproved, CreatedAt)
    VALUES ('Atto 3', 'BYD', 2023, 'SUV', 150, 'LFP', 400, 160, 480, 1750, 5, 'BYD Atto 3 - SUV điện giá rẻ', 0, 1, GETDATE());
END

-- Custom model chưa approved (để test)
IF NOT EXISTS (SELECT 1 FROM VehicleModels WHERE Name = 'Custom Model Test')
BEGIN
    INSERT INTO VehicleModels (Name, Brand, Year, Type, MotorPower, BatteryType, Voltage, MaxSpeed, Range, Weight, Seats, Description, IsCustom, IsApproved, CreatedAt)
    VALUES ('Custom Model Test', 'Test Brand', 2024, 'Sedan', 200, 'NMC', 350, 170, 400, 1800, 5, 'Custom model chưa được duyệt', 1, 0, GETDATE());
END
GO

-- 3. Seed BatteryModels
IF NOT EXISTS (SELECT 1 FROM BatteryModels WHERE Name = 'CATL 100kWh')
BEGIN
    INSERT INTO BatteryModels (Name, Brand, Chemistry, Voltage, CapacityKWh, Amperage, Weight, FormFactor, Cycles, Description, IsCustom, IsApproved, CreatedAt)
    VALUES ('CATL 100kWh', 'CATL', 'LFP', 400, 100, 250, 650, 'Prismatic', 4000, 'CATL 100kWh - Pin LFP dung lượng cao', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM BatteryModels WHERE Name = 'CATL 60kWh')
BEGIN
    INSERT INTO BatteryModels (Name, Brand, Chemistry, Voltage, CapacityKWh, Amperage, Weight, FormFactor, Cycles, Description, IsCustom, IsApproved, CreatedAt)
    VALUES ('CATL 60kWh', 'CATL', 'LFP', 400, 60, 150, 390, 'Prismatic', 4000, 'CATL 60kWh - Pin LFP tiêu chuẩn', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM BatteryModels WHERE Name = 'Panasonic 2170')
BEGIN
    INSERT INTO BatteryModels (Name, Brand, Chemistry, Voltage, CapacityKWh, Amperage, Weight, FormFactor, Cycles, Description, IsCustom, IsApproved, CreatedAt)
    VALUES ('Panasonic 2170', 'Panasonic', 'NCA', 375, 75, 200, 480, 'Cylindrical', 3000, 'Panasonic 2170 - Pin NCA cho Tesla', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM BatteryModels WHERE Name = 'VinES 42kWh')
BEGIN
    INSERT INTO BatteryModels (Name, Brand, Chemistry, Voltage, CapacityKWh, Amperage, Weight, FormFactor, Cycles, Description, IsCustom, IsApproved, CreatedAt)
    VALUES ('VinES 42kWh', 'VinES', 'LFP', 400, 42, 105, 280, 'Prismatic', 3500, 'VinES 42kWh - Pin cho VinFast', 0, 1, GETDATE());
END

-- Custom battery model chưa approved
IF NOT EXISTS (SELECT 1 FROM BatteryModels WHERE Name = 'Custom Battery Test')
BEGIN
    INSERT INTO BatteryModels (Name, Brand, Chemistry, Voltage, CapacityKWh, Amperage, Weight, FormFactor, Cycles, Description, IsCustom, IsApproved, CreatedAt)
    VALUES ('Custom Battery Test', 'Test Brand', 'NMC', 400, 80, 200, 500, 'Pouch', 2500, 'Custom battery chưa được duyệt', 1, 0, GETDATE());
END
GO

PRINT 'Seed data completed!';
PRINT 'You can now test the APIs.';
GO

