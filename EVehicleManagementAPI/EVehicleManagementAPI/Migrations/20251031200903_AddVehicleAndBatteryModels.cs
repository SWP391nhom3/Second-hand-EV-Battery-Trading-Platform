using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVehicleManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleAndBatteryModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VehicleModelId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StaffId",
                table: "Posts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BatteryModelId",
                table: "Batteries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Accounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BatteryModels",
                columns: table => new
                {
                    BatteryModelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Chemistry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Voltage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CapacityKWh = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Amperage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FormFactor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cycles = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomSpec = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatteryModels", x => x.BatteryModelId);
                });

            migrationBuilder.CreateTable(
                name: "ExternalLogins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalLogins_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtpCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpCodes_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VehicleModels",
                columns: table => new
                {
                    VehicleModelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotorPower = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BatteryType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Voltage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxSpeed = table.Column<int>(type: "int", nullable: true),
                    Range = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Seats = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomSpec = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleModels", x => x.VehicleModelId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_VehicleModelId",
                table: "Vehicles",
                column: "VehicleModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_StaffId",
                table: "Posts",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Batteries_BatteryModelId",
                table: "Batteries",
                column: "BatteryModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_AccountId",
                table: "ExternalLogins",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_Provider_ProviderKey",
                table: "ExternalLogins",
                columns: new[] { "Provider", "ProviderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_AccountId",
                table: "OtpCodes",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_Email_Purpose",
                table: "OtpCodes",
                columns: new[] { "Email", "Purpose" });

            migrationBuilder.AddForeignKey(
                name: "FK_Batteries_BatteryModels_BatteryModelId",
                table: "Batteries",
                column: "BatteryModelId",
                principalTable: "BatteryModels",
                principalColumn: "BatteryModelId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Members_StaffId",
                table: "Posts",
                column: "StaffId",
                principalTable: "Members",
                principalColumn: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleModels_VehicleModelId",
                table: "Vehicles",
                column: "VehicleModelId",
                principalTable: "VehicleModels",
                principalColumn: "VehicleModelId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batteries_BatteryModels_BatteryModelId",
                table: "Batteries");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Members_StaffId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleModels_VehicleModelId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "BatteryModels");

            migrationBuilder.DropTable(
                name: "ExternalLogins");

            migrationBuilder.DropTable(
                name: "OtpCodes");

            migrationBuilder.DropTable(
                name: "VehicleModels");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_VehicleModelId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Posts_StaffId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Batteries_BatteryModelId",
                table: "Batteries");

            migrationBuilder.DropColumn(
                name: "VehicleModelId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ContactInfo",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "BatteryModelId",
                table: "Batteries");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Accounts");
        }
    }
}
