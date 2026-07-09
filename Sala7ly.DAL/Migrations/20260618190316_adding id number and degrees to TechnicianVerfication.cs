using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sala7ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addingidnumberanddegreestoTechnicianVerfication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DegreeCertificateUrls",
                table: "TechnicianVerifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                table: "TechnicianVerifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DegreeCertificateUrls",
                table: "TechnicianVerifications");

            migrationBuilder.DropColumn(
                name: "IdNumber",
                table: "TechnicianVerifications");
        }
    }
}
