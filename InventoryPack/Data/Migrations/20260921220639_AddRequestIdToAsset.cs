using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestIdToAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "Assets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_RequestId",
                table: "Assets",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_RequestId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "Assets");
        }
    }
}
