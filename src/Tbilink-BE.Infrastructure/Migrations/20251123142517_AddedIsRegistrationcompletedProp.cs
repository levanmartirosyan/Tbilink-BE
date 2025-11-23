using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tbilink_BE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsRegistrationcompletedProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRegistrationCompleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRegistrationCompleted",
                table: "Users");
        }
    }
}
