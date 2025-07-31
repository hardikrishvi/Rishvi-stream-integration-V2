using Amazon.Runtime.Internal;
using Azure.Core;
using LinnworksAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rishvi.Models;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ShippingIntegrations.Api;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using static Rishvi.Modules.ShippingIntegrations.Models.WebhookResponse;
using Address = Rishvi.Models.Address;
using Formatting = Newtonsoft.Json.Formatting;
using Item = Rishvi.Modules.ShippingIntegrations.Models.Item;

namespace Rishvi.Modules.ShippingIntegrations.Core
{
    public class TradingApiOAuthHelper
    {
        private readonly ReportsController _reportsController;
        private readonly SetupController _setupController;

        // private readonly Guid _selectedServiceGuid = new Guid("6A476315-04DB-4D25-A25C-E6917A1BCAD9");
        //public TradingApiOAuthHelper(ReportsController reportsController, SetupController setupController);

        private readonly Guid _selectedServiceGuid = new Guid("6A476315-04DB-4D25-A25C-E6917A1BCAD9");
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
        private readonly IRepository<ClientAuth> _ClientAuth;
        private readonly IRepository<Rishvi.Models.Item> _Item;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IRepository<Rishvi.Models.IntegrationSettings> _IntegrationSettings;
        private readonly IRepository<Rishvi.Models.LinnworksSettings> _LinnworksSettings;
        private readonly IRepository<Rishvi.Models.StreamSettings> _StreamSettings;
        private readonly IRepository<Rishvi.Models.SyncSettings> _SyncSettings;
        private readonly IRepository<Rishvi.Models.Ebay> _Ebay;
        private readonly ManageToken _manageToken;
        private readonly ILogger<TradingApiOAuthHelper> _logger;

        public TradingApiOAuthHelper(ReportsController reportsController, SetupController setupController, IOptions<CourierSettings> courierSettings, ApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
            IRepository<Address> address, IRepository<CustomerInfo> customerInfo, IRepository<Fulfillment> fulfillment,
            IRepository<GeneralInfo> generalInfo, IRepository<OrderRoot> orderRoot, IRepository<ShippingInfo> shippingInfo,
            IRepository<TaxInfo> taxInfo, IRepository<TotalsInfo> totalsInfo, IRepository<Rishvi.Models.Item> item,
            IRepository<IntegrationSettings> integrationSettings, IRepository<LinnworksSettings> linnworksSettings, IRepository<Rishvi.Models.StreamSettings> streamSettings,
            IRepository<Rishvi.Models.SyncSettings> syncSettings, IRepository<Rishvi.Models.Ebay> ebay, SqlContext dbSqlCContext, ManageToken manageToken, ILogger<TradingApiOAuthHelper> logger)

        {
            _reportsController = reportsController;
            _setupController = setupController;
            _dbContext = dbContext;
            _dbSqlCContext = dbSqlCContext;
            _Address = address;
            _unitOfWork = unitOfWork;
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
            _manageToken = manageToken;
            _logger = logger;
        }

        public async Task DispatchLinnOrdersFromStream(Rishvi.Models.Authorization _User, string orderids, string linntoken)
        {
            _logger.LogInformation("Dispatching Linnworks orders from Stream for user: {UserEmail}, Order IDs: {OrderIds}", _User.Email, orderids);
            var orderlist = Regex.Split(orderids, ",");
            foreach (var linnorderid in orderlist)
            {
                if (AwsS3.S3FileIsExists("Authorization", "LinnStreamOrder/" + "_streamorder_" + linnorderid + ".json").Result)
                {
                    var jsondata = AwsS3.GetS3File("Authorization", "LinnStreamOrder/" + "_streamorder_" + linnorderid + ".json");
                    var streamdata = JsonConvert.DeserializeObject<StreamOrderRespModel.Root>(jsondata);
                    if (streamdata != null)
                    {
                        if (linnorderid.IsValidInt32())
                        {
                            await DispatchOrderInLinnworks(_User, Convert.ToInt32(linnorderid), "Stream", streamdata.response.trackingId, streamdata.response.trackingURL, linntoken, null);
                        }
                    }
                }
            }
        }

        public Task SaveStreamOrder(string s, string AuthorizationToken, string email, string ebayorderid, string linnworksorderid, string consignmentid, string trackingnumber, string trackingurl, string order = "")
        {
            try
            {
                _logger.LogInformation("Saving Stream Order for User: {Email}, Order ID: {OrderId}", email, order);
                var streamOrderRecordExists = _dbSqlCContext.StreamOrderRecord
                    .Where(x => x.LinnworksOrderId == linnworksorderid)
                    .ToList();

                dynamic jsonData = JsonConvert.DeserializeObject(s);
                string extractedConsignmentNo = jsonData.response.consignmentNo;
                string extractedTrackingUrl = jsonData.response.trackingURL;
                string extractedTrackingId = jsonData.response.trackingId;

                if (streamOrderRecordExists.Any())
                {
                    // Update the existing record (or multiple if applicable)
                    foreach (var existingRecord in streamOrderRecordExists)
                    {
                        existingRecord.JsonData = s;
                        existingRecord.AuthorizationToken = AuthorizationToken;
                        existingRecord.Email = email;
                        existingRecord.EbayOrderId = ebayorderid ?? "0";
                        existingRecord.ConsignmentId = extractedConsignmentNo ?? consignmentid;
                        existingRecord.TrackingNumber = trackingnumber ?? string.Empty;
                        existingRecord.TrackingUrl = extractedTrackingUrl ?? trackingurl;
                        existingRecord.TrackingId = extractedTrackingId ?? "0";
                        existingRecord.Order = order;
                        existingRecord.CreatedAt = DateTime.UtcNow; // or keep original
                    }
                }
                else
                {
                    // Insert new
                    var newRecord = new StreamOrderRecord
                    {
                        Id = Guid.NewGuid(),
                        JsonData = s,
                        AuthorizationToken = AuthorizationToken,
                        Email = email,
                        EbayOrderId = ebayorderid ?? "0",
                        LinnworksOrderId = linnworksorderid,
                        ConsignmentId = extractedConsignmentNo ?? consignmentid,
                        TrackingNumber = trackingnumber ?? string.Empty,
                        TrackingUrl = extractedTrackingUrl ?? trackingurl,
                        TrackingId = extractedTrackingId ?? "0",
                        Order = order,
                        CreatedAt = DateTime.UtcNow
                    };

                    _dbSqlCContext.StreamOrderRecord.Add(newRecord);
                }

                _dbSqlCContext.SaveChanges();

                var existingReports = _dbSqlCContext.ReportModel
                    .Where(x => x.email == email)
                    .ToList();

                var reportsToSave = new List<ReportModel>();

                if (ebayorderid != null)
                {
                    var existingEbay = existingReports
                        .FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid);

                    if (existingEbay == null)
                    {
                        reportsToSave.Add(new ReportModel
                        {
                            _id = Guid.NewGuid().ToString(),
                            AuthorizationToken = AuthorizationToken,
                            StreamOrderId = order,
                            StreamConsignmentId = consignmentid,
                            StreamTrackingNumber = trackingnumber,
                            StreamTrackingURL = trackingurl,
                            EbayChannelOrderRef = ebayorderid,
                            IsEbayOrderCreatedInStream = true,
                            DownloadEbayOrderInSystem = DateTime.Now,
                            CreateEbayOrderInStream = DateTime.Now,
                            email = email,
                            StreamOrderCreateJson = $"UserStreamOrder/{AuthorizationToken}_streamorder_{order}.json",
                            createdDate = DateTime.Now,
                            updatedDate = DateTime.Now
                        });
                    }
                    else
                    {
                        existingEbay.CreateEbayOrderInStream = DateTime.Now;
                        existingEbay.IsEbayOrderCreatedInStream = true;
                        existingEbay.StreamOrderCreateJson = $"UserStreamOrder/{AuthorizationToken}_streamorder_{order}.json";
                        existingEbay.StreamOrderId = order;
                        existingEbay.StreamConsignmentId = consignmentid;
                        existingEbay.StreamTrackingNumber = trackingnumber;
                        existingEbay.StreamTrackingURL = trackingurl;
                        existingEbay.updatedDate = DateTime.Now;

                        reportsToSave.Add(existingEbay);
                    }
                }
                else if (linnworksorderid != null)
                {
                    var existingLinn = existingReports
                        .FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid);

                    if (existingLinn == null)
                    {
                        reportsToSave.Add(new ReportModel
                        {
                            _id = Guid.NewGuid().ToString(),
                            AuthorizationToken = AuthorizationToken,
                            StreamOrderId = order,
                            StreamConsignmentId = consignmentid,
                            StreamTrackingNumber = trackingnumber,
                            StreamTrackingURL = trackingurl,
                            LinnNumOrderId = linnworksorderid,
                            IsLinnOrderCreatedInStream = true,
                            email = email,
                            CreateLinnOrderInStream = DateTime.Now,
                            StreamOrderCreateJson = $"UserStreamOrder/{AuthorizationToken}_streamorder_{order}.json",
                            createdDate = DateTime.Now,
                            updatedDate = DateTime.Now
                        });
                    }
                    else
                    {
                        existingLinn.CreateLinnOrderInStream = DateTime.Now;
                        existingLinn.IsLinnOrderCreatedInStream = true;
                        existingLinn.StreamOrderCreateJson = $"UserStreamOrder/{AuthorizationToken}_streamorder_{order}.json";
                        existingLinn.StreamOrderId = order;
                        existingLinn.StreamConsignmentId = consignmentid;
                        existingLinn.StreamTrackingNumber = trackingnumber;
                        existingLinn.StreamTrackingURL = trackingurl;
                        existingLinn.updatedDate = DateTime.Now;

                        reportsToSave.Add(existingLinn);
                    }
                }

                if (reportsToSave.Any())
                {
                    SaveReportDataForTEst(reportsToSave);
                }
                _logger.LogInformation("Stream Order saved successfully for User: {Email}, Order ID: {OrderId}", email, order);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
                Console.WriteLine($"Error in SaveStreamOrder: {ex.Message}");
                _logger.LogError(ex, "Error in SaveStreamOrder for User: {Email}, Order ID: {OrderId}", email, order);
                SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, null, linnworksorderid, "SaveStreamOrder", ex.Message, true, "clientId");
            }
        }

        #region Linnworks Get Order and Dispatch Function
        public List<string> CompareJsonObjects(JObject obj1, JObject obj2)
        {
            List<string> differences = new List<string>();
            foreach (var property in obj1.Properties())
            {
                var propName = property.Name;
                var propValue1 = property.Value.ToString();
                if (obj2.ContainsKey(propName))
                {
                    var propValue2 = obj2[propName].ToString();
                    if (propValue1 != propValue2)
                    {
                        differences.Add(propName);
                    }
                }
            }
            foreach (var property in obj2.Properties())
            {
                var propName = property.Name;
                if (!obj1.ContainsKey(propName))
                {
                    differences.Add(propName);
                }
            }
            return differences;
        }

        public async Task UpdateOrderExProperty(string linntoken, int orderid, Dictionary<string, string> values)
        {
            if (string.IsNullOrWhiteSpace(linntoken))
            {
                throw new ArgumentException("Invalid Linnworks token.");
            }
            _logger.LogInformation("Updating extended properties for Linnworks order ID: {OrderId}", orderid);

            var obj = new LinnworksBaseStream(linntoken);
            var linnorderid = obj.Api.Orders.GetOrderDetailsByNumOrderId(orderid);
            // Fetch the Linnworks order details
            if (linnorderid == null)
            {
                throw new Exception($"Order with ID {orderid} not found.");
            }
            var res = obj.Api.Orders.AddExtendedProperties(new AddExtendedPropertiesRequest()
            {
                ExtendedProperties = values.Where(d => !string.IsNullOrEmpty(d.Value)).Select(g => new BasicExtendedProperty()
                { Name = g.Key, Value = g.Value, Type = "Stream" }).ToArray(),
                OrderId = linnorderid.OrderId
            });
            if (res.ExtendedPropertiesInserted == 0)
            {
                var res2 = obj.Api.Orders.SetExtendedProperties(linnorderid.OrderId,
                    values.Where(d => !string.IsNullOrEmpty(d.Value)).Select(g => new LinnworksAPI.ExtendedProperty()
                    { Name = g.Key, Value = g.Value, Type = "Stream" }).ToArray());
            }
            _logger.LogInformation("Extended properties updated successfully for Linnworks order ID: {OrderId}", orderid);
        }

        // Fix for CS0120: An object reference is required for the non-static field, method, or property 'CourierSettings.SelectedServiceId'

        // Assuming that `CourierSettings` is intended to be instantiated and used as an object, 
        // we need to create an instance of `CourierSettings` and access its `SelectedServiceId` property.

        public async Task UpdateLinnworksOrdersToStream(Rishvi.Models.Authorization auth, string OrderId, string StreamOrderId)
        {
            List<CourierService> services = Services.GetServices;
            var streamAuth = _manageToken.GetToken(auth);

            CourierService selectedService = services.Find(s => s.ServiceUniqueId == CourierSettings.SelectedServiceId);
            bool existsInDb = _dbSqlCContext.ReportModel
                .Any(x => x.LinnNumOrderId == OrderId);

            _logger.LogInformation("Updating Linnworks orders to Stream for User: {UserEmail}, Order ID: {OrderId}", auth.Email, OrderId);


            if (existsInDb)
            {
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
                    .FirstOrDefaultAsync(o => o.NumOrderId == (numOrderId));

                if (orderRoot == null)
                {
                    throw new Exception($"Order with ID {OrderId} not found in database.");
                }
                if (orderRoot.Items == null || !orderRoot.Items.Any())
                {
                    orderRoot.Items = await _dbSqlCContext.Item
                        .Where(i => i.OrderId == orderRoot.OrderId)
                        .ToListAsync();
                }

                // Serialize to indented JSON
                var json = JsonConvert.SerializeObject(orderRoot);
                string LocationName = "SGK";
                string HandsonDate = "";

                if (auth.HandsOnDate)
                {
                    HandsonDate = DateTime.Now.ToString();
                }
                if (auth.UseDefaultLocation && auth.DefaultLocation != "")
                {
                    LocationName = auth.DefaultLocation;
                }
                int orderId = Convert.ToInt32(StreamOrderId);
                try
                {

                    var jsopndata = orderRoot;
                    var streamOrderResponse = StreamOrderApi.CreateOrder(new GenerateLabelRequest()
                    {
                        AuthorizationToken = auth.AuthorizationToken,
                        AddressLine1 = jsopndata.CustomerInfo.Address.Address1,
                        AddressLine2 = jsopndata.CustomerInfo.Address.Address2,
                        AddressLine3 = jsopndata.CustomerInfo.Address.Address3,
                        Postalcode = jsopndata.CustomerInfo.Address.PostCode,
                        CompanyName = jsopndata.CustomerInfo.Address.Company,
                        CountryCode = "GB",
                        DeliveryNote = "",
                        ServiceId = CourierSettings.SelectedServiceId,
                        Email = auth.Email,
                        Name = jsopndata.CustomerInfo.Address.FullName,
                        OrderReference = jsopndata.NumOrderId.ToString(),
                        OrderId = 0,
                        Packages = new List<Package>() { new Package() {
                                       PackageDepth = 0,
                                       PackageHeight  = 0,PackageWeight = 0 ,PackageWidth = 0,
                                    Items = jsopndata.Items.Select(f=> new Item()
                                    {
                                          ProductCode =  f.SKU == null ?f.ChannelSKU : f.SKU,
                                          ItemName =f.Title,
                                          Quantity = f.Quantity
                                      }).ToList()
                                 }},
                        ServiceConfigItems = new List<ServiceConfigItem>(),
                        OrderExtendedProperties = new List<Models.ExtendedProperty>(),
                        Phone = jsopndata.CustomerInfo.Address.PhoneNumber,
                        Region = jsopndata.CustomerInfo.Address.Region,
                        Town = jsopndata.CustomerInfo.Address.Town,

                    }, auth.ClientId, streamAuth.AccessToken, selectedService, true, jsopndata.ShippingInfo.PostalServiceName.ToLower().Contains("pickup") ? "COLLECTION" : "DELIVERY", StreamOrderId, LocationName, HandsonDate, auth.IsLiveAccount);
                    streamOrderResponse.Item1.AuthorizationToken = auth.AuthorizationToken;
                    streamOrderResponse.Item1.ItemId = "";
                    //if (streamOrderResponse.Item1.response == null)
                    //{
                    //    await SaveStreamOrder(streamOrderResponse.Item2, auth.AuthorizationToken.ToString(), auth.Email, null, OrderId, "Error", "Error", "Error", OrderId);
                    //}
                    //else
                    //{
                    //    await SaveStreamOrder(JsonConvert.SerializeObject(streamOrderResponse.Item1), auth.AuthorizationToken.ToString(), auth.Email, null, OrderId, streamOrderResponse.Item1.response.consignmentNo, streamOrderResponse.Item1.response.trackingId, streamOrderResponse.Item1.response.trackingURL, OrderId);
                    //}
                }
                catch (Exception ex)
                {
                    SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, null, OrderId, "UpdateLinnworksOrdersToStream", ex.Message, true, "clientId");
                    _logger.LogError(ex, "Error updating Linnworks orders to Stream for User: {UserEmail}, Order ID: {OrderId}", auth.Email, OrderId);
                }
            }
        }

        // Assuming that `CourierSettings` is intended to be instantiated and used as an object, 
        // we need to create an instance of `CourierSettings` and access its `SelectedServiceId` property.

        public async Task CreateLinnworksOrdersToStream(Rishvi.Models.Authorization auth, string OrderId)
        {
            // Proper Any query
            try
            {
                _logger.LogInformation("Creating Linnworks orders to Stream for User: {UserEmail}, Order ID: {OrderId}", auth.Email, OrderId);

                List<CourierService> services = Services.GetServices;
                //var streamAuth = _manageToken.GetToken(auth);


                CourierService selectedService = services.Find(s => s.ServiceUniqueId == CourierSettings.SelectedServiceId);
                bool existsInDb = _dbSqlCContext.ReportModel
                    .Any(x => x.LinnNumOrderId == OrderId);
                if (selectedService != null)
                {


                    if (existsInDb)
                    {
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
                            .FirstOrDefaultAsync(o => o.NumOrderId == (numOrderId));

                        if (orderRoot != null)
                        {
                            //throw new Exception($"Order with ID {OrderId} not found in database.");

                            if (orderRoot.Items == null || !orderRoot.Items.Any())
                            {
                                orderRoot.Items = await _dbSqlCContext.Item
                                    .Where(i => i.OrderId == orderRoot.OrderId)
                                    .ToListAsync();
                            }

                            // Serialize to indented JSON
                            var json = JsonConvert.SerializeObject(orderRoot);

                            var shippingdata = orderRoot.ShippingInfo.PostalServiceId;

                            try
                            {
                                var dta = _dbSqlCContext.PostalServices
                                    .FirstOrDefault(x => x.PostalServiceId == orderRoot.ShippingInfo.PostalServiceId.ToString());

                                if (dta != null)
                                {

                                    // throw new Exception($"Postal service with ID {orderRoot.ShippingInfo.PostalServiceId} not found.");


                                    var auth1 = _dbSqlCContext.Authorizations.Where(x => x.AuthorizationToken == dta.AuthorizationToken).FirstOrDefault();

                                    string LocationName = "SGK";
                                    string HandsonDate = "";
                                    string deieveryMethod = "";

                                    if (auth.HandsOnDate)
                                    {
                                        HandsonDate = DateTime.Now.ToString();
                                    }
                                    if (auth.UseDefaultLocation && auth.DefaultLocation != "")
                                    {
                                        LocationName = auth.DefaultLocation;
                                    }

                                    var streamAuth = _manageToken.GetToken(auth1);
                                    if (streamAuth.AccessToken != null)
                                    {
                                        if (json != "null")
                                        {
                                            try
                                            {
                                                var jsopndata = orderRoot;
                                                int orderId = jsopndata.NumOrderId;

                                                var getdepots = StreamOrderApi.GetDepots(streamAuth.AccessToken, auth1.ClientId, auth1.IsLiveAccount);

                                                if (getdepots != null && getdepots.response != null && getdepots.response.depots.Count > 0)
                                                {
                                                    var singledeport = getdepots.response.depots.Where(x => x.address.name == LocationName).FirstOrDefault();

                                                    if (singledeport != null)
                                                    {

                                                        var chdel = jsopndata.ShippingInfo.PostalServiceName.ToLower().Contains("pickup") ? "COLLECTION" : "DELIVERY";

                                                        var delmethod = singledeport.deliveryMethods.Where(x => x.type == chdel).Select(y => y.name).FirstOrDefault();

                                                        deieveryMethod = delmethod ?? "";

                                                    }

                                                }

                                                var streamOrderResponse = StreamOrderApi.CreateOrder(new GenerateLabelRequest()
                                                {
                                                    AuthorizationToken = auth1.AuthorizationToken,
                                                    PostalServiceName = dta.PostalServiceName,
                                                    AddressLine1 = jsopndata.CustomerInfo.Address.Address1,
                                                    AddressLine2 = jsopndata.CustomerInfo.Address.Address2,
                                                    AddressLine3 = jsopndata.CustomerInfo.Address.Address3,
                                                    Postalcode = jsopndata.CustomerInfo.Address.PostCode,
                                                    CompanyName = jsopndata.CustomerInfo.Address.Company,
                                                    CountryCode = "GB",
                                                    DeliveryNote = "",
                                                    deliveryMethod = deieveryMethod,
                                                    //ServiceId = courierSettings.SelectedServiceId,
                                                    // Access the static property directly using the class name instead of an instance
                                                    ServiceId = CourierSettings.SelectedServiceId,
                                                    Email = auth.Email,
                                                    Name = jsopndata.CustomerInfo.Address.FullName,
                                                    OrderReference = jsopndata.NumOrderId.ToString(),
                                                    OrderId = 0,
                                                    Packages = new List<Package>() { new Package() {
                                        PackageDepth = 0,
                                        PackageHeight  = 0,PackageWeight = 0 ,PackageWidth = 0,
                                        Items = jsopndata.Items.Select(f=> new Item()
                                        {
                                            ProductCode =  f.SKU == null ?f.ChannelSKU : f.SKU,
                                            ItemName =f.Title,
                                            Quantity = f.Quantity,
                                            UnitWeight = f.Weight != 0 ? f.Weight.ToDecimal() : 0,
                                            Height = f.Height != 0 ? f.Height.ToDecimal() : 0,
                                            Width = f.width != 0 ? f.width.ToDecimal() : 0,
                                            Length = f.Length != 0 ? f.Length.ToDecimal() : 0,

                                            //UnitWeight = f.Weight != 0 ? Math.Round(f.Weight.ToDecimal() / 1000, 2) : 0,

                                        }).ToList()
                                  }},
                                                    ServiceConfigItems = new List<ServiceConfigItem>(),
                                                    OrderExtendedProperties = new List<Models.ExtendedProperty>(),
                                                    Phone = jsopndata.CustomerInfo.Address.PhoneNumber,
                                                    Region = jsopndata.CustomerInfo.Address.Region,
                                                    Town = jsopndata.CustomerInfo.Address.Town
                                                }, auth1.ClientId, streamAuth.AccessToken, selectedService, true, jsopndata.ShippingInfo.PostalServiceName.ToLower().Contains("pickup") ? "COLLECTION" : "DELIVERY", null, LocationName, HandsonDate, auth1.IsLiveAccount);
                                                streamOrderResponse.Item1.AuthorizationToken = auth1.AuthorizationToken;
                                                streamOrderResponse.Item1.ItemId = "";
                                                if (streamOrderResponse.Item1.response == null)
                                                {
                                                    SaveStreamOrder(streamOrderResponse.Item2, auth1.AuthorizationToken.ToString(), auth1.Email, null, OrderId, "Error", "Error", "Error", OrderId);
                                                }
                                                else
                                                {
                                                    SaveStreamOrder(JsonConvert.SerializeObject(streamOrderResponse.Item1), auth1.AuthorizationToken.ToString(), auth1.Email, null, OrderId,
                                                        streamOrderResponse.Item1.response.consignmentNo, streamOrderResponse.Item1.response.trackingId, streamOrderResponse.Item1.response.trackingURL, OrderId);
                                                }
                                                _logger.LogInformation("Linnworks order created successfully in Stream for User: {UserEmail}, Order ID: {OrderId}", auth.Email, OrderId);
                                            }
                                            catch
                                            {
                                                _logger.LogError("Error creating Linnworks order in Stream for User: {UserEmail}, Order ID: {OrderId}", auth.Email, OrderId);
                                                SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, null, OrderId, "CreateLinnworksOrdersToStream", "Order data not found for OrderId: " + OrderId, true, "clientId");
                                            }
                                        }
                                        else
                                        {
                                            _logger.LogError("Order data is null for User: {UserEmail}, Order ID: {OrderId}", auth.Email, OrderId);
                                            SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, null, OrderId, "CreateLinnworksOrdersToStream", "Order data not found for OrderId: " + OrderId, true, "clientId");
                                        }
                                    }

                                }
                                else
                                {
                                    _logger.LogError("Postal service not found for User: {UserEmail}, Order ID: {OrderId}", auth.Email, OrderId);
                                    SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, null, OrderId, "CreateLinnworksOrdersToStream", "Postal Service not found: " + OrderId, true, "clientId");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error creating Linnworks order in Stream for User: {UserEmail}, Order ID: {OrderId}", auth.Email, OrderId);
                                SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, null, OrderId, "CreateLinnworksOrdersToStream", "Postal Service not found: " + OrderId, true, "clientId");
                            }
                        }
                    }

                }
                else
                {
                    _logger.LogError("Selected service not found for User: {UserEmail}, Order ID: {OrderId}", auth.Email, OrderId);
                    SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, null, OrderId, "CreateLinnworksOrdersToStream", "Postal Service not found: " + OrderId, true, "clientId");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Linnworks orders to Stream for User: {UserEmail}, Order ID: {OrderId}", auth.Email, OrderId);
                var str = ex.ToString();
            }
        }
        public async Task DispatchOrderInLinnworks(Rishvi.Models.Authorization _User, int OrderRef, string linntoken,
            string Service, string TrackingNumber, string TrackingUrl, string dispatchdate)
        {
            _logger.LogInformation("Dispatching order in Linnworks for User: {UserEmail}, Order Reference: {OrderRef}", _User.Email, OrderRef);
            string ProductResp = "";
            var obj = new LinnworksBaseStream(linntoken);
            // create identifier 
            var orderdata = obj.Api.Orders.GetOrderDetailsByNumOrderId(OrderRef);
            // var list = obj.Api.Orders.SetOrderShippingInfo(orderdata.OrderId, new UpdateOrderShippingInfoRequest() { 
            //  ItemWeight = orderdata.ShippingInfo.ItemWeight,
            //  ManualAdjust = orderdata.ShippingInfo.ManualAdjust,
            //  PostageCost = orderdata.ShippingInfo.PostageCost,  
            //  PostalServiceId = orderdata.ShippingInfo.PostalServiceId,
            //  TotalWeight = orderdata.ShippingInfo.TotalWeight,
            //  TrackingNumber = TrackingNumber
            // });
            var generalinfo = orderdata.GeneralInfo;
            generalinfo.DespatchByDate = dispatchdate == null ? DateTime.Now : DateTime.Parse(dispatchdate);
            obj.Api.Orders.SetOrderGeneralInfo(orderdata.OrderId, generalinfo, false);
            orderdata = obj.Api.Orders.GetOrderDetailsByNumOrderId(OrderRef);
            _logger.LogInformation("Order details fetched for User: {UserEmail}, Order Reference: {OrderRef}", _User.Email, OrderRef);
            await SaveLinnDispatch(JsonConvert.SerializeObject(orderdata), _User.AuthorizationToken.ToString(), _User.Email, linntoken, OrderRef);

        }
        #endregion
        #region Stream Create Order & Get Order and Other Function
        public async Task CreateStreamWebhook(Rishvi.Models.Authorization _User, string eventname, string event_type, string url_path, string http_method, string content_type, string auth_header)
        {
            _logger.LogInformation("Creating Stream webhook for User: {UserEmail}, Event Name: {EventName}", _User.Email, eventname);
            await _setupController.SubscribeWebhook(_User.AuthorizationToken, eventname, event_type, url_path, http_method, content_type, auth_header);
        }
        // this function is for save report data for specific user
        public async Task<StreamGetOrderResponse.Root> GetStreamOrder(Rishvi.Models.Authorization _User, string OrderId)
        {
            _logger.LogInformation("Fetching Stream order for User: {UserEmail}, Order ID: {OrderId}", _User.Email, OrderId);
            var streamAuth = _manageToken.GetToken(_User);
            return StreamOrderApi.GetOrder(streamAuth.AccessToken, OrderId, _User.ClientId, _User.IsLiveAccount);
        }
        #endregion
        #region Save s3 data

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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };
                var ebay = new Ebay
                {
                    Id = Guid.NewGuid(),
                    DownloadOrderFromEbay = root.Ebay?.DownloadOrderFromEbay ?? false,
                    SendOrderToStream = root.Ebay?.SendOrderToStream ?? false,
                    UpdateInformationFromEbayToStream = root.Ebay?.UpdateInformationFromEbayToStream ?? false,
                    DispatchOrderFromEbay = root.Ebay?.DispatchOrderFromEbay ?? false,
                    UpdateTrackingDetailsFromStream = root.Ebay?.UpdateTrackingDetailsFromStream ?? false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
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
                    Ebay = ebay, // or handle if you add a proper class later
                    LastSyncOnDate = root.LastSyncOnDate,
                    LastSyncOn = root.LastSyncOn ?? string.Empty,
                    ebaypage = root.ebaypage,
                    ebayhour = root.ebayhour,
                    linnpage = root.linnpage,
                    linnhour = root.linnhour,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };
                _LinnworksSettings.Add(linnworks);
                _StreamSettings.Add(stream);
                _SyncSettings.Add(sync);
                _Ebay.Add(ebay);
                _IntegrationSettings.Add(integrationSettings);


                await _unitOfWork.Context.SaveChangesAsync();

                Console.WriteLine("✅ Order inserted successfully");
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, null, json, "RegisterSaveFromJson", ex.Message, true, "clientId");
                Console.WriteLine("❌ Error inserting order: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("🔍 Inner: " + ex.InnerException.Message);
            }
        }

        public async Task SaveReportData(string s, string email)
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "Reports/" + email.ToLower().Replace("@", "_").Replace(".", "_").ToString() + "_report.json");

        }
        private void EnsureValidDates(ReportModel report)
        {
            DateTime sqlMinDate = new DateTime(1753, 1, 1);

            if (report.createdDate < sqlMinDate) report.createdDate = DateTime.Now;
            if (report.updatedDate < sqlMinDate) report.updatedDate = DateTime.Now;
            if (report.DownloadLinnOrderInSystem < sqlMinDate) report.DownloadLinnOrderInSystem = DateTime.Now;
            if (report.DownloadEbayOrderInSystem < sqlMinDate) report.DownloadEbayOrderInSystem = DateTime.Now;
            if (report.DispatchEbayOrderInStream < sqlMinDate) report.DispatchEbayOrderInStream = DateTime.Now;
            if (report.DispatchEbayOrderFromStream < sqlMinDate) report.DispatchEbayOrderFromStream = DateTime.Now;
            if (report.CreateLinnOrderInStream < sqlMinDate) report.CreateLinnOrderInStream = DateTime.Now;
            if (report.LastUpdateLinnOrderForStream < sqlMinDate) report.LastUpdateLinnOrderForStream = DateTime.Now;
            if (report.DispatchLinnOrderFromStream < sqlMinDate) report.DispatchLinnOrderFromStream = DateTime.Now;
            if (report.DispatchLinnOrderInStream < sqlMinDate) report.DispatchLinnOrderInStream = DateTime.Now;
            if (report.CreateEbayOrderInStream < sqlMinDate) report.CreateEbayOrderInStream = DateTime.Now;
            if (report.CreatedAt < sqlMinDate) report.CreatedAt = DateTime.Now;
            if (report.UpdatedAt.HasValue && report.UpdatedAt.Value < sqlMinDate) report.UpdatedAt = DateTime.Now;
        }
        public async Task SaveReportDataForTEst(List<ReportModel> reportData)
        {
            foreach (var report in reportData)
            {
                EnsureValidDates(report);

                var existing = await _unitOfWork.Context.Set<ReportModel>()
                    .FirstOrDefaultAsync(x => x._id == report._id);

                if (existing == null)
                {
                    _unitOfWork.Context.Add(report);
                }
                else
                {
                    existing.updatedDate = DateTime.Now;
                    existing.DownloadLinnOrderInSystem = report.DownloadLinnOrderInSystem;
                    existing.LinnOrderDetailsJson = report.LinnOrderDetailsJson;
                    existing.AuthorizationToken = report.AuthorizationToken;
                    existing.LinnNumOrderId = report.LinnNumOrderId;
                    existing.email = report.email;
                }
            }

            await _unitOfWork.Context.SaveChangesAsync();
        }
        // public async Task SaveLinnOrder(string s, string AuthorizationToken, string email, string linnorderid = "")
        // {
        //     InsertOrderFromJson(s);
        //     var stream = new MemoryStream();
        //     StreamWriter sw = new StreamWriter(stream);
        //     sw.Write(s);
        //     sw.Flush();
        //     stream.Position = 0;
        //     AwsS3.UploadFileToS3("Authorization", stream, "LinnOrder/" + AuthorizationToken.ToString() + "_linnorder_" + linnorderid + ".json");
        //     var alldata = _reportsController.GetReportData(new ReportModelReq() { email = email }).Result;
        //     if (alldata.Count(f => f.LinnNumOrderId == linnorderid) == 0)
        //     {
        //         alldata.Add(new ReportModel()
        //         {
        //             _id = Guid.NewGuid().ToString(),
        //             updatedDate = DateTime.Now,
        //             email = email,
        //             DownloadLinnOrderInSystem = DateTime.Now,
        //             createdDate = DateTime.Now,
        //             AuthorizationToken = AuthorizationToken,
        //             LinnNumOrderId = linnorderid,
        //             LinnOrderDetailsJson = "LinnOrder/" + AuthorizationToken.ToString() + "_linnorder_" + linnorderid + ".json"
        //         });
        //         await SaveReportData(JsonConvert.SerializeObject(alldata), email);
        //     }
        //     else
        //     {
        //         alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid).LinnOrderDetailsJson = "LinnOrder/" + AuthorizationToken.ToString() + "_linnorder_" + linnorderid + ".json";
        //         alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid).DownloadLinnOrderInSystem = DateTime.Now;
        //         alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid).updatedDate = DateTime.Now;
        //        await SaveReportData(JsonConvert.SerializeObject(alldata), email);
        //     }
        // }
        public async Task SaveLinnOrder(string s, string AuthorizationToken, string email, string linntoken, string linnorderid = "")
        {
            InsertOrderFromJson(s, linntoken);

            //var alldata = await _reportsController.GetReportData(new ReportModelReq() { email = email });
            var alldata = _dbSqlCContext.ReportModel
                .Where(x => x.email == email)
                .ToList();

            var existing = alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid);
            if (existing == null)
            {
                alldata.Add(new ReportModel
                {
                    _id = Guid.NewGuid().ToString(),
                    updatedDate = DateTime.Now,
                    createdDate = DateTime.Now,
                    email = email,
                    DownloadLinnOrderInSystem = DateTime.Now,
                    AuthorizationToken = AuthorizationToken,
                    LinnNumOrderId = linnorderid,
                    LinnOrderDetailsJson = s // save JSON directly in DB field
                });
            }
            else
            {
                existing.LinnOrderDetailsJson = s;
                existing.DownloadLinnOrderInSystem = DateTime.Now;
                existing.updatedDate = DateTime.Now;
            }

            await SaveReportDataForTEst(alldata);
        }
        public void InsertOrderFromJson(string json, string linntoken)
        {
            SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, json, null, "InsertOrderFromJson", null, true, "clientId");
            try
            {
                var root = JsonConvert.DeserializeObject<OrderRoot>(json);

                var obj = new LinnworksBaseStream(linntoken);

                try
                {
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

                    var generalInfo = root.GeneralInfo ?? new GeneralInfo();

                    generalInfo.Id = Guid.NewGuid();
                    generalInfo.Status = root.GeneralInfo?.Status ?? 0;
                    generalInfo.LabelPrinted = root.GeneralInfo?.LabelPrinted ?? false;
                    generalInfo.LabelError = root.GeneralInfo?.LabelError ?? "";
                    generalInfo.InvoicePrinted = root.GeneralInfo?.InvoicePrinted ?? false;
                    generalInfo.PickListPrinted = root.GeneralInfo?.PickListPrinted ?? false;
                    generalInfo.IsRuleRun = root.GeneralInfo?.IsRuleRun ?? false;
                    generalInfo.Notes = root.GeneralInfo?.Notes ?? 0;
                    generalInfo.PartShipped = root.GeneralInfo?.PartShipped ?? false;
                    generalInfo.Marker = root.GeneralInfo?.Marker ?? 0;
                    generalInfo.IsParked = root.GeneralInfo?.IsParked ?? false;
                    generalInfo.ReferenceNum = root.GeneralInfo?.ReferenceNum ?? "";
                    generalInfo.SecondaryReference = root.GeneralInfo?.SecondaryReference ?? "";
                    generalInfo.ExternalReferenceNum = root.GeneralInfo?.ExternalReferenceNum ?? "";
                    generalInfo.ReceivedDate = root.GeneralInfo?.ReceivedDate ?? DateTime.UtcNow;
                    generalInfo.Source = root.GeneralInfo?.Source ?? "N/A";
                    generalInfo.SubSource = root.GeneralInfo?.SubSource ?? "N/A";
                    generalInfo.SiteCode = string.IsNullOrWhiteSpace(root.GeneralInfo?.SiteCode) ? "N/A" : root.GeneralInfo.SiteCode;
                    generalInfo.HoldOrCancel = root.GeneralInfo?.HoldOrCancel ?? false;
                    generalInfo.DespatchByDate = root.GeneralInfo?.DespatchByDate ?? DateTime.UtcNow;
                    generalInfo.HasScheduledDelivery = root.GeneralInfo?.HasScheduledDelivery ?? false;
                    generalInfo.Location = root.GeneralInfo?.Location ?? Guid.Empty;
                    generalInfo.NumItems = root.GeneralInfo?.NumItems ?? 0;
                    generalInfo.CreatedAt = DateTime.UtcNow;
                    generalInfo.UpdatedAt = null; // or DateTime.UtcNow if you're treating it as 'just saved'

                    root.GeneralInfo = generalInfo;

                    var shippingInfo = root.ShippingInfo ?? new ShippingInfo();

                    shippingInfo.ShippingId = Guid.NewGuid();
                    shippingInfo.Vendor = root.ShippingInfo?.Vendor ?? "N/A";
                    shippingInfo.PostalServiceId = root.ShippingInfo?.PostalServiceId ?? Guid.Empty;
                    shippingInfo.PostalServiceName = root.ShippingInfo?.PostalServiceName ?? "N/A";
                    shippingInfo.TotalWeight = root.ShippingInfo?.TotalWeight ?? 0;
                    shippingInfo.ItemWeight = root.ShippingInfo?.ItemWeight ?? 0;
                    shippingInfo.PackageCategoryId = root.ShippingInfo?.PackageCategoryId ?? Guid.Empty;
                    shippingInfo.PackageCategory = root.ShippingInfo?.PackageCategory ?? "Default";
                    shippingInfo.PackageTypeId = root.ShippingInfo?.PackageTypeId ?? Guid.Empty;
                    shippingInfo.PackageType = root.ShippingInfo?.PackageType ?? "Default";
                    shippingInfo.PostageCost = root.ShippingInfo?.PostageCost ?? 0;
                    shippingInfo.PostageCostExTax = root.ShippingInfo?.PostageCostExTax ?? 0;
                    shippingInfo.TrackingNumber = root.ShippingInfo?.TrackingNumber ?? "";
                    shippingInfo.ManualAdjust = root.ShippingInfo?.ManualAdjust ?? false;
                    shippingInfo.CreatedAt = DateTime.UtcNow;
                    shippingInfo.UpdatedAt = null; // Set to DateTime.UtcNow if updating later

                    root.ShippingInfo = shippingInfo;

                    var totalsInfo = root.TotalsInfo ?? new TotalsInfo();

                    totalsInfo.TotalsInfoId = Guid.NewGuid();
                    totalsInfo.Subtotal = root.TotalsInfo?.Subtotal ?? 0;
                    totalsInfo.PostageCost = root.TotalsInfo?.PostageCost ?? 0;
                    totalsInfo.PostageCostExTax = root.TotalsInfo?.PostageCostExTax ?? 0;
                    totalsInfo.Tax = root.TotalsInfo?.Tax ?? 0;
                    totalsInfo.TotalCharge = root.TotalsInfo?.TotalCharge ?? 0;
                    totalsInfo.PaymentMethod = string.IsNullOrWhiteSpace(root.TotalsInfo?.PaymentMethod) ? "N/A" : root.TotalsInfo.PaymentMethod;
                    totalsInfo.PaymentMethodId = root.TotalsInfo?.PaymentMethodId ?? Guid.Empty;
                    totalsInfo.ProfitMargin = root.TotalsInfo?.ProfitMargin ?? 0;
                    totalsInfo.TotalDiscount = root.TotalsInfo?.TotalDiscount ?? 0;
                    totalsInfo.Currency = string.IsNullOrWhiteSpace(root.TotalsInfo?.Currency) ? "N/A" : root.TotalsInfo.Currency;
                    totalsInfo.CountryTaxRate = root.TotalsInfo?.CountryTaxRate ?? 0;
                    totalsInfo.ConversionRate = root.TotalsInfo?.ConversionRate ?? 1;
                    totalsInfo.CreatedAt = DateTime.UtcNow;
                    totalsInfo.UpdatedAt = null;

                    root.TotalsInfo = totalsInfo;

                    var taxInfo = root.TaxInfo ?? new TaxInfo();

                    taxInfo.TaxInfoId = Guid.NewGuid();
                    taxInfo.TaxNumber = string.IsNullOrWhiteSpace(root.TaxInfo?.TaxNumber) ? "N/A" : root.TaxInfo.TaxNumber;
                    taxInfo.CreatedAt = DateTime.UtcNow;
                    taxInfo.UpdatedAt = null;

                    root.TaxInfo = taxInfo;

                    var fulfillment = root.Fulfillment ?? new Fulfillment();

                    fulfillment.Id = Guid.NewGuid();
                    fulfillment.FulfillmentState = string.IsNullOrWhiteSpace(root.Fulfillment?.FulfillmentState) ? "Unknown" : root.Fulfillment.FulfillmentState;
                    fulfillment.PurchaseOrderState = string.IsNullOrWhiteSpace(root.Fulfillment?.PurchaseOrderState) ? "N/A" : root.Fulfillment.PurchaseOrderState;
                    fulfillment.CreatedAt = DateTime.UtcNow;
                    fulfillment.UpdatedAt = null;

                    root.Fulfillment = fulfillment;

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
                        var request = new GetStockItemsByIdsRequest
                        {
                            StockItemIds = new List<Guid> { i.StockItemId ?? Guid.Empty }
                        };

                        // Call the API
                        var itemdata = obj.Api.Stock.GetStockItemsByIds(request);



                        var item = new Rishvi.Models.Item();

                        item.Id = Guid.NewGuid();
                        item.ItemId = i.ItemId ?? "";
                        item.ItemNumber = i.ItemNumber ?? "";
                        item.SKU = i.SKU ?? "";
                        item.Title = i.Title ?? "";
                        item.Quantity = i.Quantity;
                        item.CategoryName = i.CategoryName ?? "";
                        item.StockLevelsSpecified = i.StockLevelsSpecified ?? false;
                        item.OnOrder = i.OnOrder ?? 0;
                        item.InOrderBook = i.InOrderBook ?? 0;
                        item.Level = i.Level ?? 0;
                        item.MinimumLevel = i.MinimumLevel ?? 0;
                        item.AvailableStock = i.AvailableStock ?? 0;
                        item.PricePerUnit = i.PricePerUnit ?? 0;
                        item.UnitCost = i.UnitCost ?? 0;
                        item.Cost = i.Cost ?? 0;
                        item.CostIncTax = i.CostIncTax ?? 0;
                        item.Weight = i.Weight ?? 0;
                        item.BarcodeNumber = i.BarcodeNumber ?? "";
                        item.ChannelSKU = i.ChannelSKU ?? "";
                        item.ChannelTitle = i.ChannelTitle ?? "";
                        item.BinRack = i.BinRack ?? "";
                        item.ImageId = i.ImageId ?? "";
                        item.RowId = i.RowId ?? Guid.NewGuid();
                        item.OrderId = order.OrderId;
                        item.StockItemId = i.StockItemId;
                        item.StockItemIntId = i.StockItemIntId ?? 0;
                        item.CreatedAt = DateTime.UtcNow;
                        item.UpdatedAt = null;
                        if (itemdata != null && itemdata.Items.Count > 0)
                        {


                            item.Height = (decimal?)itemdata.Items[0].Height;
                            item.width = (decimal?)itemdata.Items[0].Width;
                            item.Length = (decimal?)itemdata.Items[0].Depth;
                        }

                        item.CompositeSubItems = new List<Rishvi.Models.Item>();


                        // Process composite sub-items
                        foreach (var c in i.CompositeSubItems ?? new List<Rishvi.Models.Item>())
                        {
                            var requestsub = new GetStockItemsByIdsRequest
                            {
                                StockItemIds = new List<Guid> { i.StockItemId ?? Guid.Empty }
                            };

                            // Call the API
                            var itemdatasub = obj.Api.Stock.GetStockItemsByIds(requestsub);

                            var subItem = new Rishvi.Models.Item();

                            subItem.Id = Guid.NewGuid();
                            subItem.ItemId = c.ItemId ?? "";
                            subItem.ItemNumber = c.ItemNumber ?? "";
                            subItem.SKU = c.SKU ?? "";
                            subItem.Title = c.Title ?? "";
                            subItem.Quantity = c.Quantity;
                            subItem.CategoryName = c.CategoryName ?? "";
                            subItem.StockLevelsSpecified = c.StockLevelsSpecified ?? false;
                            subItem.OnOrder = c.OnOrder ?? 0;
                            subItem.InOrderBook = c.InOrderBook ?? 0;
                            subItem.Level = c.Level ?? 0;
                            subItem.MinimumLevel = c.MinimumLevel ?? 0;
                            subItem.AvailableStock = c.AvailableStock ?? 0;
                            subItem.PricePerUnit = c.PricePerUnit ?? 0;
                            subItem.UnitCost = c.UnitCost ?? 0;
                            subItem.Cost = c.Cost ?? 0;
                            subItem.CostIncTax = c.CostIncTax ?? 0;
                            subItem.Weight = c.Weight ?? 0;
                            subItem.BarcodeNumber = c.BarcodeNumber ?? "";
                            subItem.ChannelSKU = c.ChannelSKU ?? "";
                            subItem.ChannelTitle = c.ChannelTitle ?? "";
                            subItem.BinRack = c.BinRack ?? "";
                            subItem.ImageId = c.ImageId ?? "";
                            subItem.RowId = c.RowId ?? Guid.NewGuid();
                            subItem.OrderId = order.OrderId;
                            subItem.StockItemId = c.StockItemId;
                            subItem.StockItemIntId = c.StockItemIntId ?? 0;
                            subItem.CreatedAt = DateTime.UtcNow;
                            subItem.UpdatedAt = null;

                            if (itemdatasub != null && itemdatasub.Items.Count > 0)
                            {


                                item.Height = (decimal?)itemdatasub.Items[0].Height;
                                item.width = (decimal?)itemdatasub.Items[0].Width;
                                item.Length = (decimal?)itemdatasub.Items[0].Depth;
                            }


                            subItem.CompositeSubItems = new List<Rishvi.Models.Item>();


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
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"❌ Failed to insert order: {ex.Message}");
                    _unitOfWork.Context.ChangeTracker.Clear(); // EF Core >=5
                    SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, null, json, "InsertOrderFromJson", ex.Message, true, "clientId");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error inserting order: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("🔍 Inner: " + ex.InnerException.Message);
                SqlHelper.SystemLogInsert("TradingApiOAuthHelper", null, null, json, "InsertOrderFromJson", ex.Message, true, "clientId");
            }
        }
        public async Task UpdateOrderRootFullAsync(OrderRoot updatedOrder)
        {
            var existingOrder = await _dbSqlCContext.OrderRoot
                .Include(o => o.GeneralInfo)
                .Include(o => o.ShippingInfo)
                .Include(o => o.CustomerInfo).ThenInclude(c => c.Address)
                .Include(o => o.CustomerInfo).ThenInclude(c => c.BillingAddress)
                .Include(o => o.TotalsInfo)
                .Include(o => o.TaxInfo)
                .Include(o => o.Fulfillment)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.NumOrderId == updatedOrder.NumOrderId);

            if (existingOrder == null)
            {
                throw new Exception($"Order with NumOrderId {updatedOrder.NumOrderId} not found.");
            }

            // =========================
            // Update OrderRoot
            // =========================
            existingOrder.FolderName = updatedOrder.FolderName ?? new List<string>();
            existingOrder.IsPostFilteredOut = updatedOrder.IsPostFilteredOut ?? false;
            existingOrder.CanFulfil = updatedOrder.CanFulfil ?? false;
            existingOrder.HasItems = updatedOrder.HasItems ?? false;
            existingOrder.TotalItemsSum = updatedOrder.TotalItemsSum ?? 0;
            existingOrder.TempColumn = "placeholder";
            existingOrder.UpdatedAt = DateTime.UtcNow;


            // =========================
            // GeneralInfo
            // =========================
            if (existingOrder.GeneralInfo != null && updatedOrder.GeneralInfo != null)
            {
                existingOrder.GeneralInfo.Status = updatedOrder.GeneralInfo.Status ?? 0;
                existingOrder.GeneralInfo.LabelPrinted = updatedOrder.GeneralInfo.LabelPrinted ?? false;
                existingOrder.GeneralInfo.LabelError = updatedOrder.GeneralInfo.LabelError ?? "";
                existingOrder.GeneralInfo.InvoicePrinted = updatedOrder.GeneralInfo.InvoicePrinted ?? false;
                existingOrder.GeneralInfo.PickListPrinted = updatedOrder.GeneralInfo.PickListPrinted ?? false;
                existingOrder.GeneralInfo.IsRuleRun = updatedOrder.GeneralInfo.IsRuleRun ?? false;
                existingOrder.GeneralInfo.Notes = updatedOrder.GeneralInfo.Notes ?? 0;
                existingOrder.GeneralInfo.PartShipped = updatedOrder.GeneralInfo.PartShipped ?? false;
                existingOrder.GeneralInfo.Marker = updatedOrder.GeneralInfo.Marker ?? 0;
                existingOrder.GeneralInfo.IsParked = updatedOrder.GeneralInfo.IsParked ?? false;
                existingOrder.GeneralInfo.ReferenceNum = updatedOrder.GeneralInfo.ReferenceNum ?? "";
                existingOrder.GeneralInfo.SecondaryReference = updatedOrder.GeneralInfo.SecondaryReference ?? "";
                existingOrder.GeneralInfo.ExternalReferenceNum = updatedOrder.GeneralInfo.ExternalReferenceNum ?? "";
                existingOrder.GeneralInfo.ReceivedDate = updatedOrder.GeneralInfo.ReceivedDate ?? DateTime.UtcNow;
                existingOrder.GeneralInfo.Source = updatedOrder.GeneralInfo.Source ?? "N/A";
                existingOrder.GeneralInfo.SubSource = updatedOrder.GeneralInfo.SubSource ?? "N/A";
                existingOrder.GeneralInfo.SiteCode = updatedOrder.GeneralInfo.SiteCode ?? "N/A";
                existingOrder.GeneralInfo.HoldOrCancel = updatedOrder.GeneralInfo.HoldOrCancel ?? false;
                existingOrder.GeneralInfo.DespatchByDate = updatedOrder.GeneralInfo.DespatchByDate ?? DateTime.UtcNow;
                existingOrder.GeneralInfo.HasScheduledDelivery = updatedOrder.GeneralInfo.HasScheduledDelivery ?? false;
                existingOrder.GeneralInfo.Location = updatedOrder.GeneralInfo.Location ?? Guid.Empty;
                existingOrder.GeneralInfo.NumItems = updatedOrder.GeneralInfo.NumItems ?? 0;
                existingOrder.GeneralInfo.UpdatedAt = DateTime.UtcNow;


            }

            // =========================
            // ShippingInfo
            // =========================
            if (existingOrder.ShippingInfo != null && updatedOrder.ShippingInfo != null)
            {
                existingOrder.ShippingInfo.Vendor = updatedOrder.ShippingInfo.Vendor ?? "N/A";
                existingOrder.ShippingInfo.PostalServiceId = updatedOrder.ShippingInfo.PostalServiceId ?? Guid.Empty;
                existingOrder.ShippingInfo.PostalServiceName = updatedOrder.ShippingInfo.PostalServiceName ?? "N/A";
                existingOrder.ShippingInfo.TotalWeight = updatedOrder.ShippingInfo.TotalWeight ?? 0;
                existingOrder.ShippingInfo.ItemWeight = updatedOrder.ShippingInfo.ItemWeight ?? 0;
                existingOrder.ShippingInfo.PackageCategoryId = updatedOrder.ShippingInfo.PackageCategoryId ?? Guid.Empty;
                existingOrder.ShippingInfo.PackageCategory = updatedOrder.ShippingInfo.PackageCategory ?? "Default";
                existingOrder.ShippingInfo.PackageTypeId = updatedOrder.ShippingInfo.PackageTypeId ?? Guid.Empty;
                existingOrder.ShippingInfo.PackageType = updatedOrder.ShippingInfo.PackageType ?? "Default";
                existingOrder.ShippingInfo.PostageCost = updatedOrder.ShippingInfo.PostageCost ?? 0;
                existingOrder.ShippingInfo.PostageCostExTax = updatedOrder.ShippingInfo.PostageCostExTax ?? 0;
                existingOrder.ShippingInfo.TrackingNumber = updatedOrder.ShippingInfo.TrackingNumber ?? "";
                existingOrder.ShippingInfo.ManualAdjust = updatedOrder.ShippingInfo.ManualAdjust ?? false;
                existingOrder.ShippingInfo.UpdatedAt = DateTime.UtcNow;


            }

            // =========================
            // CustomerInfo
            // =========================
            if (existingOrder.CustomerInfo != null && updatedOrder.CustomerInfo != null)
            {
                existingOrder.CustomerInfo.ChannelBuyerName = updatedOrder.CustomerInfo.ChannelBuyerName;
                existingOrder.CustomerInfo.UpdatedAt = DateTime.UtcNow;

                if (existingOrder.CustomerInfo.Address != null && updatedOrder.CustomerInfo.Address != null)
                {
                    var addr = updatedOrder.CustomerInfo.Address;
                    existingOrder.CustomerInfo.Address.EmailAddress = addr.EmailAddress ?? "";
                    existingOrder.CustomerInfo.Address.Address1 = addr.Address1 ?? "";
                    existingOrder.CustomerInfo.Address.Address2 = addr.Address2 ?? "";
                    existingOrder.CustomerInfo.Address.Address3 = addr.Address3 ?? "";
                    existingOrder.CustomerInfo.Address.Town = addr.Town ?? "";
                    existingOrder.CustomerInfo.Address.Region = addr.Region ?? "";
                    existingOrder.CustomerInfo.Address.PostCode = addr.PostCode ?? "";
                    existingOrder.CustomerInfo.Address.Country = addr.Country ?? "";
                    existingOrder.CustomerInfo.Address.Continent = addr.Continent ?? "N/A";
                    existingOrder.CustomerInfo.Address.FullName = addr.FullName ?? "";
                    existingOrder.CustomerInfo.Address.Company = addr.Company ?? "";
                    existingOrder.CustomerInfo.Address.PhoneNumber = addr.PhoneNumber ?? "";
                    existingOrder.CustomerInfo.Address.CountryId = addr.CountryId ?? Guid.Empty;
                    existingOrder.CustomerInfo.Address.UpdatedAt = DateTime.UtcNow;

                }

                if (existingOrder.CustomerInfo.BillingAddress != null && updatedOrder.CustomerInfo.BillingAddress != null)
                {
                    var bill = updatedOrder.CustomerInfo.BillingAddress;
                    existingOrder.CustomerInfo.BillingAddress.EmailAddress = bill.EmailAddress ?? "";
                    existingOrder.CustomerInfo.BillingAddress.Address1 = bill.Address1 ?? "";
                    existingOrder.CustomerInfo.BillingAddress.Address2 = bill.Address2 ?? "";
                    existingOrder.CustomerInfo.BillingAddress.Address3 = bill.Address3 ?? "";
                    existingOrder.CustomerInfo.BillingAddress.Town = bill.Town ?? "";
                    existingOrder.CustomerInfo.BillingAddress.Region = bill.Region ?? "";
                    existingOrder.CustomerInfo.BillingAddress.PostCode = bill.PostCode ?? "";
                    existingOrder.CustomerInfo.BillingAddress.Country = bill.Country ?? "";
                    existingOrder.CustomerInfo.BillingAddress.Continent = bill.Continent ?? "N/A";
                    existingOrder.CustomerInfo.BillingAddress.FullName = bill.FullName ?? "";
                    existingOrder.CustomerInfo.BillingAddress.Company = bill.Company ?? "";
                    existingOrder.CustomerInfo.BillingAddress.PhoneNumber = bill.PhoneNumber ?? "";
                    existingOrder.CustomerInfo.BillingAddress.CountryId = bill.CountryId ?? Guid.Empty;
                    existingOrder.CustomerInfo.BillingAddress.UpdatedAt = DateTime.UtcNow;
                }
            }

            // =========================
            // TotalsInfo
            // =========================
            if (existingOrder.TotalsInfo != null && updatedOrder.TotalsInfo != null)
            {
                var tot = updatedOrder.TotalsInfo;
                existingOrder.TotalsInfo.Subtotal = tot.Subtotal ?? 0;
                existingOrder.TotalsInfo.PostageCost = tot.PostageCost ?? 0;
                existingOrder.TotalsInfo.PostageCostExTax = tot.PostageCostExTax ?? 0;
                existingOrder.TotalsInfo.Tax = tot.Tax ?? 0;
                existingOrder.TotalsInfo.TotalCharge = tot.TotalCharge ?? 0;
                existingOrder.TotalsInfo.PaymentMethod = tot.PaymentMethod ?? "N/A";
                existingOrder.TotalsInfo.PaymentMethodId = tot.PaymentMethodId ?? Guid.Empty;
                existingOrder.TotalsInfo.ProfitMargin = tot.ProfitMargin ?? 0;
                existingOrder.TotalsInfo.TotalDiscount = tot.TotalDiscount ?? 0;
                existingOrder.TotalsInfo.Currency = tot.Currency ?? "N/A";
                existingOrder.TotalsInfo.CountryTaxRate = tot.CountryTaxRate ?? 0;
                existingOrder.TotalsInfo.ConversionRate = tot.ConversionRate ?? 1;
                existingOrder.TotalsInfo.UpdatedAt = DateTime.UtcNow;


            }

            // =========================
            // TaxInfo
            // =========================
            if (existingOrder.TaxInfo != null && updatedOrder.TaxInfo != null)
            {
                existingOrder.TaxInfo.TaxNumber = updatedOrder.TaxInfo.TaxNumber ?? "N/A";
                existingOrder.TaxInfo.UpdatedAt = DateTime.UtcNow;
            }

            // =========================
            // Fulfillment
            // =========================
            if (existingOrder.Fulfillment != null && updatedOrder.Fulfillment != null)
            {
                existingOrder.Fulfillment.FulfillmentState = updatedOrder.Fulfillment.FulfillmentState ?? "Unknown";
                existingOrder.Fulfillment.PurchaseOrderState = updatedOrder.Fulfillment.PurchaseOrderState ?? "N/A";
                existingOrder.Fulfillment.UpdatedAt = DateTime.UtcNow;
            }

            // =========================
            // Items & CompositeSubItems
            // =========================

            if (existingOrder.Items != null && updatedOrder.Items != null)
            {
                foreach (var updatedItem in updatedOrder.Items)
                {
                    var existingItem = existingOrder.Items.FirstOrDefault(i => i.ItemId == updatedItem.ItemId);
                    if (existingItem != null)
                    {
                        existingItem.ItemNumber = updatedItem.ItemNumber ?? "";
                        existingItem.SKU = updatedItem.SKU ?? "";
                        existingItem.Title = updatedItem.Title ?? "";
                        existingItem.Quantity = updatedItem.Quantity;
                        existingItem.CategoryName = updatedItem.CategoryName ?? "";
                        existingItem.StockLevelsSpecified = updatedItem.StockLevelsSpecified ?? false;
                        existingItem.OnOrder = updatedItem.OnOrder ?? 0;
                        existingItem.InOrderBook = updatedItem.InOrderBook ?? 0;
                        existingItem.Level = updatedItem.Level ?? 0;
                        existingItem.MinimumLevel = updatedItem.MinimumLevel ?? 0;
                        existingItem.AvailableStock = updatedItem.AvailableStock ?? 0;
                        existingItem.PricePerUnit = updatedItem.PricePerUnit ?? 0;
                        existingItem.UnitCost = updatedItem.UnitCost ?? 0;
                        existingItem.Cost = updatedItem.Cost ?? 0;
                        existingItem.CostIncTax = updatedItem.CostIncTax ?? 0;
                        existingItem.Weight = updatedItem.Weight ?? 0;
                        existingItem.BarcodeNumber = updatedItem.BarcodeNumber ?? "";
                        existingItem.ChannelSKU = updatedItem.ChannelSKU ?? "";
                        existingItem.ChannelTitle = updatedItem.ChannelTitle ?? "";
                        existingItem.BinRack = updatedItem.BinRack ?? "";
                        existingItem.ImageId = updatedItem.ImageId ?? "";
                        existingItem.RowId = updatedItem.RowId ?? Guid.NewGuid();
                        existingItem.StockItemId = updatedItem.StockItemId ?? Guid.NewGuid();
                        existingItem.StockItemIntId = updatedItem.StockItemIntId ?? 0;
                        existingItem.UpdatedAt = DateTime.UtcNow;
                        existingItem.Height = updatedItem.Height ?? 0;
                        existingItem.width = updatedItem.width ?? 0;
                        existingItem.Length = updatedItem.Length ?? 0;

                        if (existingItem.CompositeSubItems != null && updatedItem.CompositeSubItems != null)
                        {
                            foreach (var updatedSub in updatedItem.CompositeSubItems)
                            {
                                var existingSub = existingItem.CompositeSubItems
                                    .FirstOrDefault(s => s.ItemId == updatedSub.ItemId);

                                if (existingSub != null)
                                {
                                    existingSub.ItemNumber = updatedSub.ItemNumber ?? "";
                                    existingSub.SKU = updatedSub.SKU ?? "";
                                    existingSub.Title = updatedSub.Title ?? "";
                                    existingSub.Quantity = updatedSub.Quantity;
                                    existingSub.CategoryName = updatedSub.CategoryName ?? "";
                                    existingSub.StockLevelsSpecified = updatedSub.StockLevelsSpecified ?? false;
                                    existingSub.OnOrder = updatedSub.OnOrder ?? 0;
                                    existingSub.InOrderBook = updatedSub.InOrderBook ?? 0;
                                    existingSub.Level = updatedSub.Level ?? 0;
                                    existingSub.MinimumLevel = updatedSub.MinimumLevel ?? 0;
                                    existingSub.AvailableStock = updatedSub.AvailableStock ?? 0;
                                    existingSub.PricePerUnit = updatedSub.PricePerUnit ?? 0;
                                    existingSub.UnitCost = updatedSub.UnitCost ?? 0;
                                    existingSub.Cost = updatedSub.Cost ?? 0;
                                    existingSub.CostIncTax = updatedSub.CostIncTax ?? 0;
                                    existingSub.Weight = updatedSub.Weight ?? 0;
                                    existingSub.BarcodeNumber = updatedSub.BarcodeNumber ?? "";
                                    existingSub.ChannelSKU = updatedSub.ChannelSKU ?? "";
                                    existingSub.ChannelTitle = updatedSub.ChannelTitle ?? "";
                                    existingSub.BinRack = updatedSub.BinRack ?? "";
                                    existingSub.ImageId = updatedSub.ImageId ?? "";
                                    existingSub.RowId = updatedSub.RowId ?? Guid.NewGuid();
                                    existingSub.StockItemId = updatedSub.StockItemId ?? Guid.NewGuid();
                                    existingSub.StockItemIntId = updatedSub.StockItemIntId ?? 0;
                                    existingSub.UpdatedAt = DateTime.UtcNow;
                                    existingSub.Height = updatedItem.Height ?? 0;
                                    existingSub.width = updatedItem.width ?? 0;
                                    existingSub.Length = updatedItem.Length ?? 0;

                                }
                            }
                        }
                    }
                    else
                    {
                        var item = new Rishvi.Models.Item
                        {

                            Id = Guid.NewGuid(),
                            ItemId = updatedItem.ItemId ?? "",
                            ItemNumber = updatedItem.ItemNumber ?? "",
                            SKU = updatedItem.SKU ?? "",
                            Title = updatedItem.Title ?? "",
                            Quantity = updatedItem.Quantity,
                            CategoryName = updatedItem.CategoryName ?? "",
                            StockLevelsSpecified = updatedItem.StockLevelsSpecified ?? false,
                            OnOrder = updatedItem.OnOrder ?? 0,
                            InOrderBook = updatedItem.InOrderBook ?? 0,
                            Level = updatedItem.Level ?? 0,
                            MinimumLevel = updatedItem.MinimumLevel ?? 0,
                            AvailableStock = updatedItem.AvailableStock ?? 0,
                            PricePerUnit = updatedItem.PricePerUnit ?? 0,
                            UnitCost = updatedItem.UnitCost ?? 0,
                            Cost = updatedItem.Cost ?? 0,
                            CostIncTax = updatedItem.CostIncTax ?? 0,
                            Weight = updatedItem.Weight ?? 0,
                            BarcodeNumber = updatedItem.BarcodeNumber ?? "",
                            ChannelSKU = updatedItem.ChannelSKU ?? "",
                            ChannelTitle = updatedItem.ChannelTitle ?? "",
                            BinRack = updatedItem.BinRack ?? "",
                            ImageId = updatedItem.ImageId ?? "",
                            RowId = updatedItem.RowId ?? Guid.NewGuid(),
                            OrderId = existingOrder.OrderId,
                            StockItemId = updatedItem.StockItemId,
                            StockItemIntId = updatedItem.StockItemIntId ?? 0,
                            Height = updatedItem.Height ?? 0,
                            width = updatedItem.width ?? 0,
                            Length = updatedItem.Length ?? 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = null,
                            CompositeSubItems = new List<Rishvi.Models.Item>()
                        };

                        // Process composite sub-items
                        foreach (var c in updatedItem.CompositeSubItems ?? new List<Rishvi.Models.Item>())
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
                                OrderId = existingOrder.OrderId,
                                StockItemId = c.StockItemId,
                                StockItemIntId = c.StockItemIntId ?? 0,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = null,
                                Height = c.Height ?? 0,
                                width = c.width ?? 0,
                                Length = c.Length ?? 0,
                                CompositeSubItems = new List<Rishvi.Models.Item>()
                            };


                            _dbSqlCContext.Item.Add(subItem);

                        }

                        existingOrder.Items.Add(item);

                        _dbSqlCContext.Item.Add(item);
                    }
                }
            }

            await _dbSqlCContext.SaveChangesAsync();
        }


        public async Task SaveLinnDispatch(string s, string AuthorizationToken, string email, string linntoken, int linnorderid)
        {
            // var stream = new MemoryStream();
            // StreamWriter sw = new StreamWriter(stream);
            // sw.Write(s);
            // sw.Flush();
            // stream.Position = 0;
            // AwsS3.UploadFileToS3("Authorization", stream, "LinnDispatch/" + AuthorizationToken.ToString() + "_linndispatch_" + linnorderid + ".json");
            //

            InsertOrderFromJson(s, linntoken);



            var existingReport = await _dbSqlCContext.ReportModel
                .FirstOrDefaultAsync(d => d.LinnNumOrderId == linnorderid.ToString() && d.email == email);

            if (existingReport == null)
            {
                var newReport = new ReportModel()
                {
                    _id = Guid.NewGuid().ToString(),
                    AuthorizationToken = AuthorizationToken,
                    createdDate = DateTime.Now,
                    updatedDate = DateTime.Now,
                    DispatchLinnOrderFromStream = DateTime.Now,
                    IsLinnOrderDispatchFromStream = true,
                    DispatchOrderInLinnJson = $"LinnDispatch/{AuthorizationToken}_linndispatch_{linnorderid}.json",
                    EbayChannelOrderRef = linnorderid.ToString(),
                    LinnNumOrderId = linnorderid.ToString(),
                    email = email
                };

                await SaveReportDataForTEst(new List<ReportModel> { newReport });
            }
            else
            {
                existingReport.updatedDate = DateTime.Now;
                existingReport.DispatchOrderInLinnJson = $"LinnDispatch/{AuthorizationToken}_linndispatch_{linnorderid}.json";
                existingReport.IsLinnOrderDispatchFromStream = true;
                existingReport.DispatchLinnOrderFromStream = DateTime.Now;

                await SaveReportDataForTEst(new List<ReportModel> { existingReport });
            }

        }


        public async Task SaveWebhook(string s, string AuthorizationToken, string reference = "")
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "Webhook/" + AuthorizationToken.ToString() + "_webhook_" + reference + ".json");

        }
        public static void SaveLogs(string s, string AuthorizationToken, string reference = "")
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "InstallLogs/" + AuthorizationToken.ToString() + "_installlogs_" + reference + ".json");

        }
        public RegistrationData GetRegistrationData(string email)
        {
            var registrationData = new RegistrationData();
            var integrationSettings = _dbContext.Authorizations.Where(x => x.Email == email).FirstOrDefault();
            //remove    registrationData.Name = integrationSettings?.Name ?? string.Empty;
            //remove   registrationData.Email = integrationSettings?.Email ?? string.Empty;
            //remove   registrationData.Password = integrationSettings?.Password ?? string.Empty;
            //remove   registrationData.AuthorizationToken = integrationSettings?.AuthorizationToken ?? string.Empty;
            //remove   registrationData.LinnworksSyncToken = integrationSettings?.LinnworksSyncToken ?? string.Empty;

            registrationData.LastSyncOnDate = integrationSettings?.UpdatedAt ?? DateTime.MinValue;
            registrationData.LastSyncOn = integrationSettings?.UpdatedAt.ToString() ?? DateTime.MinValue.ToString();
            //remove  registrationData.ebaypage = integrationSettings?.ebaypage ?? 1;
            //remove   registrationData.ebayhour = integrationSettings?.ebayhour ?? 0;
            registrationData.linnhour = integrationSettings?.LinnDays * 24 ?? 0;
            registrationData.linnpage = integrationSettings?.LinnPage ?? 1;
            return registrationData;
        }
        #endregion
    }

}
