using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class ChangeToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TargetModelId",
                table: "MdcNodes",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TargetModelId",
                table: "MdcNodes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
