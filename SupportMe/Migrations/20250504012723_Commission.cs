using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportMe.Migrations
{
    /// <inheritdoc />
    public partial class Commission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MPCommission",
                table: "PaymentDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SupportmeCommission",
                table: "PaymentDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignView_CampaignId",
                table: "CampaignView",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignView_Campaign_CampaignId",
                table: "CampaignView",
                column: "CampaignId",
                principalTable: "Campaign",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignView_Campaign_CampaignId",
                table: "CampaignView");

            migrationBuilder.DropIndex(
                name: "IX_CampaignView_CampaignId",
                table: "CampaignView");

            migrationBuilder.DropColumn(
                name: "MPCommission",
                table: "PaymentDetail");

            migrationBuilder.DropColumn(
                name: "SupportmeCommission",
                table: "PaymentDetail");
        }
    }
}
