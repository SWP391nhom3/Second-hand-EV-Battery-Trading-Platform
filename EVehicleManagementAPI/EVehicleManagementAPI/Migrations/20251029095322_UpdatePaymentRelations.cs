using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVehicleManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "IX_Payments_MemberId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Payments");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Members_BuyerId",
                table: "Payments",
                column: "BuyerId",
                principalTable: "Members",
                principalColumn: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Members_SellerId",
                table: "Payments",
                column: "SellerId",
                principalTable: "Members",
                principalColumn: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Members_BuyerId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Members_SellerId",
                table: "Payments");

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MemberId",
                table: "Payments",
                column: "MemberId");

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
    }
}
