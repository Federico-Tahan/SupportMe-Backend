using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportMe.Migrations
{
    /// <inheritdoc />
    public partial class CampaignCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Campaign",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Campaign_CategoryId",
                table: "Campaign",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaign_Category_CategoryId",
                table: "Campaign",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaign_Category_CategoryId",
                table: "Campaign");

            migrationBuilder.DropIndex(
                name: "IX_Campaign_CategoryId",
                table: "Campaign");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Campaign");
        }
    }
}
