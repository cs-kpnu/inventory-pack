using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityAndLedgerEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_Mvo_Subaccount",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Mvo",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Subaccount",
                table: "Assets");

            migrationBuilder.AddColumn<Guid>(
                name: "LedgerEntryId",
                table: "Assets",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "UnitIndex",
                table: "Assets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Subaccount = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Mvo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets",
                column: "InventoryNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_InventoryNumber_UnitIndex",
                table: "Assets",
                columns: new[] { "InventoryNumber", "UnitIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_LedgerEntryId",
                table: "Assets",
                column: "LedgerEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_Mvo_Subaccount",
                table: "LedgerEntries",
                columns: new[] { "Mvo", "Subaccount" });

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_LedgerEntries_LedgerEntryId",
                table: "Assets",
                column: "LedgerEntryId",
                principalTable: "LedgerEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_LedgerEntries_LedgerEntryId",
                table: "Assets");

            migrationBuilder.DropTable(
                name: "LedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_InventoryNumber_UnitIndex",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_LedgerEntryId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "LedgerEntryId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "UnitIndex",
                table: "Assets");

            migrationBuilder.AddColumn<string>(
                name: "Mvo",
                table: "Assets",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Assets",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subaccount",
                table: "Assets",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets",
                column: "InventoryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Mvo_Subaccount",
                table: "Assets",
                columns: new[] { "Mvo", "Subaccount" });
        }
    }
}
