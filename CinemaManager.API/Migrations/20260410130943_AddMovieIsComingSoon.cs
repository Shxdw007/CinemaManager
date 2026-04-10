using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManager.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieIsComingSoon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComingSoon",
                table: "Movies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComingSoon",
                table: "Movies");
        }
    }
}
