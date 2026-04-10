using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManager.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMoviePosterImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PosterData",
                table: "Movies");

            migrationBuilder.AddColumn<byte[]>(
                name: "PosterImage",
                table: "Movies",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PosterImage",
                table: "Movies");

            migrationBuilder.AddColumn<string>(
                name: "PosterData",
                table: "Movies",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
