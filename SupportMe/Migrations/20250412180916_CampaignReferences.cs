using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportMe.Migrations
{
    /// <inheritdoc />
    public partial class CampaignReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "GaleryAssets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CampaignTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CampaignId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignTags_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GaleryAssets_CampaignId",
                table: "GaleryAssets",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTags_CampaignId",
                table: "CampaignTags",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_GaleryAssets_Campaign_CampaignId",
                table: "GaleryAssets",
                column: "CampaignId",
                principalTable: "Campaign",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GaleryAssets_Campaign_CampaignId",
                table: "GaleryAssets");

            migrationBuilder.DropTable(
                name: "CampaignTags");

            migrationBuilder.DropIndex(
                name: "IX_GaleryAssets_CampaignId",
                table: "GaleryAssets");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "GaleryAssets");
        }
    }
}
