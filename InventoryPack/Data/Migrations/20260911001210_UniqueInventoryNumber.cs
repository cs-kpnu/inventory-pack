using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class UniqueInventoryNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets",
                column: "InventoryNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets",
                column: "InventoryNumber");
        }
    }
}
