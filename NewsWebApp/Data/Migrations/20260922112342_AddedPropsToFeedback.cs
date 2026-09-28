using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedPropsToFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Articles_ArticlesId",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "ArticlesId",
                table: "Feedback",
                newName: "ArticleId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_ArticlesId",
                table: "Feedback",
                newName: "IX_Feedback_ArticleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Articles_ArticleId",
                table: "Feedback",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Articles_ArticleId",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "ArticleId",
                table: "Feedback",
                newName: "ArticlesId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_ArticleId",
                table: "Feedback",
                newName: "IX_Feedback_ArticlesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Articles_ArticlesId",
                table: "Feedback",
                column: "ArticlesId",
                principalTable: "Articles",
                principalColumn: "Id");
        }
    }
}
