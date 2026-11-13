using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TracNghiemWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPracticeMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPractice",
                table: "Attempts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPractice",
                table: "Attempts");
        }
    }
}
