using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceFieldsAndLedgerEntryCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "RejectedRows",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mvo",
                table: "LedgerEntries",
                type: "TEXT",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<int>(
                name: "SourceRowNumber",
                table: "LedgerEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceTitle",
                table: "LedgerEntries",
                type: "TEXT",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "LedgerEntries",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LedgerEntryCodes",
                columns: table => new
                {
                    LedgerEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerEntryCodes", x => new { x.LedgerEntryId, x.Code });
                    table.ForeignKey(
                        name: "FK_LedgerEntryCodes_LedgerEntries_LedgerEntryId",
                        column: x => x.LedgerEntryId,
                        principalTable: "LedgerEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntryCodes_Code",
                table: "LedgerEntryCodes",
                column: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LedgerEntryCodes");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "RejectedRows");

            migrationBuilder.DropColumn(
                name: "SourceRowNumber",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "SourceTitle",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "LedgerEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Mvo",
                table: "LedgerEntries",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 150,
                oldNullable: true);
        }
    }
}
