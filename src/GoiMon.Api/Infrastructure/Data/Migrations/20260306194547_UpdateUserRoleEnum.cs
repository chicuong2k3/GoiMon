using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoiMon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRoleEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename 'Staff' → 'Cashier' to match the expanded UserRole enum.
            // 'Owner' string is unchanged — only its numeric value changed, not its name.
            migrationBuilder.Sql("""
                UPDATE "Users" SET "Role" = 'Cashier' WHERE "Role" = 'Staff';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert: 'Cashier' → 'Staff'
            migrationBuilder.Sql("""
                UPDATE "Users" SET "Role" = 'Staff' WHERE "Role" = 'Cashier';
                """);
        }
    }
}
