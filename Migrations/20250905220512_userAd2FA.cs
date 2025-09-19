using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nauth_asp.Migrations
{
    /// <inheritdoc />
    public partial class userAd2FA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "passwordSalt",
                table: "Users",
                newName: "Name");

            migrationBuilder.AddColumn<bool>(
                name: "isEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "TwoFactorAuthEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "TwoFactorAuthEntries");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Users",
                newName: "passwordSalt");
        }
    }
}
