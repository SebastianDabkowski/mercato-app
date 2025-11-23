using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SD.Mercato.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddGdprFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailMarketingConsent",
                schema: "users",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailMarketingConsentUpdatedAt",
                schema: "users",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "users",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "users",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailMarketingConsent",
                schema: "users",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailMarketingConsentUpdatedAt",
                schema: "users",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "users",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "users",
                table: "AspNetUsers");
        }
    }
}
