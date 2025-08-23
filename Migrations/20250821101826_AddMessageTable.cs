using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matrix.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletAddress",
                table: "Persons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IsPrivate",
                table: "Persons",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Persons",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentTime",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "IsRead",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "NotifyId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Friendships",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                table: "Friendships",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "FriendshipId",
                table: "Friendships",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PraiseCount",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "IsPublic",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "Articles",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "CollectCount",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "ArticleId",
                table: "Articles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MsgId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsRead = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MsgId);
                    table.ForeignKey(
                        name: "FK_Messages_Persons_SentId",
                        column: x => x.SentId,
                        principalTable: "Persons",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_DisplayName",
                table: "Persons",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_IsPrivate",
                table: "Persons",
                column: "IsPrivate");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_ModifyTime",
                table: "Persons",
                column: "ModifyTime");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_WalletAddress",
                table: "Persons",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GetId_IsRead_SentTime",
                table: "Notifications",
                columns: new[] { "GetId", "IsRead", "SentTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GetId_Type_SentTime",
                table: "Notifications",
                columns: new[] { "GetId", "Type", "SentTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SendId_SentTime",
                table: "Notifications",
                columns: new[] { "SendId", "SentTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SentTime",
                table: "Notifications",
                column: "SentTime");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_FriendId_Status_RequestDate",
                table: "Friendships",
                columns: new[] { "FriendId", "Status", "RequestDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequestDate",
                table: "Friendships",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_Status",
                table: "Friendships",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserId_FriendId",
                table: "Friendships",
                columns: new[] { "UserId", "FriendId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserId_Status_RequestDate",
                table: "Friendships",
                columns: new[] { "UserId", "Status", "RequestDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Friendships_NoSelfFriend",
                table: "Friendships",
                sql: "[UserId] != [FriendId]");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_AuthorId_CreateTime",
                table: "Articles",
                columns: new[] { "AuthorId", "CreateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CollectCount",
                table: "Articles",
                column: "CollectCount");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CreateTime",
                table: "Articles",
                column: "CreateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IsPublic",
                table: "Articles",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IsPublic_Status_CreateTime",
                table: "Articles",
                columns: new[] { "IsPublic", "Status", "CreateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_PraiseCount",
                table: "Articles",
                column: "PraiseCount");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Status",
                table: "Articles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SentId",
                table: "Messages",
                column: "SentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Persons_DisplayName",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_IsPrivate",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_ModifyTime",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_WalletAddress",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_GetId_IsRead_SentTime",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_GetId_Type_SentTime",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_SendId_SentTime",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_SentTime",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Type",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_FriendId_Status_RequestDate",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_RequestDate",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_Status",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_UserId_FriendId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_UserId_Status_RequestDate",
                table: "Friendships");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Friendships_NoSelfFriend",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Articles_AuthorId_CreateTime",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_CollectCount",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_CreateTime",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_IsPublic",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_IsPublic_Status_CreateTime",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_PraiseCount",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_Status",
                table: "Articles");

            migrationBuilder.AlterColumn<string>(
                name: "WalletAddress",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IsPrivate",
                table: "Persons",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Persons",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentTime",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "IsRead",
                table: "Notifications",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "NotifyId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Friendships",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                table: "Friendships",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<Guid>(
                name: "FriendshipId",
                table: "Friendships",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Articles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "PraiseCount",
                table: "Articles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "IsPublic",
                table: "Articles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "Articles",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "CollectCount",
                table: "Articles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "ArticleId",
                table: "Articles",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");
        }
    }
}
