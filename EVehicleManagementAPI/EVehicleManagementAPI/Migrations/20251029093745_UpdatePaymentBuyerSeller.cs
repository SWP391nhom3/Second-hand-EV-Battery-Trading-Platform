using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVehicleManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentBuyerSeller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Members_MemberId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BuyerId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BuyerId",
                table: "Payments",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SellerId",
                table: "Payments",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Members_BuyerId",
                table: "Payments",
                column: "BuyerId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Members_MemberId",
                table: "Payments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Members_SellerId",
                table: "Payments",
                column: "SellerId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Members_BuyerId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Members_MemberId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Members_SellerId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BuyerId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SellerId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Members_MemberId",
                table: "Payments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
