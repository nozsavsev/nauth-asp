using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nauth_asp.Migrations
{
    /// <inheritdoc />
    public partial class ServiceUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DB_ServicePermission_Permissions_permissionId",
                table: "DB_ServicePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_DB_ServicePermission_Services_serviceId",
                table: "DB_ServicePermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DB_ServicePermission",
                table: "DB_ServicePermission");

            migrationBuilder.DropIndex(
                name: "IX_DB_ServicePermission_serviceId",
                table: "DB_ServicePermission");

            migrationBuilder.RenameTable(
                name: "DB_ServicePermission",
                newName: "ServicePermissions");

            migrationBuilder.RenameIndex(
                name: "IX_DB_ServicePermission_permissionId",
                table: "ServicePermissions",
                newName: "IX_ServicePermissions_permissionId");

            migrationBuilder.AddColumn<long>(
                name: "DB_PermissionId",
                table: "ServicePermissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServicePermissions",
                table: "ServicePermissions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePermissions_DB_PermissionId",
                table: "ServicePermissions",
                column: "DB_PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePermissions_serviceId_permissionId",
                table: "ServicePermissions",
                columns: new[] { "serviceId", "permissionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePermissions_Permissions_DB_PermissionId",
                table: "ServicePermissions",
                column: "DB_PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePermissions_Permissions_permissionId",
                table: "ServicePermissions",
                column: "permissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePermissions_Services_serviceId",
                table: "ServicePermissions",
                column: "serviceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicePermissions_Permissions_DB_PermissionId",
                table: "ServicePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicePermissions_Permissions_permissionId",
                table: "ServicePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicePermissions_Services_serviceId",
                table: "ServicePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServicePermissions",
                table: "ServicePermissions");

            migrationBuilder.DropIndex(
                name: "IX_ServicePermissions_DB_PermissionId",
                table: "ServicePermissions");

            migrationBuilder.DropIndex(
                name: "IX_ServicePermissions_serviceId_permissionId",
                table: "ServicePermissions");

            migrationBuilder.DropColumn(
                name: "DB_PermissionId",
                table: "ServicePermissions");

            migrationBuilder.RenameTable(
                name: "ServicePermissions",
                newName: "DB_ServicePermission");

            migrationBuilder.RenameIndex(
                name: "IX_ServicePermissions_permissionId",
                table: "DB_ServicePermission",
                newName: "IX_DB_ServicePermission_permissionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DB_ServicePermission",
                table: "DB_ServicePermission",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DB_ServicePermission_serviceId",
                table: "DB_ServicePermission",
                column: "serviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DB_ServicePermission_Permissions_permissionId",
                table: "DB_ServicePermission",
                column: "permissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DB_ServicePermission_Services_serviceId",
                table: "DB_ServicePermission",
                column: "serviceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
