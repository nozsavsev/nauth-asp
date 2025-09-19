using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nauth_asp.Migrations
{
    /// <inheritdoc />
    public partial class EmailActionsRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailActions_Users_DB_UserId",
                table: "EmailActions");

            migrationBuilder.DropIndex(
                name: "IX_EmailActions_DB_UserId",
                table: "EmailActions");

            migrationBuilder.DropColumn(
                name: "DB_UserId",
                table: "EmailActions");

            migrationBuilder.AddColumn<long>(
                name: "userId",
                table: "EmailActions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_EmailActions_userId",
                table: "EmailActions",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailActions_Users_userId",
                table: "EmailActions",
                column: "userId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailActions_Users_userId",
                table: "EmailActions");

            migrationBuilder.DropIndex(
                name: "IX_EmailActions_userId",
                table: "EmailActions");

            migrationBuilder.DropColumn(
                name: "userId",
                table: "EmailActions");

            migrationBuilder.AddColumn<long>(
                name: "DB_UserId",
                table: "EmailActions",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailActions_DB_UserId",
                table: "EmailActions",
                column: "DB_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailActions_Users_DB_UserId",
                table: "EmailActions",
                column: "DB_UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
