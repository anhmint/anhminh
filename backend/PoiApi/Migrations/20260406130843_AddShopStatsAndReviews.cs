using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddShopStatsAndReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ListenCount column already added in DB
            
            // ViewCount column already added in DB


            // Orders table already exists in DB so we skip creating it

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShopId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CustomerName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // IX_Orders_ShopId index already exists

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ShopId",
                table: "Reviews",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // DropTable("Orders") skipped

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropColumn(
                name: "ListenCount",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Shops");
        }
    }
}
