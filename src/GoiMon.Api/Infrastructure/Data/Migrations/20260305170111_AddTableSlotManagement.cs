using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoiMon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTableSlotManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TableSlotId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TableSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CurrentState = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Available"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSlots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableSlotId",
                table: "Orders",
                column: "TableSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_TableSlots_Code",
                table: "TableSlots",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_TableSlots_TableSlotId",
                table: "Orders",
                column: "TableSlotId",
                principalTable: "TableSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_TableSlots_TableSlotId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "TableSlots");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TableSlotId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TableSlotId",
                table: "Orders");
        }
    }
}
