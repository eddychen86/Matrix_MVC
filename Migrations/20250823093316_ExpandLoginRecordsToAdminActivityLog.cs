using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matrix.Migrations
{
    /// <inheritdoc />
    public partial class ExpandLoginRecordsToAdminActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TODO(human): 移除索引相關的操作，專注於 LoginRecords 表結構變更
            
            migrationBuilder.DropColumn(
                name: "History",
                table: "LoginRecords");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "LoginRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LoginTime",
                table: "LoginRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "LoginRecords",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ActionDescription",
                table: "LoginRecords",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActionTime",
                table: "LoginRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "LoginRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdminName",
                table: "LoginRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "LoginRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "LoginRecords",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccessful",
                table: "LoginRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LogoutTime",
                table: "LoginRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PagePath",
                table: "LoginRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "LoginRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionDescription",
                table: "LoginRecords");

            migrationBuilder.DropColumn(
                name: "ActionTime",
                table: "LoginRecords");

            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "LoginRecords");

            migrationBuilder.DropColumn(
                name: "AdminName",
                table: "LoginRecords");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "LoginRecords");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "LoginRecords");

            migrationBuilder.DropColumn(
                name: "IsSuccessful",
                table: "LoginRecords");

            migrationBuilder.DropColumn(
                name: "LogoutTime",
                table: "LoginRecords");

            migrationBuilder.DropColumn(
                name: "PagePath",
                table: "LoginRecords");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "LoginRecords");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "LoginRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LoginTime",
                table: "LoginRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "LoginRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45);

            migrationBuilder.AddColumn<string>(
                name: "History",
                table: "LoginRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
