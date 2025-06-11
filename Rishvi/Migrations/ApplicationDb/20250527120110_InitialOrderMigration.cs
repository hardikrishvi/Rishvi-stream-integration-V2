using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rishvi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class InitialOrderMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shippings",
                columns: table => new
                {
                    ShippingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostalServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalWeight = table.Column<double>(type: "float", nullable: true),
                    ItemWeight = table.Column<double>(type: "float", nullable: true),
                    PackageCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostageCost = table.Column<double>(type: "float", nullable: true),
                    PostageCostExTax = table.Column<double>(type: "float", nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManualAdjust = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shippings", x => x.ShippingId);
                });

            migrationBuilder.CreateTable(
                name: "TaxInfo",
                columns: table => new
                {
                    TaxInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxInfo", x => x.TaxInfoId);
                });

            migrationBuilder.CreateTable(
                name: "Totals",
                columns: table => new
                {
                    TotalsInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subtotal = table.Column<double>(type: "float", nullable: true),
                    PostageCost = table.Column<double>(type: "float", nullable: true),
                    PostageCostExTax = table.Column<double>(type: "float", nullable: true),
                    Tax = table.Column<double>(type: "float", nullable: true),
                    TotalCharge = table.Column<double>(type: "float", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProfitMargin = table.Column<double>(type: "float", nullable: true),
                    TotalDiscount = table.Column<double>(type: "float", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryTaxRate = table.Column<double>(type: "float", nullable: true),
                    ConversionRate = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Totals", x => x.TotalsInfoId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumOrderId = table.Column<int>(type: "int", nullable: true),
                    GeneralInfo_Status = table.Column<int>(type: "int", nullable: true),
                    GeneralInfo_LabelPrinted = table.Column<bool>(type: "bit", nullable: true),
                    GeneralInfo_LabelError = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneralInfo_InvoicePrinted = table.Column<bool>(type: "bit", nullable: true),
                    GeneralInfo_PickListPrinted = table.Column<bool>(type: "bit", nullable: true),
                    GeneralInfo_IsRuleRun = table.Column<bool>(type: "bit", nullable: true),
                    GeneralInfo_Notes = table.Column<int>(type: "int", nullable: true),
                    GeneralInfo_PartShipped = table.Column<bool>(type: "bit", nullable: true),
                    GeneralInfo_Marker = table.Column<int>(type: "int", nullable: true),
                    GeneralInfo_IsParked = table.Column<bool>(type: "bit", nullable: true),
                    GeneralInfo_ReferenceNum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneralInfo_SecondaryReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneralInfo_ExternalReferenceNum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneralInfo_ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneralInfo_Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneralInfo_SubSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneralInfo_SiteCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneralInfo_HoldOrCancel = table.Column<bool>(type: "bit", nullable: true),
                    GeneralInfo_DespatchByDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneralInfo_HasScheduledDelivery = table.Column<bool>(type: "bit", nullable: true),
                    GeneralInfo_Location = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GeneralInfo_NumItems = table.Column<int>(type: "int", nullable: true),
                    ShippingInfoShippingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalsInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FolderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPostFilteredOut = table.Column<bool>(type: "bit", nullable: true),
                    CanFulfil = table.Column<bool>(type: "bit", nullable: true),
                    Fulfillment_FulfillmentState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fulfillment_PurchaseOrderState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasItems = table.Column<bool>(type: "bit", nullable: true),
                    TotalItemsSum = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_Shippings_ShippingInfoShippingId",
                        column: x => x.ShippingInfoShippingId,
                        principalTable: "Shippings",
                        principalColumn: "ShippingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_TaxInfo_TaxInfoId",
                        column: x => x.TaxInfoId,
                        principalTable: "TaxInfo",
                        principalColumn: "TaxInfoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Totals_TotalsInfoId",
                        column: x => x.TotalsInfoId,
                        principalTable: "Totals",
                        principalColumn: "TotalsInfoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    OrderRootOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelBuyerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Address2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Address3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Town = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_PostCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Continent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BillingAddress_EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_Address2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_Address3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_Town = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_PostCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_Continent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.OrderRootOrderId);
                    table.ForeignKey(
                        name: "FK_Customers_Orders_OrderRootOrderId",
                        column: x => x.OrderRootOrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    StockItemIntId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_Items_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemId",
                table: "Items",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_OrderId",
                table: "Items",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingInfoShippingId",
                table: "Orders",
                column: "ShippingInfoShippingId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TaxInfoId",
                table: "Orders",
                column: "TaxInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TotalsInfoId",
                table: "Orders",
                column: "TotalsInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Shippings");

            migrationBuilder.DropTable(
                name: "TaxInfo");

            migrationBuilder.DropTable(
                name: "Totals");
        }
    }
}
