using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matrix.Migrations
{
    /// <inheritdoc />
    public partial class AddForgotPwdTokenToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ForgotPwdToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForgotPwdToken",
                table: "Users");
        }
    }
}
