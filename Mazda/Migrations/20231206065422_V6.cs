using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mazda.Migrations
{
    /// <inheritdoc />
    public partial class V6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UrlYoutube",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UrlYoutube",
                table: "Guides");

            migrationBuilder.DropColumn(
                name: "UrlYoutube",
                table: "Blog");

            migrationBuilder.AddColumn<double>(
                name: "Price_After",
                table: "Products",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price_After",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "Discount",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlYoutube",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlYoutube",
                table: "Guides",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlYoutube",
                table: "Blog",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
