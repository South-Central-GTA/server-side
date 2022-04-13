using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class FixNamingInDirectory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EditGroupLevel",
                table: "Directories",
                newName: "WriteGroupLevel");

            migrationBuilder.RenameColumn(
                name: "AccessGroupLevel",
                table: "Directories",
                newName: "ReadGroupLevel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WriteGroupLevel",
                table: "Directories",
                newName: "EditGroupLevel");

            migrationBuilder.RenameColumn(
                name: "ReadGroupLevel",
                table: "Directories",
                newName: "AccessGroupLevel");
        }
    }
}
