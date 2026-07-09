using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sala7ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class editAi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EmbeddingVector",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CommonComplaints",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmbeddingUpdatedAt",
                table: "TechnicianProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingVectorJson",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewSummary",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SentimentScore",
                table: "TechnicianProfiles",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeAccountId",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StripeOnboardingDone",
                table: "TechnicianProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SummaryUpdatedAt",
                table: "TechnicianProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TopStrengths",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommonComplaints",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "EmbeddingUpdatedAt",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "EmbeddingVectorJson",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "ReviewSummary",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "SentimentScore",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "StripeAccountId",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "StripeOnboardingDone",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "SummaryUpdatedAt",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "TopStrengths",
                table: "TechnicianProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "EmbeddingVector",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
