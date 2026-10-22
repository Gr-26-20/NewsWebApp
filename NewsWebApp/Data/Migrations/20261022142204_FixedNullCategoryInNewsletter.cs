using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedNullCategoryInNewsletter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsLetters_Categories_CategoryId",
                table: "NewsLetters");

            migrationBuilder.DropIndex(
                name: "IX_NewsLetters_CategoryId",
                table: "NewsLetters");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "NewsLetters");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "NewsLetters",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "NewsLetters");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "NewsLetters",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsLetters_CategoryId",
                table: "NewsLetters",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsLetters_Categories_CategoryId",
                table: "NewsLetters",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }
    }
}
