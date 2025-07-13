using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matrix.Data.Migrations
{
    /// <inheritdoc />
    public partial class Change_Friend_and_ArticleAttachment_Id_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Friends",
                table: "Friends");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleAttachment",
                table: "ArticleAttachment");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Friends");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ArticleAttachment");

            migrationBuilder.AddColumn<string>(
                name: "FId",
                table: "Friends",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "FileId",
                table: "ArticleAttachment",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friends",
                table: "Friends",
                column: "FId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleAttachment",
                table: "ArticleAttachment",
                column: "FileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Friends",
                table: "Friends");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleAttachment",
                table: "ArticleAttachment");

            migrationBuilder.DropColumn(
                name: "FId",
                table: "Friends");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "ArticleAttachment");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Friends",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ArticleAttachment",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friends",
                table: "Friends",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleAttachment",
                table: "ArticleAttachment",
                column: "Id");
        }
    }
}
