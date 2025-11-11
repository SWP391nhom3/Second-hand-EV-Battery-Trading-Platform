using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovedByRejectedByToPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "approved_by",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "rejected_by",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approved_by",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "rejected_by",
                table: "Posts");
        }
    }
}
