using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "email_verified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "email_verified_at",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Email_Verification_Otps",
                columns: table => new
                {
                    otp_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    otp_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_used = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    attempt_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Email_Verification_Otps", x => x.otp_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Email_Verification_Otps_email",
                table: "Email_Verification_Otps",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_Email_Verification_Otps_email_is_used_expires_at",
                table: "Email_Verification_Otps",
                columns: new[] { "email", "is_used", "expires_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Email_Verification_Otps");

            migrationBuilder.DropColumn(
                name: "email_verified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "email_verified_at",
                table: "Users");
        }
    }
}
