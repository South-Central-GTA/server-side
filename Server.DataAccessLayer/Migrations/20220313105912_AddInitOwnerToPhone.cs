using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class AddInitOwnerToPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Items",
                newName: "InitialOwnerId");

            migrationBuilder.AddColumn<int>(
                name: "CurrentOwnerId",
                table: "Items",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentOwnerId",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "InitialOwnerId",
                table: "Items",
                newName: "OwnerId");
        }
    }
}
