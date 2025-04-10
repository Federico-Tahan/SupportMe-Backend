using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportMe.Migrations
{
    /// <inheritdoc />
    public partial class MercadoPagoSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MercadopagoSetup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisbursementInitiative = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestEmailAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThreeDsMode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntegrationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CallBackUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestMode = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MercadopagoSetup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMercadoPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationSeconds = table.Column<int>(type: "int", nullable: false),
                    MPUserId = table.Column<int>(type: "int", nullable: false),
                    refresh_token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    public_key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    live_mode = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMercadoPago", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MercadopagoSetup");

            migrationBuilder.DropTable(
                name: "UserMercadoPago");
        }
    }
}
