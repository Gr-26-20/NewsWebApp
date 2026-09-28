using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedArticlePropFromFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Articles_ArticleId",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_ArticleId",
                table: "Feedback");

            migrationBuilder.AlterColumn<int>(
                name: "ArticleId",
                table: "Feedback",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<int>(
                name: "ArticleId",
                table: "Feedback",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_ArticleId",
                table: "Feedback",
                column: "ArticleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Articles_ArticleId",
                table: "Feedback",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "Id");
        }
    }
}
