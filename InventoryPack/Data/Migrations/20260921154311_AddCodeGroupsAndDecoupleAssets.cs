using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeGroupsAndDecoupleAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_LedgerEntries_LedgerEntryId",
                table: "Assets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LedgerEntryCodes",
                table: "LedgerEntryCodes");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntryCodes_Code",
                table: "LedgerEntryCodes");

            migrationBuilder.DropIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_InventoryNumber_UnitIndex",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "LedgerEntryCodes");

            migrationBuilder.DropColumn(
                name: "InventoryNumber",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "UnitIndex",
                table: "Assets");

            migrationBuilder.RenameColumn(
                name: "LedgerEntryId",
                table: "Assets",
                newName: "CodeGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Assets_LedgerEntryId",
                table: "Assets",
                newName: "IX_Assets_CodeGroupId");

            migrationBuilder.AddColumn<Guid>(
                name: "CodeGroupId",
                table: "LedgerEntryCodes",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "AllocatedAt",
                table: "Assets",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_LedgerEntryCodes",
                table: "LedgerEntryCodes",
                columns: new[] { "LedgerEntryId", "CodeGroupId" });

            migrationBuilder.CreateTable(
                name: "CodeGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntryCodes_CodeGroupId",
                table: "LedgerEntryCodes",
                column: "CodeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeGroups_Code",
                table: "CodeGroups",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_CodeGroups_CodeGroupId",
                table: "Assets",
                column: "CodeGroupId",
                principalTable: "CodeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntryCodes_CodeGroups_CodeGroupId",
                table: "LedgerEntryCodes",
                column: "CodeGroupId",
                principalTable: "CodeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_CodeGroups_CodeGroupId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntryCodes_CodeGroups_CodeGroupId",
                table: "LedgerEntryCodes");

            migrationBuilder.DropTable(
                name: "CodeGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LedgerEntryCodes",
                table: "LedgerEntryCodes");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntryCodes_CodeGroupId",
                table: "LedgerEntryCodes");

            migrationBuilder.DropColumn(
                name: "CodeGroupId",
                table: "LedgerEntryCodes");

            migrationBuilder.DropColumn(
                name: "AllocatedAt",
                table: "Assets");

            migrationBuilder.RenameColumn(
                name: "CodeGroupId",
                table: "Assets",
                newName: "LedgerEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_Assets_CodeGroupId",
                table: "Assets",
                newName: "IX_Assets_LedgerEntryId");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "LedgerEntryCodes",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InventoryNumber",
                table: "Assets",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UnitIndex",
                table: "Assets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LedgerEntryCodes",
                table: "LedgerEntryCodes",
                columns: new[] { "LedgerEntryId", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntryCodes_Code",
                table: "LedgerEntryCodes",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets",
                column: "InventoryNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_InventoryNumber_UnitIndex",
                table: "Assets",
                columns: new[] { "InventoryNumber", "UnitIndex" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_LedgerEntries_LedgerEntryId",
                table: "Assets",
                column: "LedgerEntryId",
                principalTable: "LedgerEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
