using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLedgerEntryQuantityToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "LedgerEntries",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "LedgerEntries",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 4);
        }
    }
}
