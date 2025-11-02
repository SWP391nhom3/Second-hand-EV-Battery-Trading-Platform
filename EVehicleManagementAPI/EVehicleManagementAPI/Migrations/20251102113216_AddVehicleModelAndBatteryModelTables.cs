using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVehicleManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleModelAndBatteryModelTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Tạo bảng VehicleModels (chỉ tạo nếu chưa tồn tại)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VehicleModels')
                BEGIN
                    CREATE TABLE [VehicleModels] (
                        [VehicleModelId] int NOT NULL IDENTITY,
                        [Name] nvarchar(max) NOT NULL,
                        [Brand] nvarchar(max) NOT NULL,
                        [Year] int NULL,
                        [Type] nvarchar(max) NOT NULL,
                        [MotorPower] decimal(18,2) NULL,
                        [Voltage] decimal(18,2) NULL,
                        [Range] int NULL,
                        [Weight] decimal(18,2) NULL,
                        [BatteryCapacity] decimal(18,2) NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [IsCustom] bit NOT NULL,
                        [IsApproved] bit NOT NULL,
                        [CustomSpec] nvarchar(max) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_VehicleModels] PRIMARY KEY ([VehicleModelId])
                    );
                END
            ");

            // ✅ Tạo bảng BatteryModels (chỉ tạo nếu chưa tồn tại)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BatteryModels')
                BEGIN
                    CREATE TABLE [BatteryModels] (
                        [BatteryModelId] int NOT NULL IDENTITY,
                        [Name] nvarchar(max) NOT NULL,
                        [Brand] nvarchar(max) NOT NULL,
                        [Chemistry] nvarchar(max) NOT NULL,
                        [Voltage] decimal(18,2) NULL,
                        [CapacityKWh] decimal(18,2) NULL,
                        [Amperage] decimal(18,2) NULL,
                        [FormFactor] nvarchar(max) NOT NULL,
                        [Weight] decimal(18,2) NULL,
                        [Cycles] int NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [IsCustom] bit NOT NULL,
                        [IsApproved] bit NOT NULL,
                        [CustomSpec] nvarchar(max) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_BatteryModels] PRIMARY KEY ([BatteryModelId])
                    );
                END
            ");

            // ✅ Thêm cột VehicleModelId vào bảng Vehicles (nullable, chỉ thêm nếu chưa có)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Vehicles]') AND name = 'VehicleModelId')
                BEGIN
                    ALTER TABLE [Vehicles] ADD [VehicleModelId] int NULL;
                    CREATE INDEX [IX_Vehicles_VehicleModelId] ON [Vehicles] ([VehicleModelId]);
                    ALTER TABLE [Vehicles] ADD CONSTRAINT [FK_Vehicles_VehicleModels_VehicleModelId] 
                        FOREIGN KEY ([VehicleModelId]) REFERENCES [VehicleModels] ([VehicleModelId]) ON DELETE SET NULL;
                END
            ");

            // ✅ Thêm cột BatteryModelId vào bảng Batteries (nullable, chỉ thêm nếu chưa có)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Batteries]') AND name = 'BatteryModelId')
                BEGIN
                    ALTER TABLE [Batteries] ADD [BatteryModelId] int NULL;
                    CREATE INDEX [IX_Batteries_BatteryModelId] ON [Batteries] ([BatteryModelId]);
                    ALTER TABLE [Batteries] ADD CONSTRAINT [FK_Batteries_BatteryModels_BatteryModelId] 
                        FOREIGN KEY ([BatteryModelId]) REFERENCES [BatteryModels] ([BatteryModelId]) ON DELETE SET NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ Rollback: Xóa foreign key và cột (chỉ xóa nếu tồn tại)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Batteries]') AND name = 'BatteryModelId')
                BEGIN
                    ALTER TABLE [Batteries] DROP CONSTRAINT IF EXISTS [FK_Batteries_BatteryModels_BatteryModelId];
                    DROP INDEX IF EXISTS [IX_Batteries_BatteryModelId] ON [Batteries];
                    ALTER TABLE [Batteries] DROP COLUMN [BatteryModelId];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Vehicles]') AND name = 'VehicleModelId')
                BEGIN
                    ALTER TABLE [Vehicles] DROP CONSTRAINT IF EXISTS [FK_Vehicles_VehicleModels_VehicleModelId];
                    DROP INDEX IF EXISTS [IX_Vehicles_VehicleModelId] ON [Vehicles];
                    ALTER TABLE [Vehicles] DROP COLUMN [VehicleModelId];
                END
            ");

            // ✅ Xóa bảng (cẩn thận - chỉ trong Down migration)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BatteryModels')
                BEGIN
                    DROP TABLE [BatteryModels];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'VehicleModels')
                BEGIN
                    DROP TABLE [VehicleModels];
                END
            ");
        }
    }
}
