using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sala7ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class sdadasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "TechnicianPortfolios",
                newName: "ImageUrlBefore");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "TechnicianPortfolios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlAfter",
                table: "TechnicianPortfolios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrlAfter",
                table: "TechnicianPortfolios");

            migrationBuilder.RenameColumn(
                name: "ImageUrlBefore",
                table: "TechnicianPortfolios",
                newName: "ImageUrl");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "TechnicianPortfolios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
