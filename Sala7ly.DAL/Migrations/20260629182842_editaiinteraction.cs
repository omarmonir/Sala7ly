using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sala7ly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class editaiinteraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AI_INTERACTIONS_AspNetUsers_UserId1",
                table: "AI_INTERACTIONS");

            migrationBuilder.DropIndex(
                name: "IX_AI_INTERACTIONS_UserId1",
                table: "AI_INTERACTIONS");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "AI_INTERACTIONS");

            migrationBuilder.AlterColumn<string>(
                name: "ResolvedByAdminId",
                table: "Disputes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InitiatedByUserId",
                table: "Disputes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ResolvedByAdminId",
                table: "Disputes",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InitiatedByUserId",
                table: "Disputes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "AI_INTERACTIONS",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AI_INTERACTIONS_UserId1",
                table: "AI_INTERACTIONS",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AI_INTERACTIONS_AspNetUsers_UserId1",
                table: "AI_INTERACTIONS",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
