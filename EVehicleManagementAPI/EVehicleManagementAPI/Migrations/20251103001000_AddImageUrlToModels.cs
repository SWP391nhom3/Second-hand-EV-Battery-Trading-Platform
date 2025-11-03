using Microsoft.EntityFrameworkCore.Migrations;

namespace EVehicleManagementAPI.Migrations
{
    public partial class AddImageUrlToModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // VehicleModels.ImageUrl
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'ImageUrl' AND Object_ID = Object_ID(N'dbo.VehicleModels')
)
BEGIN
    ALTER TABLE dbo.VehicleModels ADD ImageUrl NVARCHAR(512) NULL;
END
");

            // BatteryModels.ImageUrl
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'ImageUrl' AND Object_ID = Object_ID(N'dbo.BatteryModels')
)
BEGIN
    ALTER TABLE dbo.BatteryModels ADD ImageUrl NVARCHAR(512) NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa cột nếu tồn tại (an toàn khi rollback)
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'ImageUrl' AND Object_ID = Object_ID(N'dbo.VehicleModels')
)
BEGIN
    ALTER TABLE dbo.VehicleModels DROP COLUMN ImageUrl;
END
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'ImageUrl' AND Object_ID = Object_ID(N'dbo.BatteryModels')
)
BEGIN
    ALTER TABLE dbo.BatteryModels DROP COLUMN ImageUrl;
END
");
        }
    }
}


