using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rishvi.Migrations
{
    /// <inheritdoc />
    public partial class RegsiterService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LinnworksSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownloadOrderFromStream = table.Column<bool>(type: "bit", nullable: false),
                    DownloadOrderFromEbay = table.Column<bool>(type: "bit", nullable: false),
                    PrintLabelFromStream = table.Column<bool>(type: "bit", nullable: false),
                    PrintLabelFromLinnworks = table.Column<bool>(type: "bit", nullable: false),
                    DispatchOrderFromStream = table.Column<bool>(type: "bit", nullable: false),
                    DispatchOrderFromEbay = table.Column<bool>(type: "bit", nullable: false),
                    SendChangeToEbay = table.Column<bool>(type: "bit", nullable: false),
                    SendChangeToStream = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinnworksSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StreamSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GetTrackingDetails = table.Column<bool>(type: "bit", nullable: false),
                    EnableWebhook = table.Column<bool>(type: "bit", nullable: false),
                    SendChangeFromLinnworksToStream = table.Column<bool>(type: "bit", nullable: false),
                    SendChangesFromEbayToStream = table.Column<bool>(type: "bit", nullable: false),
                    CreateProductToStream = table.Column<bool>(type: "bit", nullable: false),
                    DownloadProductFromStreamToLinnworks = table.Column<bool>(type: "bit", nullable: false),
                    GetRoutePlanFromStream = table.Column<bool>(type: "bit", nullable: false),
                    GetDepotListFromStream = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SyncEbayOrder = table.Column<bool>(type: "bit", nullable: false),
                    SyncLinnworksOrder = table.Column<bool>(type: "bit", nullable: false),
                    CreateEbayOrderToStream = table.Column<bool>(type: "bit", nullable: false),
                    CreateLinnworksOrderToStream = table.Column<bool>(type: "bit", nullable: false),
                    DispatchLinnworksOrderFromStream = table.Column<bool>(type: "bit", nullable: false),
                    DispatchEbayOrderFromStream = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorizationToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinnworksSyncToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinnworksId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StreamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastSyncOnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSyncOn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ebaypage = table.Column<int>(type: "int", nullable: false),
                    ebayhour = table.Column<int>(type: "int", nullable: false),
                    linnpage = table.Column<int>(type: "int", nullable: false),
                    linnhour = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationSettings_LinnworksSettings_LinnworksId",
                        column: x => x.LinnworksId,
                        principalTable: "LinnworksSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntegrationSettings_StreamSettings_StreamId",
                        column: x => x.StreamId,
                        principalTable: "StreamSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntegrationSettings_SyncSettings_SyncId",
                        column: x => x.SyncId,
                        principalTable: "SyncSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationSettings_LinnworksId",
                table: "IntegrationSettings",
                column: "LinnworksId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationSettings_StreamId",
                table: "IntegrationSettings",
                column: "StreamId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationSettings_SyncId",
                table: "IntegrationSettings",
                column: "SyncId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationSettings");

            migrationBuilder.DropTable(
                name: "LinnworksSettings");

            migrationBuilder.DropTable(
                name: "StreamSettings");

            migrationBuilder.DropTable(
                name: "SyncSettings");
        }
    }
}
