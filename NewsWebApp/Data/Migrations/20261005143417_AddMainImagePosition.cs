using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMainImagePosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MainImagePositionX",
                table: "Articles",
                type: "float",
                nullable: false,
                defaultValue: 50.0);

            migrationBuilder.AddColumn<double>(
                name: "MainImagePositionY",
                table: "Articles",
                type: "float",
                nullable: false,
                defaultValue: 50.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainImagePositionX",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "MainImagePositionY",
                table: "Articles");
        }
    }
}
