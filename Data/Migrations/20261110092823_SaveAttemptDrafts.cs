using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TracNghiemWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class SaveAttemptDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DraftSavedAt",
                table: "Attempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_SubmittedAt_Id",
                table: "Attempts",
                columns: new[] { "SubmittedAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attempts_SubmittedAt_Id",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "DraftSavedAt",
                table: "Attempts");
        }
    }
}
