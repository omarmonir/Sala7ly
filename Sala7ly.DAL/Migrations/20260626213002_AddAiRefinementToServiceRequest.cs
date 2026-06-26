using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sala7ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAiRefinementToServiceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiRefinementJson",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiSuggestedCategoryId",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiSummary",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeChargeId",
                table: "EscrowTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiRefinementJson",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "AiSuggestedCategoryId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "AiSummary",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "StripeChargeId",
                table: "EscrowTransactions");
        }
    }
}
