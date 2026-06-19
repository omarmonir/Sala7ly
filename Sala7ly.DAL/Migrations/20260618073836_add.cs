using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sala7ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely drop the foreign key constraint if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_TechnicianPortfolios_ServiceRequests_ServiceRequestId'
                    AND parent_object_id = OBJECT_ID('TechnicianPortfolios')
                )
                BEGIN
                    ALTER TABLE [TechnicianPortfolios]
                    DROP CONSTRAINT [FK_TechnicianPortfolios_ServiceRequests_ServiceRequestId];
                END
            ");

            // Drop the ServiceRequestId column if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('TechnicianPortfolios')
                    AND name = 'ServiceRequestId'
                )
                BEGIN
                    ALTER TABLE [TechnicianPortfolios] DROP COLUMN [ServiceRequestId];
                END
            ");

            // Drop the Caption column if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('TechnicianPortfolios')
                    AND name = 'Caption'
                )
                BEGIN
                    ALTER TABLE [TechnicianPortfolios] DROP COLUMN [Caption];
                END
            ");

            // Drop the Type column if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('TechnicianPortfolios')
                    AND name = 'Type'
                )
                BEGIN
                    ALTER TABLE [TechnicianPortfolios] DROP COLUMN [Type];
                END
            ");

            // Add Description column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('TechnicianPortfolios')
                    AND name = 'Description'
                )
                BEGIN
                    ALTER TABLE [TechnicianPortfolios]
                    ADD [Description] nvarchar(1000) NULL;
                END
            ");

            // Add Title column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('TechnicianPortfolios')
                    AND name = 'Title'
                )
                BEGIN
                    ALTER TABLE [TechnicianPortfolios]
                    ADD [Title] nvarchar(150) NULL;
                END
            ");
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

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "TechnicianPortfolios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ServiceRequestId",
                table: "TechnicianPortfolios",
                type: "int",
                nullable: true);

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
