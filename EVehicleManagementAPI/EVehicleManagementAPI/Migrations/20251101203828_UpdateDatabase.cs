using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVehicleManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_Posts_StaffId",
                table: "Posts",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Members_StaffId",
                table: "Posts",
                column: "StaffId",
                principalTable: "Members",
                principalColumn: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Members_StaffId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_StaffId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ContactInfo",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "Posts");
        }
    }
}
