using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVehicleManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AutoUpdateAllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ========== UPDATE ACCOUNTS TABLE ==========
            
            // EmailVerified column
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Accounts]') AND name = 'EmailVerified')
                BEGIN
                    ALTER TABLE [Accounts] ADD [EmailVerified] bit NOT NULL DEFAULT 0;
                END
            ");

            // GoogleId column (nullable)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Accounts]') AND name = 'GoogleId')
                BEGIN
                    ALTER TABLE [Accounts] ADD [GoogleId] nvarchar(max) NULL;
                END
            ");

            // LastLoginAt column (nullable)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Accounts]') AND name = 'LastLoginAt')
                BEGIN
                    ALTER TABLE [Accounts] ADD [LastLoginAt] datetime2 NULL;
                END
            ");

            // Phone column - change to nullable (nếu đang là NOT NULL)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Accounts]') AND name = 'Phone' AND is_nullable = 0)
                BEGIN
                    ALTER TABLE [Accounts] ALTER COLUMN [Phone] nvarchar(max) NULL;
                END
            ");

            // ========== UPDATE POSTS TABLE ==========
            
            // TransactionType column
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Posts]') AND name = 'TransactionType')
                BEGIN
                    ALTER TABLE [Posts] ADD [TransactionType] nvarchar(max) NOT NULL DEFAULT 'DIRECT';
                END
            ");

            // StaffId column (nullable)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Posts]') AND name = 'StaffId')
                BEGIN
                    ALTER TABLE [Posts] ADD [StaffId] int NULL;
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_StaffId')
                    BEGIN
                        CREATE INDEX [IX_Posts_StaffId] ON [Posts] ([StaffId]);
                    END
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Members')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Posts_Members_StaffId')
                        BEGIN
                            ALTER TABLE [Posts] ADD CONSTRAINT [FK_Posts_Members_StaffId] 
                                FOREIGN KEY ([StaffId]) REFERENCES [Members] ([MemberId]) ON DELETE NO ACTION;
                        END
                    END
                END
            ");

            // ContactInfo column (nullable)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Posts]') AND name = 'ContactInfo')
                BEGIN
                    ALTER TABLE [Posts] ADD [ContactInfo] nvarchar(max) NULL;
                END
            ");

            // ========== UPDATE VEHICLES TABLE ==========
            
            // VehicleModelId column (nullable foreign key)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Vehicles]') AND name = 'VehicleModelId')
                BEGIN
                    ALTER TABLE [Vehicles] ADD [VehicleModelId] int NULL;
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehicles_VehicleModelId')
                    BEGIN
                        CREATE INDEX [IX_Vehicles_VehicleModelId] ON [Vehicles] ([VehicleModelId]);
                    END
                END
            ");

            // ========== UPDATE BATTERIES TABLE ==========
            
            // BatteryModelId column (nullable foreign key)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Batteries]') AND name = 'BatteryModelId')
                BEGIN
                    ALTER TABLE [Batteries] ADD [BatteryModelId] int NULL;
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Batteries_BatteryModelId')
                    BEGIN
                        CREATE INDEX [IX_Batteries_BatteryModelId] ON [Batteries] ([BatteryModelId]);
                    END
                END
            ");

            // ========== CREATE NEW TABLES ==========
            
            // VehicleModels table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VehicleModels')
                BEGIN
                    CREATE TABLE [VehicleModels] (
                        [VehicleModelId] int NOT NULL IDENTITY,
                        [Name] nvarchar(max) NOT NULL DEFAULT '',
                        [Brand] nvarchar(max) NOT NULL DEFAULT '',
                        [Year] int NULL,
                        [Type] nvarchar(max) NOT NULL DEFAULT '',
                        [MotorPower] decimal(18,2) NULL,
                        [BatteryType] nvarchar(max) NOT NULL DEFAULT '',
                        [Voltage] decimal(18,2) NULL,
                        [MaxSpeed] int NULL,
                        [Range] int NULL,
                        [Weight] decimal(18,2) NULL,
                        [Seats] int NULL,
                        [Description] nvarchar(max) NOT NULL DEFAULT '',
                        [CustomSpec] nvarchar(max) NULL,
                        [IsCustom] bit NOT NULL DEFAULT 0,
                        [IsApproved] bit NOT NULL DEFAULT 0,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_VehicleModels] PRIMARY KEY ([VehicleModelId])
                    );
                END
            ");

            // BatteryModels table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BatteryModels')
                BEGIN
                    CREATE TABLE [BatteryModels] (
                        [BatteryModelId] int NOT NULL IDENTITY,
                        [Name] nvarchar(max) NOT NULL DEFAULT '',
                        [Brand] nvarchar(max) NOT NULL DEFAULT '',
                        [Chemistry] nvarchar(max) NOT NULL DEFAULT '',
                        [Voltage] decimal(18,2) NULL,
                        [CapacityKWh] decimal(18,2) NULL,
                        [Amperage] decimal(18,2) NULL,
                        [Weight] decimal(18,2) NULL,
                        [FormFactor] nvarchar(max) NOT NULL DEFAULT '',
                        [Cycles] int NULL,
                        [Description] nvarchar(max) NOT NULL DEFAULT '',
                        [CustomSpec] nvarchar(max) NULL,
                        [IsCustom] bit NOT NULL DEFAULT 0,
                        [IsApproved] bit NOT NULL DEFAULT 0,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_BatteryModels] PRIMARY KEY ([BatteryModelId])
                    );
                END
            ");

            // ExternalLogins table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExternalLogins')
                BEGIN
                    CREATE TABLE [ExternalLogins] (
                        [Id] int NOT NULL IDENTITY,
                        [AccountId] int NOT NULL,
                        [Provider] nvarchar(max) NOT NULL DEFAULT '',
                        [ProviderKey] nvarchar(max) NOT NULL DEFAULT '',
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT [PK_ExternalLogins] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ExternalLogins_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([AccountId]) ON DELETE CASCADE
                    );
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExternalLogins_AccountId')
                    BEGIN
                        CREATE INDEX [IX_ExternalLogins_AccountId] ON [ExternalLogins] ([AccountId]);
                    END
                END
            ");

            // OtpCodes table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OtpCodes')
                BEGIN
                    CREATE TABLE [OtpCodes] (
                        [Id] int NOT NULL IDENTITY,
                        [AccountId] int NULL,
                        [Email] nvarchar(max) NOT NULL DEFAULT '',
                        [Code] nvarchar(max) NOT NULL DEFAULT '',
                        [Purpose] nvarchar(max) NOT NULL DEFAULT '',
                        [ExpiresAt] datetime2 NOT NULL DEFAULT GETDATE(),
                        [ConsumedAt] datetime2 NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT [PK_OtpCodes] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_OtpCodes_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([AccountId]) ON DELETE SET NULL
                    );
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OtpCodes_AccountId')
                    BEGIN
                        CREATE INDEX [IX_OtpCodes_AccountId] ON [OtpCodes] ([AccountId]);
                    END
                END
            ");

            // ========== ADD FOREIGN KEYS (sau khi tạo bảng) ==========
            
            // FK Vehicles -> VehicleModels
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'VehicleModels')
                    AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Vehicles]') AND name = 'VehicleModelId')
                    AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Vehicles_VehicleModels_VehicleModelId')
                BEGIN
                    ALTER TABLE [Vehicles] ADD CONSTRAINT [FK_Vehicles_VehicleModels_VehicleModelId] 
                        FOREIGN KEY ([VehicleModelId]) REFERENCES [VehicleModels] ([VehicleModelId]) ON DELETE SET NULL;
                END
            ");

            // FK Batteries -> BatteryModels
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BatteryModels')
                    AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Batteries]') AND name = 'BatteryModelId')
                    AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Batteries_BatteryModels_BatteryModelId')
                BEGIN
                    ALTER TABLE [Batteries] ADD CONSTRAINT [FK_Batteries_BatteryModels_BatteryModelId] 
                        FOREIGN KEY ([BatteryModelId]) REFERENCES [BatteryModels] ([BatteryModelId]) ON DELETE SET NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ⚠️ Không nên rollback vì có thể mất dữ liệu
            // Chỉ rollback khi thực sự cần thiết
        }
    }
}
