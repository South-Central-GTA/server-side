using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.DataAccessLayer.Migrations
{
    public partial class FixKeyMail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MailAccountGroupAccessModel_Groups_GroupModelId",
                table: "MailAccountGroupAccessModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MailAccountGroupAccessModel",
                table: "MailAccountGroupAccessModel");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "MailAccountGroupAccessModel");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "OrderedVehicles",
                newName: "GroupModelId");

            migrationBuilder.AlterColumn<int>(
                name: "GroupModelId",
                table: "MailAccountGroupAccessModel",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MailAccountGroupAccessModel",
                table: "MailAccountGroupAccessModel",
                columns: new[] { "MailAccountModelMailAddress", "GroupModelId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MailAccountGroupAccessModel_Groups_GroupModelId",
                table: "MailAccountGroupAccessModel",
                column: "GroupModelId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MailAccountGroupAccessModel_Groups_GroupModelId",
                table: "MailAccountGroupAccessModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MailAccountGroupAccessModel",
                table: "MailAccountGroupAccessModel");

            migrationBuilder.RenameColumn(
                name: "GroupModelId",
                table: "OrderedVehicles",
                newName: "GroupId");

            migrationBuilder.AlterColumn<int>(
                name: "GroupModelId",
                table: "MailAccountGroupAccessModel",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "MailAccountGroupAccessModel",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MailAccountGroupAccessModel",
                table: "MailAccountGroupAccessModel",
                columns: new[] { "MailAccountModelMailAddress", "GroupId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MailAccountGroupAccessModel_Groups_GroupModelId",
                table: "MailAccountGroupAccessModel",
                column: "GroupModelId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
