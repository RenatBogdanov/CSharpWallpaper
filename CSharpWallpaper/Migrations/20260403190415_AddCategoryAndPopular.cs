using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSharpWallpaper.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndPopular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPopular",
                table: "Wallpapers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPopular",
                table: "Wallpapers");
        }
    }
}
