using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedPackageIdToPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "selected_package_id",
                table: "Posts",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "selected_package_id",
                table: "Posts");
        }
    }
}
