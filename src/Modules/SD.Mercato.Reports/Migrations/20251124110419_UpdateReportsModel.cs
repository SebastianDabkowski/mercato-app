using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SD.Mercato.Reports.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReportsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SellerInvoices_StoreId_PeriodStartDate_PeriodEndDate",
                table: "SellerInvoices");

            migrationBuilder.CreateIndex(
                name: "IX_SellerInvoices_StoreId_PeriodStartDate_PeriodEndDate",
                table: "SellerInvoices",
                columns: new[] { "StoreId", "PeriodStartDate", "PeriodEndDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SellerInvoices_StoreId_PeriodStartDate_PeriodEndDate",
                table: "SellerInvoices");

            migrationBuilder.CreateIndex(
                name: "IX_SellerInvoices_StoreId_PeriodStartDate_PeriodEndDate",
                table: "SellerInvoices",
                columns: new[] { "StoreId", "PeriodStartDate", "PeriodEndDate" });
        }
    }
}
