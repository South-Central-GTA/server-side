using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class FixMisSpelling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Node",
                table: "MdcNodes",
                newName: "Note");

            migrationBuilder.AlterColumn<int>(
                name: "TargetModelId",
                table: "MdcNodes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Note",
                table: "MdcNodes",
                newName: "Node");

            migrationBuilder.AlterColumn<int>(
                name: "TargetModelId",
                table: "MdcNodes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
