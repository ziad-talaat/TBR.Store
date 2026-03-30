using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBL.EF.Migrations
{
    /// <inheritdoc />
    public partial class addingclickedcounttoproduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClickedCount",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClickedCount",
                table: "Product");
        }
    }
}
