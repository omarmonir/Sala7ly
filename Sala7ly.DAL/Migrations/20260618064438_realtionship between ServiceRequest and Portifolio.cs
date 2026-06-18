using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sala7ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class realtionshipbetweenServiceRequestandPortifolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechnicianPortfolios_ServiceRequests_ServiceRequestId",
                table: "TechnicianPortfolios");

            migrationBuilder.DropIndex(
                name: "IX_TechnicianPortfolios_ServiceRequestId",
                table: "TechnicianPortfolios");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "TechnicianPortfolios");

            migrationBuilder.DropColumn(
                name: "ServiceRequestId",
                table: "TechnicianPortfolios");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TechnicianPortfolios");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TechnicianPortfolios",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TechnicianPortfolios",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "TechnicianPortfolios");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TechnicianPortfolios");

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "TechnicianPortfolios",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ServiceRequestId",
                table: "TechnicianPortfolios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "TechnicianPortfolios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianPortfolios_ServiceRequestId",
                table: "TechnicianPortfolios",
                column: "ServiceRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_TechnicianPortfolios_ServiceRequests_ServiceRequestId",
                table: "TechnicianPortfolios",
                column: "ServiceRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
