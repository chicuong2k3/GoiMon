using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoiMon.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductComboItems_ProductCombos_ProductComboId",
                table: "ProductComboItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductComboItems_ProductComboId",
                table: "ProductComboItems");

            migrationBuilder.DropColumn(
                name: "ProductComboId",
                table: "ProductComboItems");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductCombos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComboItems_ComboId",
                table: "ProductComboItems",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComboItems_ProductCombos_ComboId",
                table: "ProductComboItems",
                column: "ComboId",
                principalTable: "ProductCombos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductComboItems_ProductCombos_ComboId",
                table: "ProductComboItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductComboItems_ComboId",
                table: "ProductComboItems");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductCombos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductComboId",
                table: "ProductComboItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_ProductComboItems_ProductComboId",
                table: "ProductComboItems",
                column: "ProductComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComboItems_ProductCombos_ProductComboId",
                table: "ProductComboItems",
                column: "ProductComboId",
                principalTable: "ProductCombos",
                principalColumn: "Id");
        }
    }
}
