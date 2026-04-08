using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddShopCategoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Shops",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_CategoryId",
                table: "Shops",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_Categories_CategoryId",
                table: "Shops",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shops_Categories_CategoryId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_CategoryId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Shops");
        }
    }
}
