using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageZoomProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CategoryImageZoom",
                table: "Articles",
                type: "float",
                nullable: false,
                defaultValue: 100.0);

            migrationBuilder.AddColumn<double>(
                name: "SecondaryImageZoom",
                table: "Articles",
                type: "float",
                nullable: false,
                defaultValue: 100.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryImageZoom",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "SecondaryImageZoom",
                table: "Articles");
        }
    }
}
