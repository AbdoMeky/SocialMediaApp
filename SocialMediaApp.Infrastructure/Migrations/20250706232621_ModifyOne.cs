using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMember_Chat_GroupChatId",
                table: "ChatMember");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendShipRequests_User_UserId",
                table: "FriendShipRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendShips_User_UserId",
                table: "FriendShips");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FriendShips",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "FriendId",
                table: "FriendShips",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FriendShipRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ReseaverId",
                table: "FriendShipRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMember_Chat_GroupChatId",
                table: "ChatMember",
                column: "GroupChatId",
                principalTable: "Chat",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendShipRequests_User_UserId",
                table: "FriendShipRequests",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendShips_User_UserId",
                table: "FriendShips",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMember_Chat_GroupChatId",
                table: "ChatMember");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendShipRequests_User_UserId",
                table: "FriendShipRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendShips_User_UserId",
                table: "FriendShips");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "User");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FriendShips",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FriendId",
                table: "FriendShips",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FriendShipRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReseaverId",
                table: "FriendShipRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMember_Chat_GroupChatId",
                table: "ChatMember",
                column: "GroupChatId",
                principalTable: "Chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FriendShipRequests_User_UserId",
                table: "FriendShipRequests",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FriendShips_User_UserId",
                table: "FriendShips",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
