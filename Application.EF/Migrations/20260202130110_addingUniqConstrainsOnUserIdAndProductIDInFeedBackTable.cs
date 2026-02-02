using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBL.EF.Migrations
{
    /// <inheritdoc />
    public partial class addingUniqConstrainsOnUserIdAndProductIDInFeedBackTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeedBacks_UserId",
                table: "FeedBacks");

            migrationBuilder.CreateIndex(
                name: "IX_FeedBacks_UserId_ProductId",
                table: "FeedBacks",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeedBacks_UserId_ProductId",
                table: "FeedBacks");

            migrationBuilder.CreateIndex(
                name: "IX_FeedBacks_UserId",
                table: "FeedBacks",
                column: "UserId");
        }
    }
}
