using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SD.Mercato.ProductCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultCommissionRateToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DefaultCommissionRate",
                schema: "productcatalog",
                table: "Categories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductQuestions",
                schema: "productcatalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AskedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AskedByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductQuestions_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "productcatalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductAnswers",
                schema: "productcatalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnsweredByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AnsweredByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AnsweredByRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAnswers_ProductQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "productcatalog",
                        principalTable: "ProductQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAnswers_AnsweredByUserId",
                schema: "productcatalog",
                table: "ProductAnswers",
                column: "AnsweredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAnswers_QuestionId",
                schema: "productcatalog",
                table: "ProductAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_AskedByUserId",
                schema: "productcatalog",
                table: "ProductQuestions",
                column: "AskedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_ProductId",
                schema: "productcatalog",
                table: "ProductQuestions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_Status",
                schema: "productcatalog",
                table: "ProductQuestions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAnswers",
                schema: "productcatalog");

            migrationBuilder.DropTable(
                name: "ProductQuestions",
                schema: "productcatalog");

            migrationBuilder.DropColumn(
                name: "DefaultCommissionRate",
                schema: "productcatalog",
                table: "Categories");
        }
    }
}
