using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class AddVehInteraPoint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "VehicleInteractionPointPitch",
                table: "Groups",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "VehicleInteractionPointRoll",
                table: "Groups",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "VehicleInteractionPointX",
                table: "Groups",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "VehicleInteractionPointY",
                table: "Groups",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "VehicleInteractionPointYaw",
                table: "Groups",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "VehicleInteractionPointZ",
                table: "Groups",
                type: "real",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleInteractionPointPitch",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "VehicleInteractionPointRoll",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "VehicleInteractionPointX",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "VehicleInteractionPointY",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "VehicleInteractionPointYaw",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "VehicleInteractionPointZ",
                table: "Groups");
        }
    }
}
