using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rishvi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateItemTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StockLevelsSpecified = table.Column<bool>(type: "bit", nullable: true),
                    OnOrder = table.Column<int>(type: "int", nullable: true),
                    InOrderBook = table.Column<int>(type: "int", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: true),
                    MinimumLevel = table.Column<int>(type: "int", nullable: true),
                    AvailableStock = table.Column<int>(type: "int", nullable: true),
                    PricePerUnit = table.Column<double>(type: "float", nullable: true),
                    UnitCost = table.Column<double>(type: "float", nullable: true),
                    Cost = table.Column<double>(type: "float", nullable: true),
                    CostIncTax = table.Column<double>(type: "float", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    BarcodeNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChannelSKU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChannelTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BinRack = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StockItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StockItemIntId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ItemId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderRootOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Item_ItemId1",
                        column: x => x.ItemId1,
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Item_Orders_OrderRootOrderId",
                        column: x => x.OrderRootOrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });
            
            migrationBuilder.CreateIndex(
                name: "IX_Item_ItemId1",
                table: "Item",
                column: "ItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_Item_OrderRootOrderId",
                table: "Item",
                column: "OrderRootOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item");
        }
    }
}
