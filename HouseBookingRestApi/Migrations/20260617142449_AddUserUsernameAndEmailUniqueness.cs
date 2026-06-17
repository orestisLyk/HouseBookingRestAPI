using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseBookingRestApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserUsernameAndEmailUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "Users",
                newName: "UQ_Users_Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "UQ_Users_Username",
                table: "Users");

            migrationBuilder.RenameIndex(
                name: "UQ_Users_Email",
                table: "Users",
                newName: "IX_Users_Email");
        }
    }
}
