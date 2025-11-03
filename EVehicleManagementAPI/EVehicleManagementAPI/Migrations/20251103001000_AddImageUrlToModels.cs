using Microsoft.EntityFrameworkCore.Migrations;

namespace EVehicleManagementAPI.Migrations
{
    public partial class AddImageUrlToModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "VehicleModels",
                type: "nvarchar(512)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "BatteryModels",
                type: "nvarchar(512)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "BatteryModels");
        }
    }
}


