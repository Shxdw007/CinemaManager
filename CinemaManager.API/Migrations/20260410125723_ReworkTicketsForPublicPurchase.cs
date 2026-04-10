using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManager.API.Migrations
{
    /// <inheritdoc />
    public partial class ReworkTicketsForPublicPurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_UserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SessionId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "RowNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Tickets",
                newName: "Seat");

            migrationBuilder.RenameColumn(
                name: "SeatNumber",
                table: "Tickets",
                newName: "Row");

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "Tickets",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SessionId_Row_Seat",
                table: "Tickets",
                columns: new[] { "SessionId", "Row", "Seat" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_SessionId_Row_Seat",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "Seat",
                table: "Tickets",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Row",
                table: "Tickets",
                newName: "SeatNumber");

            migrationBuilder.AddColumn<int>(
                name: "RowNumber",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Tickets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SessionId",
                table: "Tickets",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_UserId",
                table: "Tickets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
