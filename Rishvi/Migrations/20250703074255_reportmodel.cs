using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rishvi.Migrations
{
    /// <inheritdoc />
    public partial class reportmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.CreateTable(
                name: "ReportModel",
                columns: table => new
                {
                    _id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthorizationToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinnNumOrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EbayChannelOrderRef = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderLineItemId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamOrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamConsignmentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamTrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamTrackingURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DownloadLinnOrderInSystem = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DownloadEbayOrderInSystem = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DispatchEbayOrderInStream = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DispatchEbayOrderFromStream = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateLinnOrderInStream = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateLinnOrderForStream = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DispatchLinnOrderFromStream = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DispatchLinnOrderInStream = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateEbayOrderInStream = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLinnOrderCreatedInStream = table.Column<bool>(type: "bit", nullable: false),
                    IsEbayOrderCreatedInStream = table.Column<bool>(type: "bit", nullable: false),
                    IsLinnOrderDispatchInStream = table.Column<bool>(type: "bit", nullable: false),
                    IsEbayOrderDispatchInStream = table.Column<bool>(type: "bit", nullable: false),
                    IsLinnOrderDispatchFromStream = table.Column<bool>(type: "bit", nullable: false),
                    IsEbayOrderDispatchFromStream = table.Column<bool>(type: "bit", nullable: false),
                    EbayOrderDetaailJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinnOrderDetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamOrderCreateJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DispatchOrderInEbayJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DispatchOrderInLinnJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportModel", x => x._id);
                });

            

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropTable(
                name: "ReportModel");

            
        }
    }
}
