using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class RemoveNullables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CriminalRecords_Characters_CharacterModelId",
                table: "CriminalRecords");

            migrationBuilder.AlterColumn<int>(
                name: "CharacterModelId",
                table: "CriminalRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CriminalRecords_Characters_CharacterModelId",
                table: "CriminalRecords",
                column: "CharacterModelId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CriminalRecords_Characters_CharacterModelId",
                table: "CriminalRecords");

            migrationBuilder.AlterColumn<int>(
                name: "CharacterModelId",
                table: "CriminalRecords",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_CriminalRecords_Characters_CharacterModelId",
                table: "CriminalRecords",
                column: "CharacterModelId",
                principalTable: "Characters",
                principalColumn: "Id");
        }
    }
}
