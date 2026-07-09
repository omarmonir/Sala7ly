using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sala7ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class adddispute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EscrowTransactions_Disputes_DisputeId1",
                table: "EscrowTransactions");

            migrationBuilder.DropIndex(
                name: "IX_EscrowTransactions_DisputeId1",
                table: "EscrowTransactions");

            migrationBuilder.DropColumn(
                name: "DisputeId1",
                table: "EscrowTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_DisputeId",
                table: "EscrowTransactions",
                column: "DisputeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EscrowTransactions_Disputes_DisputeId",
                table: "EscrowTransactions",
                column: "DisputeId",
                principalTable: "Disputes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EscrowTransactions_Disputes_DisputeId",
                table: "EscrowTransactions");

            migrationBuilder.DropIndex(
                name: "IX_EscrowTransactions_DisputeId",
                table: "EscrowTransactions");

            migrationBuilder.AddColumn<int>(
                name: "DisputeId1",
                table: "EscrowTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_DisputeId1",
                table: "EscrowTransactions",
                column: "DisputeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EscrowTransactions_Disputes_DisputeId1",
                table: "EscrowTransactions",
                column: "DisputeId1",
                principalTable: "Disputes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
