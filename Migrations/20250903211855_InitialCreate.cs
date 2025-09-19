using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nauth_asp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    isEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    passwordHash = table.Column<string>(type: "text", nullable: false),
                    passwordSalt = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailActions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DB_UserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailActions_Users_DB_UserId",
                        column: x => x.DB_UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    userId = table.Column<long>(type: "bigint", nullable: false),
                    userId1 = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Services_Users_userId1",
                        column: x => x.userId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TwoFactorAuthEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    recoveryCode = table.Column<string>(type: "text", nullable: false),
                    _2faSecret = table.Column<string>(type: "text", nullable: false),
                    userId = table.Column<long>(type: "bigint", nullable: false),
                    userId1 = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoFactorAuthEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwoFactorAuthEntries_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TwoFactorAuthEntries_Users_userId1",
                        column: x => x.userId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId1 = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Permissions_Services_ServiceId1",
                        column: x => x.ServiceId1,
                        principalTable: "Services",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    jwtHash = table.Column<string>(type: "text", nullable: false),
                    userId = table.Column<long>(type: "bigint", nullable: false),
                    userId1 = table.Column<long>(type: "bigint", nullable: true),
                    serviceId = table.Column<long>(type: "bigint", nullable: true),
                    serviceId1 = table.Column<long>(type: "bigint", nullable: true),
                    is2FAConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Services_serviceId",
                        column: x => x.serviceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Sessions_Services_serviceId1",
                        column: x => x.serviceId1,
                        principalTable: "Services",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sessions_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_userId1",
                        column: x => x.userId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserServices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userId = table.Column<long>(type: "bigint", nullable: false),
                    userId1 = table.Column<long>(type: "bigint", nullable: true),
                    serviceId = table.Column<long>(type: "bigint", nullable: false),
                    serviceId1 = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserServices_Services_serviceId",
                        column: x => x.serviceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserServices_Services_serviceId1",
                        column: x => x.serviceId1,
                        principalTable: "Services",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserServices_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserServices_Users_userId1",
                        column: x => x.userId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    permissionId = table.Column<long>(type: "bigint", nullable: false),
                    permissionId1 = table.Column<long>(type: "bigint", nullable: true),
                    userId = table.Column<long>(type: "bigint", nullable: false),
                    userId1 = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_permissionId",
                        column: x => x.permissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_permissionId1",
                        column: x => x.permissionId1,
                        principalTable: "Permissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_userId1",
                        column: x => x.userId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailActions_DB_UserId",
                table: "EmailActions",
                column: "DB_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailActions_token",
                table: "EmailActions",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_key_ServiceId",
                table: "Permissions",
                columns: new[] { "key", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ServiceId",
                table: "Permissions",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ServiceId1",
                table: "Permissions",
                column: "ServiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_Services_userId",
                table: "Services",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_userId1",
                table: "Services",
                column: "userId1");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_serviceId",
                table: "Sessions",
                column: "serviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_serviceId1",
                table: "Sessions",
                column: "serviceId1");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_userId",
                table: "Sessions",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_userId1",
                table: "Sessions",
                column: "userId1");

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorAuthEntries_userId",
                table: "TwoFactorAuthEntries",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorAuthEntries_userId1",
                table: "TwoFactorAuthEntries",
                column: "userId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_permissionId",
                table: "UserPermissions",
                column: "permissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_permissionId1",
                table: "UserPermissions",
                column: "permissionId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_userId_permissionId",
                table: "UserPermissions",
                columns: new[] { "userId", "permissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_userId1",
                table: "UserPermissions",
                column: "userId1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserServices_serviceId",
                table: "UserServices",
                column: "serviceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserServices_serviceId1",
                table: "UserServices",
                column: "serviceId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserServices_userId_serviceId",
                table: "UserServices",
                columns: new[] { "userId", "serviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserServices_userId1",
                table: "UserServices",
                column: "userId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailActions");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "TwoFactorAuthEntries");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "UserServices");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
