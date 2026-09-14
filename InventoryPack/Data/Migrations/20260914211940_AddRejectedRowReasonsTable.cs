using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectedRowReasonsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "RejectedRows");

            migrationBuilder.CreateTable(
                name: "RejectedRowReasons",
                columns: table => new
                {
                    RejectedRowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RejectedRowReasons", x => new { x.RejectedRowId, x.Reason });
                    table.ForeignKey(
                        name: "FK_RejectedRowReasons_RejectedRows_RejectedRowId",
                        column: x => x.RejectedRowId,
                        principalTable: "RejectedRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RejectedRowReasons_Reason",
                table: "RejectedRowReasons",
                column: "Reason");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RejectedRowReasons");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "RejectedRows",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
