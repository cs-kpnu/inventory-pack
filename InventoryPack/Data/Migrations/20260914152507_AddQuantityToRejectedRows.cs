using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityToRejectedRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "RejectedRows",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "RejectedRows");
        }
    }
}
