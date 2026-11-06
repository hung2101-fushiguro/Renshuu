using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TracNghiemWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingPassages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OptionStyle",
                table: "Questions",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "ABCD");

            migrationBuilder.AddColumn<int>(
                name: "PassageId",
                table: "Questions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReadingPassages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingPassages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReadingPassages_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_PassageId",
                table: "Questions",
                column: "PassageId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingPassages_SubjectId",
                table: "ReadingPassages",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_ReadingPassages_PassageId",
                table: "Questions",
                column: "PassageId",
                principalTable: "ReadingPassages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_ReadingPassages_PassageId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "ReadingPassages");

            migrationBuilder.DropIndex(
                name: "IX_Questions_PassageId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "OptionStyle",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PassageId",
                table: "Questions");
        }
    }
}
