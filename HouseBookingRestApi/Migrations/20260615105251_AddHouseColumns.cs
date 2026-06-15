using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseBookingRestApi.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Houses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerNight",
                table: "Houses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Houses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "PricePerNight",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Houses");
        }
    }
}
