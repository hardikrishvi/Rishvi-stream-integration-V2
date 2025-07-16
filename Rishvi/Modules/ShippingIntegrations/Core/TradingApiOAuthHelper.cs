using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using LinnworksAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rishvi.Models;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.ShippingIntegrations.Api;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using Address = Rishvi.Models.Address;
using Item = Rishvi.Modules.ShippingIntegrations.Models.Item;

namespace Rishvi.Modules.ShippingIntegrations.Core
{
    public class TradingApiOAuthHelper
    {
        //private readonly Guid _selectedServiceGuid = new Guid("6A476315-04DB-4D25-A25C-E6917A1BCAD9");
        private readonly ReportsController _reportsController;
        private readonly SetupController _setupController;
        private readonly ApplicationDbContext _dbContext;
        private readonly SqlContext _dbSqlCContext;
        private readonly IRepository<Address> _Address;
        private readonly IRepository<CustomerInfo> _CustomerInfo;
        private readonly IRepository<Fulfillment> _Fulfillment;
        private readonly IRepository<GeneralInfo> _GeneralInfo;
        private readonly IRepository<OrderRoot> _OrderRoot;
        private readonly IRepository<ShippingInfo> _ShippingInfo;
        private readonly IRepository<TaxInfo> _TaxInfo;
        private readonly IRepository<TotalsInfo> _TotalsInfo;
        private readonly IRepository<Rishvi.Models.Item> _Item;
        private readonly IRepository<IntegrationSettings> _IntegrationSettings;
        private readonly IRepository<LinnworksSettings> _LinnworksSettings;
        private readonly IRepository<StreamSettings> _StreamSettings;
        private readonly IRepository<SyncSettings> _SyncSettings;
        private readonly IRepository<Ebay> _Ebay;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ManageToken _manageToken;

        public TradingApiOAuthHelper(
             ReportsController reportsController,
             SetupController setupController,
             IOptions<CourierSettings> courierSettings,
             ApplicationDbContext dbContext,
             IUnitOfWork unitOfWork,
             IRepository<Address> address,
             IRepository<CustomerInfo> customerInfo,
             IRepository<Fulfillment> fulfillment,
             IRepository<GeneralInfo> generalInfo,
             IRepository<OrderRoot> orderRoot,
             IRepository<ShippingInfo> shippingInfo,
             IRepository<TaxInfo> taxInfo,
             IRepository<TotalsInfo> totalsInfo,
             IRepository<Rishvi.Models.Item> item,
             IRepository<IntegrationSettings> integrationSettings,
             IRepository<LinnworksSettings> linnworksSettings,
             IRepository<StreamSettings> streamSettings,
             IRepository<SyncSettings> syncSettings,
             IRepository<Ebay> ebay,
             SqlContext dbSqlCContext,
             ManageToken manageToken)
        {
            _reportsController = reportsController;
            _setupController = setupController;
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _Address = address;
            _CustomerInfo = customerInfo;
            _Fulfillment = fulfillment;
            _GeneralInfo = generalInfo;
            _OrderRoot = orderRoot;
            _ShippingInfo = shippingInfo;
            _TaxInfo = taxInfo;
            _TotalsInfo = totalsInfo;
            _Item = item;
            _IntegrationSettings = integrationSettings;
            _LinnworksSettings = linnworksSettings;
            _StreamSettings = streamSettings;
            _SyncSettings = syncSettings;
            _Ebay = ebay;
            _dbSqlCContext = dbSqlCContext;
            _manageToken = manageToken;
        }

        #region Dispatch / Stream helpers
        public async Task DispatchLinnOrdersFromStream(AuthorizationConfigClass user, string orderIds, string linnToken)
        {
            var orderList = orderIds.Split(',');
            foreach (var linnOrderId in orderList)
            {
                if (await AwsS3.S3FileIsExists("Authorization", $"LinnStreamOrder/_streamorder_{linnOrderId}.json"))
                {
                    var jsonData = AwsS3.GetS3File("Authorization", $"LinnStreamOrder/_streamorder_{linnOrderId}.json");
                    var streamData = JsonConvert.DeserializeObject<StreamOrderRespModel.Root>(jsonData);
                    if (streamData != null && int.TryParse(linnOrderId, out int parsedId))
                    {
                        await DispatchOrderInLinnworks(user, parsedId, "Stream", streamData.response.trackingId, streamData.response.trackingURL, linnToken, null);
                    }
                }
            }
        }

        public async Task<StreamGetOrderResponse.Root> GetStreamOrder(AuthorizationConfigClass user, string orderId)
        {
            var streamAuth = _manageToken.GetToken(user);
            return StreamOrderApi.GetOrder(streamAuth.AccessToken, orderId, user.ClientId);
        }
        #endregion

        #region Webhook subscription
        public async Task CreateStreamWebhook(AuthorizationConfigClass user, string eventName, string eventType, string urlPath, string httpMethod, string contentType, string authHeader)
        {
            await _setupController.SubscribeWebhook(user.AuthorizationToken, eventName, eventType, urlPath, httpMethod, contentType, authHeader);
        }
        #endregion

        #region Order saves
        public async Task SaveStreamOrder(string json, string authToken, string email, string ebayOrderId, string linnworksOrderId, string consignmentId, string trackingNumber, string trackingUrl, string order = "")
        {
            try
            {
                dynamic data = JsonConvert.DeserializeObject(json);
                string consignment = data?.response?.consignmentNo;
                string trackingId = data?.response?.trackingId;
                string tUrl = data?.response?.trackingURL;

                var record = new StreamOrderRecord
                {
                    Id = Guid.NewGuid(),
                    JsonData = json,
                    AuthorizationToken = authToken,
                    Email = email,
                    EbayOrderId = ebayOrderId ?? "0",
                    LinnworksOrderId = linnworksOrderId,
                    ConsignmentId = consignment ?? consignmentId,
                    TrackingNumber = trackingNumber ?? "",
                    TrackingUrl = tUrl ?? trackingUrl,
                    TrackingId = trackingId ?? "0",
                    Order = order,
                    CreatedAt = DateTime.UtcNow
                };
                _dbSqlCContext.StreamOrderRecord.Add(record);
                await _dbSqlCContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SaveStreamOrder error: {ex.Message}");
            }
        }
        #endregion

        #region Linnworks
        public async Task DispatchOrderInLinnworks(AuthorizationConfigClass user, int orderRef, string linnToken, string service, string trackingNumber, string trackingUrl, string dispatchDate)
        {
            var obj = new LinnworksBaseStream(linnToken);
            var order = obj.Api.Orders.GetOrderDetailsByNumOrderId(orderRef);
            var generalInfo = order.GeneralInfo;
            generalInfo.DespatchByDate = string.IsNullOrEmpty(dispatchDate) ? DateTime.Now : DateTime.Parse(dispatchDate);
            obj.Api.Orders.SetOrderGeneralInfo(order.OrderId, generalInfo, false);

            await SaveLinnDispatch(JsonConvert.SerializeObject(order), user.AuthorizationToken.ToString(), user.Email, orderRef);
        }

        public async Task SaveLinnDispatch(string json, string authToken, string email, int linnOrderId)
        {
            InsertOrderFromJson(json);
            var report = _dbSqlCContext.ReportModel.FirstOrDefault(r => r.LinnNumOrderId == linnOrderId.ToString() && r.email == email);
            if (report == null)
            {
                _dbSqlCContext.ReportModel.Add(new ReportModel
                {
                    _id = Guid.NewGuid().ToString(),
                    AuthorizationToken = authToken,
                    DispatchLinnOrderFromStream = DateTime.Now,
                    IsLinnOrderDispatchFromStream = true,
                    DispatchOrderInLinnJson = $"LinnDispatch/{authToken}_linndispatch_{linnOrderId}.json",
                    LinnNumOrderId = linnOrderId.ToString(),
                    email = email,
                    createdDate = DateTime.Now,
                    updatedDate = DateTime.Now
                });
            }
            else
            {
                report.DispatchLinnOrderFromStream = DateTime.Now;
                report.IsLinnOrderDispatchFromStream = true;
                report.DispatchOrderInLinnJson = $"LinnDispatch/{authToken}_linndispatch_{linnOrderId}.json";
                report.updatedDate = DateTime.Now;
            }
            await _dbSqlCContext.SaveChangesAsync();
        }
        #endregion

        public async Task SaveLinnOrder(string json, string authToken, string email, string linnOrderId)
        {
            try
            {
                dynamic jsonData = JsonConvert.DeserializeObject(json);
                string extractedConsignmentNo = jsonData.response.consignmentNo;
                string extractedTrackingUrl = jsonData.response.trackingURL;
                string extractedTrackingId = jsonData.response.trackingId;
                string trackingNo = jsonData.response.trackingNo;
                // Insert into your relational DB
                InsertOrderFromJson(json);

                // Update or insert in ReportModel
                var report = _dbSqlCContext.ReportModel
                    .FirstOrDefault(r => r.LinnNumOrderId == linnOrderId && r.email == email);

                if (report == null)
                {
                    _dbSqlCContext.ReportModel.Add(new ReportModel
                    {
                        _id = Guid.NewGuid().ToString(),
                        AuthorizationToken = authToken,
                        StreamOrderId = linnOrderId,
                        StreamConsignmentId = extractedConsignmentNo,
                        StreamTrackingNumber = trackingNo,
                        StreamTrackingURL = extractedTrackingUrl,
                        EbayChannelOrderRef = linnOrderId,
                        IsEbayOrderCreatedInStream = true,
                        DownloadEbayOrderInSystem = DateTime.Now,
                        CreateEbayOrderInStream = DateTime.Now,
                        email = email,
                        StreamOrderCreateJson = $"UserStreamOrder/{authToken}_streamorder_{linnOrderId}.json",
                        createdDate = DateTime.Now,
                        updatedDate = DateTime.Now
                    });
                }
                else
                {
                    report.StreamOrderCreateJson = $"LinnOrder/{authToken}_linnorder_{linnOrderId}.json";
                    report.updatedDate = DateTime.Now;
                }

                await _dbSqlCContext.SaveChangesAsync();
                Console.WriteLine($"✅ SaveLinnOrder completed for order {linnOrderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SaveLinnOrder failed: {ex.Message}");
            }
        }


        #region Save helpers
        public void InsertOrderFromJson(string json)
        {
            try
            {
                var root = JsonConvert.DeserializeObject<OrderRoot>(json);

                // Create related entities
                var address = new Address
                {
                    Id = Guid.NewGuid(),
                    EmailAddress = root.CustomerInfo?.Address?.EmailAddress ?? "",
                    Address1 = root.CustomerInfo?.Address?.Address1 ?? "",
                    Address2 = root.CustomerInfo?.Address?.Address2 ?? "",
                    Address3 = root.CustomerInfo?.Address?.Address3 ?? "",
                    Town = root.CustomerInfo?.Address?.Town ?? "",
                    Region = root.CustomerInfo?.Address?.Region ?? "",
                    PostCode = root.CustomerInfo?.Address?.PostCode ?? "",
                    Country = root.CustomerInfo?.Address?.Country ?? "",
                    Continent = root.CustomerInfo?.Address?.Continent ?? "N/A",
                    FullName = root.CustomerInfo?.Address?.FullName ?? "",
                    Company = root.CustomerInfo?.Address?.Company ?? "",
                    PhoneNumber = root.CustomerInfo?.Address?.PhoneNumber ?? "",
                    CountryId = root.CustomerInfo?.Address?.CountryId == Guid.Empty
                                ? Guid.NewGuid()
                                : root.CustomerInfo?.Address?.CountryId,
                    temp = "placeholder",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                var billingAddress = new Address
                {
                    Id = Guid.NewGuid(),
                    EmailAddress = root.CustomerInfo?.BillingAddress?.EmailAddress ?? "",
                    Address1 = root.CustomerInfo?.BillingAddress?.Address1 ?? "",
                    Address2 = root.CustomerInfo?.BillingAddress?.Address2 ?? "",
                    Address3 = root.CustomerInfo?.BillingAddress?.Address3 ?? "",
                    Town = root.CustomerInfo?.BillingAddress?.Town ?? "",
                    Region = root.CustomerInfo?.BillingAddress?.Region ?? "",
                    PostCode = root.CustomerInfo?.BillingAddress?.PostCode ?? "",
                    Country = root.CustomerInfo?.BillingAddress?.Country ?? "",
                    Continent = root.CustomerInfo?.BillingAddress?.Continent ?? "N/A",
                    FullName = root.CustomerInfo?.BillingAddress?.FullName ?? "",
                    Company = root.CustomerInfo?.BillingAddress?.Company ?? "",
                    PhoneNumber = root.CustomerInfo?.BillingAddress?.PhoneNumber ?? "",
                    CountryId = root.CustomerInfo?.BillingAddress?.CountryId == Guid.Empty
                                ? Guid.NewGuid()
                                : root.CustomerInfo?.BillingAddress?.CountryId,
                    temp = "placeholder",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                var customerInfo = new CustomerInfo
                {
                    CustomerInfoId = Guid.NewGuid(),
                    ChannelBuyerName = root.CustomerInfo?.ChannelBuyerName ?? string.Empty,
                    AddressId = address.Id,
                    Address = address,
                    BillingAddressId = billingAddress.Id,
                    BillingAddress = billingAddress,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                var generalInfo = root.GeneralInfo ?? new GeneralInfo
                {
                    Id = Guid.NewGuid(),
                    Status = 0
                };
                generalInfo.Id = Guid.NewGuid();
                generalInfo.CreatedAt = DateTime.UtcNow;
                generalInfo.UpdatedAt = null;

                var shippingInfo = root.ShippingInfo ?? new ShippingInfo();
                shippingInfo.ShippingId = Guid.NewGuid();
                shippingInfo.CreatedAt = DateTime.UtcNow;
                shippingInfo.UpdatedAt = null;

                var totalsInfo = root.TotalsInfo ?? new TotalsInfo();
                totalsInfo.TotalsInfoId = Guid.NewGuid();
                totalsInfo.CreatedAt = DateTime.UtcNow;
                totalsInfo.UpdatedAt = null;

                var taxInfo = root.TaxInfo ?? new TaxInfo();
                taxInfo.TaxInfoId = Guid.NewGuid();
                taxInfo.CreatedAt = DateTime.UtcNow;
                taxInfo.UpdatedAt = null;

                var fulfillment = root.Fulfillment ?? new Fulfillment();
                fulfillment.Id = Guid.NewGuid();
                fulfillment.CreatedAt = DateTime.UtcNow;
                fulfillment.UpdatedAt = null;

                // Build order root
                var order = new OrderRoot
                {
                    OrderId = root.OrderId != Guid.Empty ? root.OrderId : Guid.NewGuid(),
                    NumOrderId = root.NumOrderId,
                    GeneralInfo = generalInfo,
                    ShippingInfo = shippingInfo,
                    CustomerInfo = customerInfo,
                    TotalsInfo = totalsInfo,
                    TaxInfo = taxInfo,
                    Fulfillment = fulfillment,
                    FolderName = root.FolderName ?? new List<string>(),
                    IsPostFilteredOut = root.IsPostFilteredOut ?? false,
                    CanFulfil = root.CanFulfil ?? false,
                    HasItems = root.HasItems ?? false,
                    TotalItemsSum = root.TotalItemsSum ?? 0,
                    TempColumn = "placeholder",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                var items = new List<Rishvi.Models.Item>();
                foreach (var i in root.Items ?? new List<Rishvi.Models.Item>())
                {
                    var item = new Rishvi.Models.Item
                    {
                        Id = Guid.NewGuid(),
                        ItemId = i.ItemId ?? "",
                        ItemNumber = i.ItemNumber ?? "",
                        SKU = i.SKU ?? "",
                        Title = i.Title ?? "",
                        Quantity = i.Quantity,
                        CategoryName = i.CategoryName ?? "",
                        StockLevelsSpecified = i.StockLevelsSpecified ?? false,
                        OnOrder = i.OnOrder ?? 0,
                        InOrderBook = i.InOrderBook ?? 0,
                        Level = i.Level ?? 0,
                        MinimumLevel = i.MinimumLevel ?? 0,
                        AvailableStock = i.AvailableStock ?? 0,
                        PricePerUnit = i.PricePerUnit ?? 0,
                        UnitCost = i.UnitCost ?? 0,
                        Cost = i.Cost ?? 0,
                        CostIncTax = i.CostIncTax ?? 0,
                        Weight = i.Weight ?? 0,
                        BarcodeNumber = i.BarcodeNumber ?? "",
                        ChannelSKU = i.ChannelSKU ?? "",
                        ChannelTitle = i.ChannelTitle ?? "",
                        BinRack = i.BinRack ?? "",
                        ImageId = i.ImageId ?? "",
                        RowId = i.RowId ?? Guid.NewGuid(),
                        OrderId = order.OrderId,
                        StockItemId = i.StockItemId,
                        StockItemIntId = i.StockItemIntId ?? 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = null,
                        CompositeSubItems = new List<Rishvi.Models.Item>()
                    };

                    foreach (var c in i.CompositeSubItems ?? new List<Rishvi.Models.Item>())
                    {
                        var subItem = new Rishvi.Models.Item
                        {
                            Id = Guid.NewGuid(),
                            ItemId = c.ItemId ?? "",
                            ItemNumber = c.ItemNumber ?? "",
                            SKU = c.SKU ?? "",
                            Title = c.Title ?? "",
                            Quantity = c.Quantity,
                            CategoryName = c.CategoryName ?? "",
                            StockLevelsSpecified = c.StockLevelsSpecified ?? false,
                            OnOrder = c.OnOrder ?? 0,
                            InOrderBook = c.InOrderBook ?? 0,
                            Level = c.Level ?? 0,
                            MinimumLevel = c.MinimumLevel ?? 0,
                            AvailableStock = c.AvailableStock ?? 0,
                            PricePerUnit = c.PricePerUnit ?? 0,
                            UnitCost = c.UnitCost ?? 0,
                            Cost = c.Cost ?? 0,
                            CostIncTax = c.CostIncTax ?? 0,
                            Weight = c.Weight ?? 0,
                            BarcodeNumber = c.BarcodeNumber ?? "",
                            ChannelSKU = c.ChannelSKU ?? "",
                            ChannelTitle = c.ChannelTitle ?? "",
                            BinRack = c.BinRack ?? "",
                            ImageId = c.ImageId ?? "",
                            RowId = c.RowId ?? Guid.NewGuid(),
                            OrderId = order.OrderId,
                            StockItemId = c.StockItemId,
                            StockItemIntId = c.StockItemIntId ?? 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = null
                        };
                        item.CompositeSubItems.Add(subItem);
                        items.Add(subItem);
                    }
                    items.Add(item);
                }

                _Address.Add(address);
                _Address.Add(billingAddress);
                _CustomerInfo.Add(customerInfo);
                _GeneralInfo.Add(generalInfo);
                _ShippingInfo.Add(shippingInfo);
                _TotalsInfo.Add(totalsInfo);
                _TaxInfo.Add(taxInfo);
                _Fulfillment.Add(fulfillment);
                _OrderRoot.Add(order);
                _Item.AddRange(items);

                _unitOfWork.Context.SaveChanges();
                Console.WriteLine("✅ Order inserted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ InsertOrderFromJson failed: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"🔍 Inner: {ex.InnerException.Message}");
            }
        }
        #endregion

        public void RegisterSave(string s, string AuthorizationToken, string email = "", string token = "")
        {
            RegisterSaveFromJson(s).GetAwaiter().GetResult();
        }

        public async Task RegisterSaveFromJson(string json)
        {
            try
            {
                var root = JsonConvert.DeserializeObject<RegistrationData>(json);
                var linnworks = new LinnworksSettings
                {
                    Id = Guid.NewGuid(),
                    DownloadOrderFromStream = root.Linnworks?.DownloadOrderFromStream ?? false,
                    DownloadOrderFromEbay = root.Linnworks?.DownloadOrderFromEbay ?? false,
                    PrintLabelFromStream = root.Linnworks?.PrintLabelFromStream ?? false,
                    PrintLabelFromLinnworks = root.Linnworks?.PrintLabelFromLinnworks ?? false,
                    DispatchOrderFromStream = root.Linnworks?.DispatchOrderFromStream ?? false,
                    DispatchOrderFromEbay = root.Linnworks?.DispatchOrderFromEbay ?? false,
                    SendChangeToEbay = root.Linnworks?.SendChangeToEbay ?? false,
                    SendChangeToStream = root.Linnworks?.SendChangeToStream ?? false,
                    CreatedAt = DateTime.UtcNow
                };

                var stream = new StreamSettings
                {
                    Id = Guid.NewGuid(),
                    GetTrackingDetails = root.Stream?.GetTrackingDetails ?? false,
                    EnableWebhook = root.Stream?.EnableWebhook ?? false,
                    SendChangeFromLinnworksToStream = root.Stream?.SendChangeFromLinnworksToStream ?? false,
                    SendChangesFromEbayToStream = root.Stream?.SendChangesFromEbayToStream ?? false,
                    CreateProductToStream = root.Stream?.CreateProductToStream ?? false,
                    DownloadProductFromStreamToLinnworks = root.Stream?.DownloadProductFromStreamToLinnworks ?? false,
                    GetRoutePlanFromStream = root.Stream?.GetRoutePlanFromStream ?? false,
                    GetDepotListFromStream = root.Stream?.GetDepotListFromStream ?? false,
                    CreatedAt = DateTime.UtcNow
                };

                var sync = new SyncSettings
                {
                    Id = Guid.NewGuid(),
                    SyncEbayOrder = root.Sync?.SyncEbayOrder ?? false,
                    SyncLinnworksOrder = root.Sync?.SyncLinnworksOrder ?? false,
                    CreateEbayOrderToStream = root.Sync?.CreateEbayOrderToStream ?? false,
                    CreateLinnworksOrderToStream = root.Sync?.CreateLinnworksOrderToStream ?? false,
                    DispatchLinnworksOrderFromStream = root.Sync?.DispatchLinnworksOrderFromStream ?? false,
                    DispatchEbayOrderFromStream = root.Sync?.DispatchEbayOrderFromStream ?? false,
                    CreatedAt = DateTime.UtcNow
                };

                var ebay = new Ebay
                {
                    Id = Guid.NewGuid(),
                    DownloadOrderFromEbay = root.Ebay?.DownloadOrderFromEbay ?? false,
                    SendOrderToStream = root.Ebay?.SendOrderToStream ?? false,
                    UpdateInformationFromEbayToStream = root.Ebay?.UpdateInformationFromEbayToStream ?? false,
                    DispatchOrderFromEbay = root.Ebay?.DispatchOrderFromEbay ?? false,
                    UpdateTrackingDetailsFromStream = root.Ebay?.UpdateTrackingDetailsFromStream ?? false,
                    CreatedAt = DateTime.UtcNow
                };

                var integrationSettings = new IntegrationSettings
                {
                    Id = Guid.NewGuid(),
                    Name = root.Name ?? string.Empty,
                    Email = root.Email ?? string.Empty,
                    Password = root.Password ?? string.Empty,
                    AuthorizationToken = root.AuthorizationToken ?? string.Empty,
                    LinnworksSyncToken = root.LinnworksSyncToken ?? string.Empty,
                    Linnworks = linnworks,
                    Stream = stream,
                    Sync = sync,
                    Ebay = ebay,
                    LastSyncOnDate = root.LastSyncOnDate,
                    LastSyncOn = root.LastSyncOn ?? string.Empty,
                    ebaypage = root.ebaypage,
                    ebayhour = root.ebayhour,
                    linnpage = root.linnpage,
                    linnhour = root.linnhour,
                    CreatedAt = DateTime.UtcNow
                };

                _LinnworksSettings.Add(linnworks);
                _StreamSettings.Add(stream);
                _SyncSettings.Add(sync);
                _Ebay.Add(ebay);
                _IntegrationSettings.Add(integrationSettings);

                await _unitOfWork.Context.SaveChangesAsync();

                Console.WriteLine("✅ Registration inserted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error inserting registration: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("🔍 Inner: " + ex.InnerException.Message);
            }
        }
        public RegistrationData GetRegistrationData(string email)
        {
            var registrationData = new RegistrationData();
            var integrationSettings = _IntegrationSettings.Get(x => x.Email == email).FirstOrDefault();
            registrationData.Name = integrationSettings?.Name ?? string.Empty;
            registrationData.Email = integrationSettings?.Email ?? string.Empty;
            registrationData.Password = integrationSettings?.Password ?? string.Empty;
            registrationData.AuthorizationToken = integrationSettings?.AuthorizationToken ?? string.Empty;
            registrationData.LinnworksSyncToken = integrationSettings?.LinnworksSyncToken ?? string.Empty;
            registrationData.Linnworks = new LinnworksModel
            {
                DownloadOrderFromEbay = integrationSettings?.Linnworks?.DownloadOrderFromStream ?? false,
                DownloadOrderFromStream = integrationSettings?.Linnworks?.DownloadOrderFromEbay ?? false,
                PrintLabelFromStream = integrationSettings?.Linnworks?.PrintLabelFromStream ?? false,
                PrintLabelFromLinnworks = integrationSettings?.Linnworks?.PrintLabelFromLinnworks ?? false,
                DispatchOrderFromStream = integrationSettings?.Linnworks?.DispatchOrderFromStream ?? false,
                DispatchOrderFromEbay = integrationSettings?.Linnworks?.DispatchOrderFromEbay ?? false,
                SendChangeToEbay = integrationSettings?.Linnworks?.SendChangeToEbay ?? false,
                SendChangeToStream = integrationSettings?.Linnworks?.SendChangeToStream ?? false,
            };
            registrationData.Stream = new StreamModel
            {
                GetTrackingDetails = integrationSettings?.Stream?.GetTrackingDetails ?? false,
                EnableWebhook = integrationSettings?.Stream?.EnableWebhook ?? false,
                SendChangeFromLinnworksToStream = integrationSettings?.Stream?.SendChangeFromLinnworksToStream ?? false,
                SendChangesFromEbayToStream = integrationSettings?.Stream?.SendChangesFromEbayToStream ?? false,
                CreateProductToStream = integrationSettings?.Stream?.CreateProductToStream ?? false,
                DownloadProductFromStreamToLinnworks = integrationSettings?.Stream?.DownloadProductFromStreamToLinnworks ?? false,
                GetDepotListFromStream = integrationSettings?.Stream?.GetDepotListFromStream ?? false,
                GetRoutePlanFromStream = integrationSettings?.Stream?.GetRoutePlanFromStream ?? false,
            };
            registrationData.Ebay = new EbayModel
            {
                DownloadOrderFromEbay = integrationSettings?.Ebay?.DownloadOrderFromEbay ?? false,
                SendOrderToStream = integrationSettings?.Ebay?.SendOrderToStream ?? false,
                UpdateInformationFromEbayToStream = integrationSettings?.Ebay?.UpdateInformationFromEbayToStream ?? false,
                DispatchOrderFromEbay = integrationSettings?.Ebay?.DispatchOrderFromEbay ?? false,
                UpdateTrackingDetailsFromStream = integrationSettings?.Ebay?.UpdateTrackingDetailsFromStream ?? false,
            };
            registrationData.Sync = new SyncModel
            {
                SyncEbayOrder = integrationSettings?.Sync?.SyncEbayOrder ?? false,
                SyncLinnworksOrder = integrationSettings?.Sync?.SyncLinnworksOrder ?? false,
                CreateEbayOrderToStream = integrationSettings?.Sync?.CreateEbayOrderToStream ?? false,
                CreateLinnworksOrderToStream = integrationSettings?.Sync?.CreateLinnworksOrderToStream ?? false,
                DispatchEbayOrderFromStream = integrationSettings?.Sync?.DispatchEbayOrderFromStream ?? false,
                DispatchLinnworksOrderFromStream = integrationSettings?.Sync?.DispatchLinnworksOrderFromStream ?? false,
                UpdateLinnworksOrderToStream = integrationSettings?.Sync?.UpdateLinnworksOrderToStream ?? false,
            };
            registrationData.LastSyncOnDate = integrationSettings?.LastSyncOnDate ?? DateTime.MinValue;
            registrationData.LastSyncOn = integrationSettings?.LastSyncOn ?? DateTime.MinValue.ToString();
            registrationData.ebaypage = integrationSettings?.ebaypage ?? 1;
            registrationData.ebayhour = integrationSettings?.ebayhour ?? 0;
            registrationData.linnhour = integrationSettings?.linnhour ?? 0;
            registrationData.linnpage = integrationSettings?.linnpage ?? 1;
            return registrationData;
        }


        public async Task UpdateLinnworksOrdersToStream(AuthorizationConfigClass auth, string OrderId, string StreamOrderId)
        {
            List<CourierService> services = Services.GetServices;
            var streamAuth = _manageToken.GetToken(auth);

            CourierService selectedService = services.Find(s => s.ServiceUniqueId == CourierSettings.SelectedServiceId);
            bool existsInDb = _dbSqlCContext.ReportModel.Any(x => x.LinnNumOrderId == OrderId);

            if (!existsInDb) return;

            int numOrderId = int.Parse(OrderId);
            var orderRoot = await _dbSqlCContext.OrderRoot
                .Include(o => o.GeneralInfo)
                .Include(o => o.ShippingInfo)
                .Include(o => o.CustomerInfo).ThenInclude(c => c.Address)
                .Include(o => o.CustomerInfo).ThenInclude(c => c.BillingAddress)
                .Include(o => o.TotalsInfo)
                .Include(o => o.TaxInfo)
                .Include(o => o.Fulfillment)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.NumOrderId == numOrderId);

            if (orderRoot == null)
                throw new Exception($"Order with ID {OrderId} not found in database.");

            if (orderRoot.Items == null || !orderRoot.Items.Any())
            {
                orderRoot.Items = await _dbSqlCContext.Item
                    .Where(i => i.OrderId == orderRoot.OrderId)
                    .ToListAsync();
            }

            var streamOrderResponse = await StreamOrderApi.CreateOrderAsync(new GenerateLabelRequest
            {
                AuthorizationToken = auth.AuthorizationToken,
                AddressLine1 = orderRoot.CustomerInfo.Address.Address1,
                AddressLine2 = orderRoot.CustomerInfo.Address.Address2,
                AddressLine3 = orderRoot.CustomerInfo.Address.Address3,
                Postalcode = orderRoot.CustomerInfo.Address.PostCode,
                CompanyName = orderRoot.CustomerInfo.Address.Company,
                CountryCode = "GB",
                DeliveryNote = "",
                ServiceId = CourierSettings.SelectedServiceId,
                Email = auth.Email,
                Name = orderRoot.CustomerInfo.Address.FullName,
                OrderReference = orderRoot.NumOrderId.ToString(),
                OrderId = 0,
                Packages = new List<Package> {
            new Package {
                PackageDepth = 0,
                PackageHeight = 0,
                PackageWeight = 0,
                PackageWidth = 0,
                Items = orderRoot.Items.Select(f => new Item
                {
                    ProductCode = f.SKU ?? f.ChannelSKU,
                    ItemName = f.Title,
                    Quantity = f.Quantity
                }).ToList()
            }
        },
                ServiceConfigItems = new List<ServiceConfigItem>(),
                OrderExtendedProperties = new List<Models.ExtendedProperty>(),
                Phone = orderRoot.CustomerInfo.Address.PhoneNumber,
                Region = orderRoot.CustomerInfo.Address.Region,
                Town = orderRoot.CustomerInfo.Address.Town
            }, auth.ClientId, streamAuth.AccessToken, selectedService, true,
            orderRoot.ShippingInfo.PostalServiceName.ToLower().Contains("pickup") ? "COLLECTION" : "DELIVERY", StreamOrderId);

            streamOrderResponse.AuthorizationToken = auth.AuthorizationToken;

            if (streamOrderResponse.response != null && !streamOrderResponse.IsError)
            {
                await SaveStreamOrder(JsonConvert.SerializeObject(streamOrderResponse),
                    auth.AuthorizationToken, auth.Email, null, OrderId,
                    streamOrderResponse.response.consignmentNo,
                    streamOrderResponse.response.trackingId,
                    streamOrderResponse.response.trackingURL,
                    OrderId);
            }
            else
            {
                await SaveStreamOrder(streamOrderResponse.ErrorMessage, auth.AuthorizationToken, auth.Email,
                    null, OrderId, "Error", "Error", "Error", OrderId);
            }
        }


    }

}
