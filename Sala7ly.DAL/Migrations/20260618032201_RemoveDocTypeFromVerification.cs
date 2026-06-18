using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sala7ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDocTypeFromVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocType",
                table: "TechnicianVerifications");

            migrationBuilder.RenameColumn(
                name: "DocumentUrl",
                table: "TechnicianVerifications",
                newName: "DocumentUrlFront");

            migrationBuilder.AddColumn<string>(
                name: "DocumentUrlBack",
                table: "TechnicianVerifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentUrlBack",
                table: "TechnicianVerifications");

            migrationBuilder.RenameColumn(
                name: "DocumentUrlFront",
                table: "TechnicianVerifications",
                newName: "DocumentUrl");

            migrationBuilder.AddColumn<string>(
                name: "DocType",
                table: "TechnicianVerifications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
