using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rishvi.Migrations
{
    /// <inheritdoc />
    public partial class updateadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    temp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Authorizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegratedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsConfigActive = table.Column<bool>(type: "bit", nullable: true),
                    ConfigStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhoneNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    County = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabelReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinnworksUniqueIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorizationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    access_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expires_in = table.Column<int>(type: "int", nullable: true),
                    refresh_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    refresh_token_expires_in = table.Column<int>(type: "int", nullable: true),
                    token_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FtpHost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FtpUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FtpPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FtpPort = table.Column<int>(type: "int", nullable: true),
                    LinnworksToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinnworksServer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinnRefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fulfiilmentLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartyFileCreated = table.Column<bool>(type: "bit", nullable: true),
                    AutoOrderSync = table.Column<bool>(type: "bit", nullable: false),
                    AutoOrderDespatchSync = table.Column<bool>(type: "bit", nullable: false),
                    UseDefaultLocation = table.Column<bool>(type: "bit", nullable: false),
                    DefaultLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinnDays = table.Column<int>(type: "int", nullable: false),
                    LinnPage = table.Column<int>(type: "int", nullable: false),
                    SendChangeToStream = table.Column<bool>(type: "bit", nullable: false),
                    HandsOnDate = table.Column<bool>(type: "bit", nullable: false),
                    ShippingApiConfigId = table.Column<int>(type: "int", nullable: false),
                    IsLiveAccount = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authorizations", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "ErrorLog",
                columns: table => new
                {
                    ErrorLogID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GetDate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GetDate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLog", x => x.ErrorLogID);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    event_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_code_desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_time = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    event_link = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.id);
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
                name: "PostalServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorizationToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalServiceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GetDate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GetDate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportModel",
                columns: table => new
                {
                    _id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthorizationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinnNumOrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EbayChannelOrderRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderLineItemId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreamOrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreamConsignmentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreamTrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreamTrackingURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DownloadLinnOrderInSystem = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DownloadEbayOrderInSystem = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispatchEbayOrderInStream = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispatchEbayOrderFromStream = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateLinnOrderInStream = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateLinnOrderForStream = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispatchLinnOrderFromStream = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispatchLinnOrderInStream = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateEbayOrderInStream = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsLinnOrderCreatedInStream = table.Column<bool>(type: "bit", nullable: false),
                    IsEbayOrderCreatedInStream = table.Column<bool>(type: "bit", nullable: true),
                    IsLinnOrderDispatchInStream = table.Column<bool>(type: "bit", nullable: true),
                    IsEbayOrderDispatchInStream = table.Column<bool>(type: "bit", nullable: true),
                    IsLinnOrderDispatchFromStream = table.Column<bool>(type: "bit", nullable: false),
                    IsEbayOrderDispatchFromStream = table.Column<bool>(type: "bit", nullable: false),
                    EbayOrderDetaailJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinnOrderDetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreamOrderCreateJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DispatchOrderInEbayJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DispatchOrderInLinnJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportModel", x => x._id);
                });

            migrationBuilder.CreateTable(
                name: "Runs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    loadId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runs", x => x.id);
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
                name: "StreamOrderRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorizationToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EbayOrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinnworksOrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsignmentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrackingUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrackingId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamOrderRecord", x => x.Id);
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
                name: "Subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    party_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    @event = table.Column<string>(name: "event", type: "nvarchar(max)", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    url_path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    http_method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.id);
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
                    UpdateLinnworksOrderToStream = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemLog",
                columns: table => new
                {
                    SystemLogID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModuleName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestHeader = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsError = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GetDate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GetDate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLog", x => x.SystemLogID);
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
                name: "WebhookOrders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    order = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookOrders", x => x.id);
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
                    EbayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastSyncOnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSyncOn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ebaypage = table.Column<int>(type: "int", nullable: false),
                    ebayhour = table.Column<int>(type: "int", nullable: false),
                    linnpage = table.Column<int>(type: "int", nullable: false),
                    linnhour = table.Column<int>(type: "int", nullable: false),
                    SyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationSettings_Ebay_EbayId",
                        column: x => x.EbayId,
                        principalTable: "Ebay",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumOrderId = table.Column<int>(type: "int", nullable: false),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
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
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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
                name: "IX_CustomerInfo_AddressId",
                table: "CustomerInfo",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInfo_BillingAddressId",
                table: "CustomerInfo",
                column: "BillingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationSettings_EbayId",
                table: "IntegrationSettings",
                column: "EbayId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Item_ItemId1",
                table: "Item",
                column: "ItemId1");

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
                name: "Authorizations");

            migrationBuilder.DropTable(
                name: "ErrorLog");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "IntegrationSettings");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "PostalServices");

            migrationBuilder.DropTable(
                name: "ReportModel");

            migrationBuilder.DropTable(
                name: "Runs");

            migrationBuilder.DropTable(
                name: "StreamOrderRecord");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "SystemLog");

            migrationBuilder.DropTable(
                name: "WebhookOrders");

            migrationBuilder.DropTable(
                name: "Ebay");

            migrationBuilder.DropTable(
                name: "LinnworksSettings");

            migrationBuilder.DropTable(
                name: "StreamSettings");

            migrationBuilder.DropTable(
                name: "SyncSettings");

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
        }
    }
}
