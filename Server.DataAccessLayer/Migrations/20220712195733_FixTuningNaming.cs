using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class FixTuningNaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Speakers",
                table: "Vehicles",
                newName: "Speaker");

            migrationBuilder.RenameColumn(
                name: "Breaks",
                table: "Vehicles",
                newName: "Brakes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Speaker",
                table: "Vehicles",
                newName: "Speakers");

            migrationBuilder.RenameColumn(
                name: "Brakes",
                table: "Vehicles",
                newName: "Breaks");
        }
    }
}
