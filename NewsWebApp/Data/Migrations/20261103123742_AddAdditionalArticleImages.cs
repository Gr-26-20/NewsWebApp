using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalArticleImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecondaryImageCaption",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryImageUrl",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdImageCaption",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdImageUrl",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondaryImageCaption",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "SecondaryImageUrl",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ThirdImageCaption",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "ThirdImageUrl",
                table: "Articles");
        }
    }
}
