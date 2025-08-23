using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matrix.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminActivityLogIndexesAndConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoginRecords_Persons_UserId",
                table: "LoginRecords");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "LoginRecords",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSuccessful",
                table: "LoginRecords",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "LoginRecords",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActionTime",
                table: "LoginRecords",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "LoginId",
                table: "LoginRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_LoginRecords_ActionTime",
                table: "LoginRecords",
                column: "ActionTime");

            migrationBuilder.CreateIndex(
                name: "IX_LoginRecords_ActionType",
                table: "LoginRecords",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_LoginRecords_ActionType_ActionTime",
                table: "LoginRecords",
                columns: new[] { "ActionType", "ActionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginRecords_IpAddress",
                table: "LoginRecords",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_LoginRecords_IsSuccessful",
                table: "LoginRecords",
                column: "IsSuccessful");

            migrationBuilder.CreateIndex(
                name: "IX_LoginRecords_PagePath",
                table: "LoginRecords",
                column: "PagePath");

            migrationBuilder.CreateIndex(
                name: "IX_LoginRecords_UserId_ActionTime",
                table: "LoginRecords",
                columns: new[] { "UserId", "ActionTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_LoginRecords_Persons_UserId",
                table: "LoginRecords",
                column: "UserId",
                principalTable: "Persons",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoginRecords_Persons_UserId",
                table: "LoginRecords");

            migrationBuilder.DropIndex(
                name: "IX_LoginRecords_ActionTime",
                table: "LoginRecords");

            migrationBuilder.DropIndex(
                name: "IX_LoginRecords_ActionType",
                table: "LoginRecords");

            migrationBuilder.DropIndex(
                name: "IX_LoginRecords_ActionType_ActionTime",
                table: "LoginRecords");

            migrationBuilder.DropIndex(
                name: "IX_LoginRecords_IpAddress",
                table: "LoginRecords");

            migrationBuilder.DropIndex(
                name: "IX_LoginRecords_IsSuccessful",
                table: "LoginRecords");

            migrationBuilder.DropIndex(
                name: "IX_LoginRecords_PagePath",
                table: "LoginRecords");

            migrationBuilder.DropIndex(
                name: "IX_LoginRecords_UserId_ActionTime",
                table: "LoginRecords");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "LoginRecords",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSuccessful",
                table: "LoginRecords",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "LoginRecords",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActionTime",
                table: "LoginRecords",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<Guid>(
                name: "LoginId",
                table: "LoginRecords",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginRecords_Persons_UserId",
                table: "LoginRecords",
                column: "UserId",
                principalTable: "Persons",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
