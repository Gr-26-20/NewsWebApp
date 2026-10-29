using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewsletterBools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSubscribedToNewsNewsletter",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscribedToSportsNewsletter",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscribedToSwedenNewsletter",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscribedToWeatherNewsletter",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscribedToWorldNewsletter",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSubscribedToNewsNewsletter",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsSubscribedToSportsNewsletter",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsSubscribedToSwedenNewsletter",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsSubscribedToWeatherNewsletter",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsSubscribedToWorldNewsletter",
                table: "AspNetUsers");
        }
    }
}
