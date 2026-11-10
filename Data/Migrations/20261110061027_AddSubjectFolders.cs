using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TracNghiemWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "Subjects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubjectFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectFolders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_FolderId",
                table: "Subjects",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectFolders_Name",
                table: "SubjectFolders",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_SubjectFolders_FolderId",
                table: "Subjects",
                column: "FolderId",
                principalTable: "SubjectFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [Subjects] WHERE [Name] LIKE N'JLPT-N3%')
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM [SubjectFolders] WHERE [Name] = N'JLPT N3')
                    BEGIN
                        INSERT INTO [SubjectFolders] ([Name]) VALUES (N'JLPT N3');
                    END;

                    UPDATE [Subjects]
                    SET [FolderId] = (SELECT [Id] FROM [SubjectFolders] WHERE [Name] = N'JLPT N3')
                    WHERE [Name] LIKE N'JLPT-N3%';
                END;

                IF EXISTS (SELECT 1 FROM [Subjects] WHERE [Name] LIKE N'PRN222%')
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM [SubjectFolders] WHERE [Name] = N'PRN222')
                    BEGIN
                        INSERT INTO [SubjectFolders] ([Name]) VALUES (N'PRN222');
                    END;

                    UPDATE [Subjects]
                    SET [FolderId] = (SELECT [Id] FROM [SubjectFolders] WHERE [Name] = N'PRN222')
                    WHERE [Name] LIKE N'PRN222%';
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_SubjectFolders_FolderId",
                table: "Subjects");

            migrationBuilder.DropTable(
                name: "SubjectFolders");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_FolderId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "Subjects");
        }
    }
}
