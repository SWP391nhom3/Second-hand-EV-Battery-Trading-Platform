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
                    CREATE INDEX [IX_Posts_StaffId] ON [Posts] ([StaffId]);
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Members')
                    BEGIN
                        ALTER TABLE [Posts] ADD CONSTRAINT [FK_Posts_Members_StaffId] 
                            FOREIGN KEY ([StaffId]) REFERENCES [Members] ([MemberId]) ON DELETE NO ACTION;
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

            // ========== CREATE NEW TABLES ==========
            
            // VehicleModels table
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
                        [ExternalLoginId] int NOT NULL IDENTITY,
                        [AccountId] int NULL,
                        [Provider] nvarchar(max) NOT NULL,
                        [ProviderKey] nvarchar(max) NOT NULL,
                        [ProviderDisplayName] nvarchar(max) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_ExternalLogins] PRIMARY KEY ([ExternalLoginId]),
                        CONSTRAINT [FK_ExternalLogins_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([AccountId]) ON DELETE SET NULL
                    );
                    CREATE INDEX [IX_ExternalLogins_AccountId] ON [ExternalLogins] ([AccountId]);
                END
            ");

            // OtpCodes table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OtpCodes')
                BEGIN
                    CREATE TABLE [OtpCodes] (
                        [OtpCodeId] int NOT NULL IDENTITY,
                        [AccountId] int NULL,
                        [Email] nvarchar(max) NOT NULL,
                        [Code] nvarchar(max) NOT NULL,
                        [Purpose] nvarchar(max) NOT NULL,
                        [ExpiresAt] datetime2 NOT NULL,
                        [IsUsed] bit NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_OtpCodes] PRIMARY KEY ([OtpCodeId]),
                        CONSTRAINT [FK_OtpCodes_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([AccountId]) ON DELETE SET NULL
                    );
                    CREATE INDEX [IX_OtpCodes_AccountId] ON [OtpCodes] ([AccountId]);
                END
            ");

            // ========== UPDATE VEHICLES TABLE ==========
            
            // VehicleModelId column (nullable foreign key)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Vehicles]') AND name = 'VehicleModelId')
                BEGIN
                    ALTER TABLE [Vehicles] ADD [VehicleModelId] int NULL;
                    CREATE INDEX [IX_Vehicles_VehicleModelId] ON [Vehicles] ([VehicleModelId]);
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'VehicleModels')
                    BEGIN
                        ALTER TABLE [Vehicles] ADD CONSTRAINT [FK_Vehicles_VehicleModels_VehicleModelId] 
                            FOREIGN KEY ([VehicleModelId]) REFERENCES [VehicleModels] ([VehicleModelId]) ON DELETE SET NULL;
                    END
                END
            ");

            // ========== UPDATE BATTERIES TABLE ==========
            
            // BatteryModelId column (nullable foreign key)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Batteries]') AND name = 'BatteryModelId')
                BEGIN
                    ALTER TABLE [Batteries] ADD [BatteryModelId] int NULL;
                    CREATE INDEX [IX_Batteries_BatteryModelId] ON [Batteries] ([BatteryModelId]);
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BatteryModels')
                    BEGIN
                        ALTER TABLE [Batteries] ADD CONSTRAINT [FK_Batteries_BatteryModels_BatteryModelId] 
                            FOREIGN KEY ([BatteryModelId]) REFERENCES [BatteryModels] ([BatteryModelId]) ON DELETE SET NULL;
                    END
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

