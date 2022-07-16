using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class AddTuningParts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Aerials",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AirFilter",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ArchCover",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Armor",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BackWheels",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Boost",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Breaks",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Dashboard",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DialDesign",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DoorSpeaker",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Engine",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EngineBlock",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Exhaust",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Fender",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Frame",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FrontBumper",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FrontWheels",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Grille",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Hood",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Horns",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Hydraulics",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ornaments",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Plaques",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Plate",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlateHolder",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlateVanity",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RearBumper",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RightFender",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Roof",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Seats",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShiftLever",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SideSkirt",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Speakers",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Spoilers",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SteeringWheel",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Struts",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Suspension",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tank",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Transmission",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Trim",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrimDesign",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Trunk",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Turbo",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WindowTint",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Windows",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Xenon",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aerials",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "AirFilter",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ArchCover",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Armor",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "BackWheels",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Boost",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Breaks",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Dashboard",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DialDesign",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DoorSpeaker",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Engine",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "EngineBlock",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Exhaust",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Fender",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Frame",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FrontBumper",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FrontWheels",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Grille",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Hood",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Horns",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Hydraulics",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Ornaments",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Plaques",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Plate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PlateHolder",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PlateVanity",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "RearBumper",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "RightFender",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Roof",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Seats",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ShiftLever",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "SideSkirt",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Speakers",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Spoilers",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "SteeringWheel",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Struts",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Suspension",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Tank",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Transmission",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Trim",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TrimDesign",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Trunk",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Turbo",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "WindowTint",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Windows",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Xenon",
                table: "Vehicles");
        }
    }
}
