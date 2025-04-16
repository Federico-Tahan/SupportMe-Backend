using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportMe.Migrations
{
    /// <inheritdoc />
    public partial class paymentinclude : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetail_CampaignId",
                table: "PaymentDetail",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDetail_Campaign_CampaignId",
                table: "PaymentDetail",
                column: "CampaignId",
                principalTable: "Campaign",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDetail_Campaign_CampaignId",
                table: "PaymentDetail");

            migrationBuilder.DropIndex(
                name: "IX_PaymentDetail_CampaignId",
                table: "PaymentDetail");
        }
    }
}
