using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class BlockedCanNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BlockedByCharacterName",
                table: "Files",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BlockedByCharacterName",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
