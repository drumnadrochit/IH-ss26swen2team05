using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourRoutingValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "FromLatitude",
                table: "Tours",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FromLongitude",
                table: "Tours",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RouteGeoJson",
                table: "Tours",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ToLatitude",
                table: "Tours",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ToLongitude",
                table: "Tours",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromLatitude",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "FromLongitude",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "RouteGeoJson",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "ToLatitude",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "ToLongitude",
                table: "Tours");
        }
    }
}
