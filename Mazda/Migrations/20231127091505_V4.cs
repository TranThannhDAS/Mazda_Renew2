using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mazda.Migrations
{
    /// <inheritdoc />
    public partial class V4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description_Short",
                table: "Blog");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Products",
                newName: "UrlYoutube");

            migrationBuilder.RenameColumn(
                name: "Description_Short",
                table: "Guides",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "ntext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlShoppe",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Guides",
                type: "ntext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Blog",
                type: "ntext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Create_Date",
                table: "Blog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Blog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlShoppe",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Create_Date",
                table: "Blog");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Blog");

            migrationBuilder.RenameColumn(
                name: "UrlYoutube",
                table: "Products",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Guides",
                newName: "Description_Short");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "ntext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Guides",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "ntext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Blog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "ntext",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description_Short",
                table: "Blog",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
