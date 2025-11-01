using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EVehicleManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Accounts_AccountId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_PostPackageSubs_Payments_PaymentId",
                table: "PostPackageSubs");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceFees_ConstructFees_ConstructFeeId",
                table: "ServiceFees");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name", "Status" },
                values: new object[,]
                {
                    { 1, "Admin", "ACTIVE" },
                    { 2, "Staff", "ACTIVE" },
                    { 3, "Member", "ACTIVE" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Accounts_AccountId",
                table: "Members",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PostPackageSubs_Payments_PaymentId",
                table: "PostPackageSubs",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceFees_ConstructFees_ConstructFeeId",
                table: "ServiceFees",
                column: "ConstructFeeId",
                principalTable: "ConstructFees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Accounts_AccountId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_PostPackageSubs_Payments_PaymentId",
                table: "PostPackageSubs");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceFees_ConstructFees_ConstructFeeId",
                table: "ServiceFees");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Accounts_AccountId",
                table: "Members",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostPackageSubs_Payments_PaymentId",
                table: "PostPackageSubs",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceFees_ConstructFees_ConstructFeeId",
                table: "ServiceFees",
                column: "ConstructFeeId",
                principalTable: "ConstructFees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
