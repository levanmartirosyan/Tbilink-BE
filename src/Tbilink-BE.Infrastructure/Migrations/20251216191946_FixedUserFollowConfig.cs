using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tbilink_BE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedUserFollowConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFollows_Users_FollowedId",
                table: "UserFollows");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFollows_Users_FollowerId",
                table: "UserFollows");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollows_Users_FollowedId",
                table: "UserFollows",
                column: "FollowedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollows_Users_FollowerId",
                table: "UserFollows",
                column: "FollowerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFollows_Users_FollowedId",
                table: "UserFollows");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFollows_Users_FollowerId",
                table: "UserFollows");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollows_Users_FollowedId",
                table: "UserFollows",
                column: "FollowedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollows_Users_FollowerId",
                table: "UserFollows",
                column: "FollowerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
