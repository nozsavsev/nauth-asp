using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nauth_asp.Migrations
{
    /// <inheritdoc />
    public partial class SessionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicePermissions");

            migrationBuilder.AddColumn<string>(
                name: "Browser",
                table: "Sessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "Sessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Sessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Os",
                table: "Sessions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Browser",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Device",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Os",
                table: "Sessions");

            migrationBuilder.CreateTable(
                name: "ServicePermissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    permissionId = table.Column<long>(type: "bigint", nullable: false),
                    serviceId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DB_PermissionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePermissions_Permissions_DB_PermissionId",
                        column: x => x.DB_PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServicePermissions_Permissions_permissionId",
                        column: x => x.permissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicePermissions_Services_serviceId",
                        column: x => x.serviceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicePermissions_DB_PermissionId",
                table: "ServicePermissions",
                column: "DB_PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePermissions_permissionId",
                table: "ServicePermissions",
                column: "permissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePermissions_serviceId_permissionId",
                table: "ServicePermissions",
                columns: new[] { "serviceId", "permissionId" },
                unique: true);
        }
    }
}
