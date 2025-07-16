using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rishvi.Migrations
{
    /// <inheritdoc />
    public partial class FixCustomerInfoCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SystemLog",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResponseJson",
                table: "SystemLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RequestJson",
                table: "SystemLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RequestHeader",
                table: "SystemLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModuleName",
                table: "SystemLog",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "SystemLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ErrorLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ErrorLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Town = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Continent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fulfillment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FulfillmentState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PurchaseOrderState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fulfillment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneralInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    LabelPrinted = table.Column<bool>(type: "bit", nullable: true),
                    LabelError = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoicePrinted = table.Column<bool>(type: "bit", nullable: true),
                    PickListPrinted = table.Column<bool>(type: "bit", nullable: true),
                    IsRuleRun = table.Column<bool>(type: "bit", nullable: true),
                    Notes = table.Column<int>(type: "int", nullable: true),
                    PartShipped = table.Column<bool>(type: "bit", nullable: true),
                    Marker = table.Column<int>(type: "int", nullable: true),
                    IsParked = table.Column<bool>(type: "bit", nullable: true),
                    ReferenceNum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecondaryReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalReferenceNum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoldOrCancel = table.Column<bool>(type: "bit", nullable: true),
                    DespatchByDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasScheduledDelivery = table.Column<bool>(type: "bit", nullable: true),
                    Location = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumItems = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingInfo",
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
                    ManualAdjust = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingInfo", x => x.ShippingId);
                });

            migrationBuilder.CreateTable(
                name: "TaxInfo",
                columns: table => new
                {
                    TaxInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxInfo", x => x.TaxInfoId);
                });

            migrationBuilder.CreateTable(
                name: "TotalsInfo",
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
                    ConversionRate = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TotalsInfo", x => x.TotalsInfoId);
                });

            migrationBuilder.CreateTable(
                name: "CustomerInfo",
                columns: table => new
                {
                    CustomerInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelBuyerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerInfo", x => x.CustomerInfoId);
                    table.ForeignKey(
                        name: "FK_CustomerInfo_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerInfo_Address_BillingAddressId",
                        column: x => x.BillingAddressId,
                        principalTable: "Address",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumOrderId = table.Column<int>(type: "int", nullable: true),
                    GeneralInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShippingInfoShippingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalsInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FolderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPostFilteredOut = table.Column<bool>(type: "bit", nullable: true),
                    CanFulfil = table.Column<bool>(type: "bit", nullable: true),
                    FulfillmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasItems = table.Column<bool>(type: "bit", nullable: true),
                    TotalItemsSum = table.Column<int>(type: "int", nullable: true),
                    TempColumn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_CustomerInfo_CustomerInfoId",
                        column: x => x.CustomerInfoId,
                        principalTable: "CustomerInfo",
                        principalColumn: "CustomerInfoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Fulfillment_FulfillmentId",
                        column: x => x.FulfillmentId,
                        principalTable: "Fulfillment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_GeneralInfo_GeneralInfoId",
                        column: x => x.GeneralInfoId,
                        principalTable: "GeneralInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_ShippingInfo_ShippingInfoShippingId",
                        column: x => x.ShippingInfoShippingId,
                        principalTable: "ShippingInfo",
                        principalColumn: "ShippingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_TaxInfo_TaxInfoId",
                        column: x => x.TaxInfoId,
                        principalTable: "TaxInfo",
                        principalColumn: "TaxInfoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_TotalsInfo_TotalsInfoId",
                        column: x => x.TotalsInfoId,
                        principalTable: "TotalsInfo",
                        principalColumn: "TotalsInfoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Item",
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
                    StockItemIntId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderRootOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Item_Orders_OrderRootOrderId",
                        column: x => x.OrderRootOrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInfo_AddressId",
                table: "CustomerInfo",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInfo_BillingAddressId",
                table: "CustomerInfo",
                column: "BillingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_OrderRootOrderId",
                table: "Item",
                column: "OrderRootOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerInfoId",
                table: "Orders",
                column: "CustomerInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FulfillmentId",
                table: "Orders",
                column: "FulfillmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GeneralInfoId",
                table: "Orders",
                column: "GeneralInfoId");

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
                name: "Item");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "CustomerInfo");

            migrationBuilder.DropTable(
                name: "Fulfillment");

            migrationBuilder.DropTable(
                name: "GeneralInfo");

            migrationBuilder.DropTable(
                name: "ShippingInfo");

            migrationBuilder.DropTable(
                name: "TaxInfo");

            migrationBuilder.DropTable(
                name: "TotalsInfo");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SystemLog",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "ResponseJson",
                table: "SystemLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RequestJson",
                table: "SystemLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RequestHeader",
                table: "SystemLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ModuleName",
                table: "SystemLog",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "SystemLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ErrorLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ErrorLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
