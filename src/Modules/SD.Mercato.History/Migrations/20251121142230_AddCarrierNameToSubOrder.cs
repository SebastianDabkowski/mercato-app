using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SD.Mercato.History.Migrations
{
    /// <inheritdoc />
    public partial class AddCarrierNameToSubOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarrierName",
                table: "SubOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarrierName",
                table: "SubOrders");
        }
    }
}
