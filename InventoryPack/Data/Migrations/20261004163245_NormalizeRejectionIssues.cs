using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPack.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeRejectionIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old reasons cannot reconstruct shared issues. Require fresh diagnostics instead of silently losing them.
            migrationBuilder.Sql("""
                CREATE TEMP TABLE "RejectionRefactorGuard" (
                    "RowCount" INTEGER CONSTRAINT "Rejection_refactor_requires_empty_rejected_rows" CHECK ("RowCount" = 0)
                );
                INSERT INTO "RejectionRefactorGuard" SELECT COUNT(*) FROM "RejectedRows";
                DROP TABLE "RejectionRefactorGuard";
                """);

            migrationBuilder.DropTable(
                name: "RejectedRowReasons");

            migrationBuilder.AddColumn<int>(
                name: "ParsedCodeCount",
                table: "RejectedRows",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ParsedName",
                table: "RejectedRows",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RejectionIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RejectionIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RejectedRowIssues",
                columns: table => new
                {
                    RejectedRowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RejectionIssueId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RejectedRowIssues", x => new { x.RejectedRowId, x.RejectionIssueId });
                    table.ForeignKey(
                        name: "FK_RejectedRowIssues_RejectedRows_RejectedRowId",
                        column: x => x.RejectedRowId,
                        principalTable: "RejectedRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RejectedRowIssues_RejectionIssues_RejectionIssueId",
                        column: x => x.RejectionIssueId,
                        principalTable: "RejectionIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RejectedRowIssues_RejectionIssueId",
                table: "RejectedRowIssues",
                column: "RejectionIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_RejectionIssues_Reason",
                table: "RejectionIssues",
                column: "Reason");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RejectedRowIssues");

            migrationBuilder.DropTable(
                name: "RejectionIssues");

            migrationBuilder.DropColumn(
                name: "ParsedCodeCount",
                table: "RejectedRows");

            migrationBuilder.DropColumn(
                name: "ParsedName",
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
    }
}
