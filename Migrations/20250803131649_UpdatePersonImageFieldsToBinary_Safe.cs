using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matrix.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonImageFieldsToBinary_Safe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 步驟 1: 先移除 ExternalUrl 欄位
            migrationBuilder.DropColumn(
                name: "ExternalUrl",
                table: "Persons");

            // 步驟 2: 新增臨時的二進制欄位
            migrationBuilder.AddColumn<byte[]>(
                name: "AvatarPath_New",
                table: "Persons",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "BannerPath_New",
                table: "Persons",
                type: "varbinary(max)",
                nullable: true);

            // 步驟 3: 清空舊的字串資料（因為無法轉換）
            migrationBuilder.Sql("UPDATE Persons SET AvatarPath = NULL, BannerPath = NULL");

            // 步驟 4: 移除舊的字串欄位
            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "BannerPath",
                table: "Persons");

            // 步驟 5: 重新命名新欄位為原來的名稱
            migrationBuilder.RenameColumn(
                name: "AvatarPath_New",
                table: "Persons",
                newName: "AvatarPath");

            migrationBuilder.RenameColumn(
                name: "BannerPath_New",
                table: "Persons",
                newName: "BannerPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BannerPath",
                table: "Persons",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarPath",
                table: "Persons",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUrl",
                table: "Persons",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}
