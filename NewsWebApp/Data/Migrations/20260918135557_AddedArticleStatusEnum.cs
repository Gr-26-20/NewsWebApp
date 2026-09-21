using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedArticleStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "articleStatus",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "articleStatus",
                table: "Articles");
        }
    }
}
