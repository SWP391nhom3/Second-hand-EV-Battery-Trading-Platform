-- Script Import VehicleModels và BatteryModels từ CSV
-- Hướng dẫn: Sửa đường dẫn file CSV và chạy script này

USE EVehicleDB;
GO

-- ============================================
-- OPTION 1: Import VehicleModels từ CSV
-- ============================================
-- Bước 1: Tạo bảng tạm
IF OBJECT_ID('tempdb..#VehicleModelsTemp') IS NOT NULL
    DROP TABLE #VehicleModelsTemp;

CREATE TABLE #VehicleModelsTemp (
    Name NVARCHAR(MAX),
    Brand NVARCHAR(MAX),
    Year INT,
    Type NVARCHAR(MAX),
    MotorPower DECIMAL(18,2),
    BatteryType NVARCHAR(MAX),
    Voltage DECIMAL(18,2),
    MaxSpeed INT,
    Range INT,
    Weight DECIMAL(18,2),
    Seats INT,
    Description NVARCHAR(MAX)
);

-- Bước 2: Import CSV vào bảng tạm
-- Thay đổi đường dẫn file CSV của bạn
BULK INSERT #VehicleModelsTemp
FROM '/path/to/VehicleModels.csv'  -- ⚠️ THAY ĐỔI ĐƯỜNG DẪN
WITH (
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,  -- Bỏ qua header row
    CODEPAGE = '65001'  -- UTF-8
);

-- Bước 3: Insert vào VehicleModels (bỏ qua duplicate)
INSERT INTO VehicleModels (Name, Brand, Year, Type, MotorPower, BatteryType, Voltage, MaxSpeed, Range, Weight, Seats, Description, IsCustom, IsApproved, CreatedAt)
SELECT 
    Name,
    Brand,
    Year,
    Type,
    MotorPower,
    BatteryType,
    Voltage,
    MaxSpeed,
    Range,
    Weight,
    Seats,
    Description,
    0 AS IsCustom,      -- Model chuẩn
    1 AS IsApproved,    -- Đã approved
    GETDATE() AS CreatedAt
FROM #VehicleModelsTemp t
WHERE NOT EXISTS (
    SELECT 1 FROM VehicleModels vm 
    WHERE LOWER(vm.Brand) = LOWER(t.Brand) 
    AND LOWER(vm.Name) = LOWER(t.Name)
    AND vm.Year = t.Year
    AND LOWER(vm.Type) = LOWER(t.Type)
);

PRINT 'VehicleModels imported: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
DROP TABLE #VehicleModelsTemp;
GO

-- ============================================
-- OPTION 2: Import BatteryModels từ CSV
-- ============================================
IF OBJECT_ID('tempdb..#BatteryModelsTemp') IS NOT NULL
    DROP TABLE #BatteryModelsTemp;

CREATE TABLE #BatteryModelsTemp (
    Name NVARCHAR(MAX),
    Brand NVARCHAR(MAX),
    Chemistry NVARCHAR(MAX),
    Voltage DECIMAL(18,2),
    CapacityKWh DECIMAL(18,2),
    Amperage DECIMAL(18,2),
    Weight DECIMAL(18,2),
    FormFactor NVARCHAR(MAX),
    Cycles INT,
    Description NVARCHAR(MAX)
);

-- Thay đổi đường dẫn file CSV của bạn
BULK INSERT #BatteryModelsTemp
FROM '/path/to/BatteryModels.csv'  -- ⚠️ THAY ĐỔI ĐƯỜNG DẪN
WITH (
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2,
    CODEPAGE = '65001'
);

INSERT INTO BatteryModels (Name, Brand, Chemistry, Voltage, CapacityKWh, Amperage, Weight, FormFactor, Cycles, Description, IsCustom, IsApproved, CreatedAt)
SELECT 
    Name,
    Brand,
    Chemistry,
    Voltage,
    CapacityKWh,
    Amperage,
    Weight,
    FormFactor,
    Cycles,
    Description,
    0 AS IsCustom,
    1 AS IsApproved,
    GETDATE() AS CreatedAt
FROM #BatteryModelsTemp t
WHERE NOT EXISTS (
    SELECT 1 FROM BatteryModels bm 
    WHERE LOWER(bm.Brand) = LOWER(t.Brand) 
    AND LOWER(bm.Name) = LOWER(t.Name)
    AND LOWER(bm.Chemistry) = LOWER(t.Chemistry)
    AND bm.CapacityKWh = t.CapacityKWh
);

PRINT 'BatteryModels imported: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
DROP TABLE #BatteryModelsTemp;
GO

PRINT 'Import completed!';
GO

