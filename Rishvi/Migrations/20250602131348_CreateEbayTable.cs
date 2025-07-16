using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rishvi.Migrations
{
    /// <inheritdoc />
    public partial class CreateEbayTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EbayId",
                table: "IntegrationSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Ebay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownloadOrderFromEbay = table.Column<bool>(type: "bit", nullable: false),
                    SendOrderToStream = table.Column<bool>(type: "bit", nullable: false),
                    UpdateInformationFromEbayToStream = table.Column<bool>(type: "bit", nullable: false),
                    DispatchOrderFromEbay = table.Column<bool>(type: "bit", nullable: false),
                    UpdateTrackingDetailsFromStream = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ebay", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationSettings_EbayId",
                table: "IntegrationSettings",
                column: "EbayId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntegrationSettings_Ebay_EbayId",
                table: "IntegrationSettings",
                column: "EbayId",
                principalTable: "Ebay",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntegrationSettings_Ebay_EbayId",
                table: "IntegrationSettings");

            migrationBuilder.DropTable(
                name: "Ebay");

            migrationBuilder.DropIndex(
                name: "IX_IntegrationSettings_EbayId",
                table: "IntegrationSettings");

            migrationBuilder.DropColumn(
                name: "EbayId",
                table: "IntegrationSettings");
        }
    }
}
