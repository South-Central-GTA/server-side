using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class MakeNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Directories_DirectoryModelId",
                table: "Files");

            migrationBuilder.AlterColumn<int>(
                name: "DirectoryModelId",
                table: "Files",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Directories_DirectoryModelId",
                table: "Files",
                column: "DirectoryModelId",
                principalTable: "Directories",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Directories_DirectoryModelId",
                table: "Files");

            migrationBuilder.AlterColumn<int>(
                name: "DirectoryModelId",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Directories_DirectoryModelId",
                table: "Files",
                column: "DirectoryModelId",
                principalTable: "Directories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
