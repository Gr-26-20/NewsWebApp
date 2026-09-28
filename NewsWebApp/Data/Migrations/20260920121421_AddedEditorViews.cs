using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedEditorViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArticlesId",
                table: "Feedback",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_ArticlesId",
                table: "Feedback",
                column: "ArticlesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Articles_ArticlesId",
                table: "Feedback",
                column: "ArticlesId",
                principalTable: "Articles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Articles_ArticlesId",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_ArticlesId",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "ArticlesId",
                table: "Feedback");
        }
    }
}
