using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matrix.Migrations
{
    /// <inheritdoc />
    public partial class Add_Unique_PraiseCollect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PraiseCollects_UserId",
                table: "PraiseCollects");

            migrationBuilder.CreateIndex(
                name: "IX_PraiseCollects_UserId_ArticleId_Type",
                table: "PraiseCollects",
                columns: new[] { "UserId", "ArticleId", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PraiseCollects_UserId_ArticleId_Type",
                table: "PraiseCollects");

            migrationBuilder.CreateIndex(
                name: "IX_PraiseCollects_UserId",
                table: "PraiseCollects",
                column: "UserId");
        }
    }
}
