using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBL.EF.Migrations
{
    /// <inheritdoc />
    public partial class addImageUrlToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PRice100",
                table: "Product",
                newName: "Price100");

            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "Product",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Product");

            migrationBuilder.RenameColumn(
                name: "Price100",
                table: "Product",
                newName: "PRice100");
        }
    }
}
