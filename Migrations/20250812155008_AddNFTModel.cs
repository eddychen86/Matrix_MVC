using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matrix.Migrations
{
    /// <inheritdoc />
    public partial class AddNFTModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Persons_AuthorId",
                table: "Articles");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerPersonId",
                table: "Articles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NFTs",
                columns: table => new
                {
                    NftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    CollectTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(28,18)", precision: 28, scale: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NFTs", x => x.NftId);
                    table.ForeignKey(
                        name: "FK_NFTs_Persons_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Persons",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_OwnerPersonId",
                table: "Articles",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_NFTs_CollectTime",
                table: "NFTs",
                column: "CollectTime");

            migrationBuilder.CreateIndex(
                name: "IX_NFTs_Currency",
                table: "NFTs",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_NFTs_OwnerId",
                table: "NFTs",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Persons_AuthorId",
                table: "Articles",
                column: "AuthorId",
                principalTable: "Persons",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Persons_OwnerPersonId",
                table: "Articles",
                column: "OwnerPersonId",
                principalTable: "Persons",
                principalColumn: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Persons_AuthorId",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Persons_OwnerPersonId",
                table: "Articles");

            migrationBuilder.DropTable(
                name: "NFTs");

            migrationBuilder.DropIndex(
                name: "IX_Articles_OwnerPersonId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "OwnerPersonId",
                table: "Articles");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Persons_AuthorId",
                table: "Articles",
                column: "AuthorId",
                principalTable: "Persons",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
