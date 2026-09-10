using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InventoryNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Mvo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Subaccount = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    PrintedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RejectedRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    RawText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Subaccount = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Mvo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RejectedRows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_InventoryNumber",
                table: "Assets",
                column: "InventoryNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Mvo_Subaccount",
                table: "Assets",
                columns: new[] { "Mvo", "Subaccount" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_PrintedAt",
                table: "Assets",
                column: "PrintedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RejectedRows_RowNumber",
                table: "RejectedRows",
                column: "RowNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "RejectedRows");
        }
    }
}
