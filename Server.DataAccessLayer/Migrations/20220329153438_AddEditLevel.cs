using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class AddEditLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EditGroupLevel",
                table: "Directories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditGroupLevel",
                table: "Directories");
        }
    }
}
