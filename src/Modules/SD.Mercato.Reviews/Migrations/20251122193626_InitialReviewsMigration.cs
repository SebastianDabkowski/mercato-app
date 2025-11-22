using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SD.Mercato.Reviews.Migrations
{
    /// <inheritdoc />
    public partial class InitialReviewsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reviews");

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                schema: "reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubOrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BuyerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Approved"),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModeratedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ModerationNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                schema: "reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BuyerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Approved"),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModeratedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ModerationNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_BuyerUserId",
                schema: "reviews",
                table: "ProductReviews",
                column: "BuyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_IsVisible",
                schema: "reviews",
                table: "ProductReviews",
                column: "IsVisible");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId",
                schema: "reviews",
                table: "ProductReviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_Status",
                schema: "reviews",
                table: "ProductReviews",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_StoreId",
                schema: "reviews",
                table: "ProductReviews",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_SubOrderItemId_BuyerUserId",
                schema: "reviews",
                table: "ProductReviews",
                columns: new[] { "SubOrderItemId", "BuyerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BuyerUserId",
                schema: "reviews",
                table: "Reviews",
                column: "BuyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_IsVisible",
                schema: "reviews",
                table: "Reviews",
                column: "IsVisible");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Status",
                schema: "reviews",
                table: "Reviews",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_StoreId",
                schema: "reviews",
                table: "Reviews",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_SubOrderId_BuyerUserId",
                schema: "reviews",
                table: "Reviews",
                columns: new[] { "SubOrderId", "BuyerUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductReviews",
                schema: "reviews");

            migrationBuilder.DropTable(
                name: "Reviews",
                schema: "reviews");
        }
    }
}
