using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nauth_asp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Services_ServiceId1",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Users_userId1",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Services_serviceId1",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_userId1",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_TwoFactorAuthEntries_Users_userId1",
                table: "TwoFactorAuthEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Permissions_permissionId1",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Users_userId1",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserServices_Services_serviceId1",
                table: "UserServices");

            migrationBuilder.DropForeignKey(
                name: "FK_UserServices_Users_userId1",
                table: "UserServices");

            migrationBuilder.DropIndex(
                name: "IX_UserServices_serviceId1",
                table: "UserServices");

            migrationBuilder.DropIndex(
                name: "IX_UserServices_userId1",
                table: "UserServices");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_permissionId1",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_userId1",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_TwoFactorAuthEntries_userId1",
                table: "TwoFactorAuthEntries");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_serviceId1",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_userId1",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Services_userId1",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_ServiceId1",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "serviceId1",
                table: "UserServices");

            migrationBuilder.DropColumn(
                name: "userId1",
                table: "UserServices");

            migrationBuilder.DropColumn(
                name: "permissionId1",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "userId1",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "userId1",
                table: "TwoFactorAuthEntries");

            migrationBuilder.DropColumn(
                name: "serviceId1",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "userId1",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "userId1",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ServiceId1",
                table: "Permissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "serviceId1",
                table: "UserServices",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "userId1",
                table: "UserServices",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "permissionId1",
                table: "UserPermissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "userId1",
                table: "UserPermissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "userId1",
                table: "TwoFactorAuthEntries",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "serviceId1",
                table: "Sessions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "userId1",
                table: "Sessions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "userId1",
                table: "Services",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ServiceId1",
                table: "Permissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserServices_serviceId1",
                table: "UserServices",
                column: "serviceId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserServices_userId1",
                table: "UserServices",
                column: "userId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_permissionId1",
                table: "UserPermissions",
                column: "permissionId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_userId1",
                table: "UserPermissions",
                column: "userId1");

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorAuthEntries_userId1",
                table: "TwoFactorAuthEntries",
                column: "userId1");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_serviceId1",
                table: "Sessions",
                column: "serviceId1");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_userId1",
                table: "Sessions",
                column: "userId1");

            migrationBuilder.CreateIndex(
                name: "IX_Services_userId1",
                table: "Services",
                column: "userId1");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ServiceId1",
                table: "Permissions",
                column: "ServiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Services_ServiceId1",
                table: "Permissions",
                column: "ServiceId1",
                principalTable: "Services",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Users_userId1",
                table: "Services",
                column: "userId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Services_serviceId1",
                table: "Sessions",
                column: "serviceId1",
                principalTable: "Services",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Users_userId1",
                table: "Sessions",
                column: "userId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TwoFactorAuthEntries_Users_userId1",
                table: "TwoFactorAuthEntries",
                column: "userId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Permissions_permissionId1",
                table: "UserPermissions",
                column: "permissionId1",
                principalTable: "Permissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Users_userId1",
                table: "UserPermissions",
                column: "userId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserServices_Services_serviceId1",
                table: "UserServices",
                column: "serviceId1",
                principalTable: "Services",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserServices_Users_userId1",
                table: "UserServices",
                column: "userId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
