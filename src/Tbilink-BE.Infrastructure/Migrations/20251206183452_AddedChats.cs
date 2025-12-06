using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tbilink_BE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "Messages",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GroupName",
                table: "Messages",
                column: "GroupName");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Groups_GroupName",
                table: "Messages",
                column: "GroupName",
                principalTable: "Groups",
                principalColumn: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Groups_GroupName",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_GroupName",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "Messages");
        }
    }
}
