using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoiMon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComboOrderAndVariantReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "ProductComboItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ComboId",
                table: "OrderItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComboName",
                table: "OrderItems",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductComboItems_VariantId",
                table: "ProductComboItems",
                column: "VariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductComboItems_VariantId",
                table: "ProductComboItems");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "ProductComboItems");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ComboName",
                table: "OrderItems");
        }
    }
}
