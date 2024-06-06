using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPFinalProject.Migrations
{
    /// <inheritdoc />
    public partial class deleteDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Documents_DocumentId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Questions_DocumentId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "DocumentPage",
                table: "Questions");

            migrationBuilder.AlterColumn<string>(
                name: "Answer",
                table: "Questions",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(sbyte),
                oldType: "tinyint")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<sbyte>(
                name: "Answer",
                table: "Questions",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Questions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentPage",
                table: "Questions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_DocumentId",
                table: "Questions",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Documents_DocumentId",
                table: "Questions",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId");
        }
    }
}
