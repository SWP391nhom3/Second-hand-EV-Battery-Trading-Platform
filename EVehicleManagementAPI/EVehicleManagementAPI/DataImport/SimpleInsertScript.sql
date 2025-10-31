-- Script SQL Đơn Giản - Copy/Paste và Sửa Data
-- Team members chỉ cần copy dòng INSERT và sửa giá trị

USE EVehicleDB;
GO

-- ============================================
-- VEHICLE MODELS - Template
-- ============================================
-- Copy dòng dưới và sửa thông tin:

INSERT INTO VehicleModels (Name, Brand, Year, Type, MotorPower, BatteryType, Voltage, MaxSpeed, Range, Weight, Seats, Description, IsCustom, IsApproved, CreatedAt)
VALUES 
('VF 8', 'VinFast', 2023, 'SUV', 300, 'LFP', 400, 180, 450, 2200, 7, 'VinFast VF 8 - SUV điện hạng sang', 0, 1, GETDATE()),
('VF 9', 'VinFast', 2024, 'SUV', 350, 'LFP', 400, 200, 500, 2400, 7, 'VinFast VF 9 - SUV điện cao cấp', 0, 1, GETDATE()),
('VF e34', 'VinFast', 2023, 'Sedan', 150, 'LFP', 350, 160, 450, 1680, 5, 'VinFast VF e34 - Sedan điện', 0, 1, GETDATE()),
('Model 3', 'Tesla', 2023, 'Sedan', 283, 'NMC', 375, 225, 567, 1847, 5, 'Tesla Model 3 - Sedan điện phổ biến', 0, 1, GETDATE()),
('Model Y', 'Tesla', 2023, 'SUV', 318, 'NMC', 375, 217, 533, 2003, 7, 'Tesla Model Y - SUV điện', 0, 1, GETDATE()),
('Model S', 'Tesla', 2023, 'Sedan', 493, 'NMC', 375, 322, 652, 2241, 5, 'Tesla Model S - Sedan điện cao cấp', 0, 1, GETDATE()),
('Atto 3', 'BYD', 2023, 'SUV', 150, 'LFP', 400, 160, 480, 1750, 5, 'BYD Atto 3 - SUV điện giá rẻ', 0, 1, GETDATE()),
('Dolphin', 'BYD', 2023, 'Hatchback', 70, 'LFP', 400, 150, 405, 1625, 5, 'BYD Dolphin - Hatchback điện', 0, 1, GETDATE()),
('Seal', 'BYD', 2024, 'Sedan', 230, 'LFP', 400, 180, 570, 2150, 5, 'BYD Seal - Sedan điện', 0, 1, GETDATE()),
('e6', 'BYD', 2023, 'MPV', 160, 'LFP', 400, 130, 522, 2450, 7, 'BYD e6 - MPV điện', 0, 1, GETDATE())
-- Thêm dòng mới ở đây, copy format trên và sửa data
;

GO

-- ============================================
-- BATTERY MODELS - Template
-- ============================================
-- Copy dòng dưới và sửa thông tin:

INSERT INTO BatteryModels (Name, Brand, Chemistry, Voltage, CapacityKWh, Amperage, Weight, FormFactor, Cycles, Description, IsCustom, IsApproved, CreatedAt)
VALUES 
('CATL 100kWh', 'CATL', 'LFP', 400, 100, 250, 650, 'Prismatic', 4000, 'CATL 100kWh - Pin LFP dung lượng cao', 0, 1, GETDATE()),
('CATL 60kWh', 'CATL', 'LFP', 400, 60, 150, 390, 'Prismatic', 4000, 'CATL 60kWh - Pin LFP tiêu chuẩn', 0, 1, GETDATE()),
('CATL 80kWh', 'CATL', 'LFP', 400, 80, 200, 520, 'Prismatic', 4000, 'CATL 80kWh - Pin LFP trung bình', 0, 1, GETDATE()),
('Panasonic 2170', 'Panasonic', 'NCA', 375, 75, 200, 480, 'Cylindrical', 3000, 'Panasonic 2170 - Pin NCA cho Tesla', 0, 1, GETDATE()),
('Panasonic 4680', 'Panasonic', 'NCA', 400, 100, 350, 600, 'Cylindrical', 3000, 'Panasonic 4680 - Pin NCA thế hệ mới', 0, 1, GETDATE()),
('VinES 42kWh', 'VinES', 'LFP', 400, 42, 105, 280, 'Prismatic', 3500, 'VinES 42kWh - Pin cho VinFast', 0, 1, GETDATE()),
('VinES 90kWh', 'VinES', 'LFP', 400, 90, 225, 585, 'Prismatic', 3500, 'VinES 90kWh - Pin dung lượng cao', 0, 1, GETDATE()),
('BYD Blade', 'BYD', 'LFP', 400, 82, 205, 535, 'Prismatic', 5000, 'BYD Blade Battery - Pin LFP an toàn cao', 0, 1, GETDATE()),
('LG Chem 64kWh', 'LG Chem', 'NCM622', 375, 64, 170, 410, 'Prismatic', 2500, 'LG Chem 64kWh - Pin NCM', 0, 1, GETDATE()),
('Samsung SDI 75kWh', 'Samsung SDI', 'NCM811', 375, 75, 200, 485, 'Prismatic', 2500, 'Samsung SDI 75kWh - Pin NCM811', 0, 1, GETDATE())
-- Thêm dòng mới ở đây, copy format trên và sửa data
;

GO

PRINT 'Data imported successfully!';
PRINT 'Check results: SELECT * FROM VehicleModels; SELECT * FROM BatteryModels;';
GO

