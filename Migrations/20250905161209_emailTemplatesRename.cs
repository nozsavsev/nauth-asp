using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nauth_asp.Migrations
{
    /// <inheritdoc />
    public partial class emailTemplatesRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "htmlBody",
                table: "EmailTemplates",
                newName: "HtmlBody");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HtmlBody",
                table: "EmailTemplates",
                newName: "htmlBody");
        }
    }
}
