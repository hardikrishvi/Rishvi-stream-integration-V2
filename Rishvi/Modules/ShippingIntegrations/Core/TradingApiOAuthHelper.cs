using LinnworksAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using System.Net;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using Item = Rishvi.Modules.ShippingIntegrations.Models.Item;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.ShippingIntegrations.Api;
using Microsoft.Extensions.Options;
using Rishvi.Models;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.Core.Data;
using Address = Rishvi.Models.Address;
using static Rishvi.Modules.ShippingIntegrations.Models.WebhookResponse;
using Formatting = Newtonsoft.Json.Formatting;

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

        public TradingApiOAuthHelper(ReportsController reportsController, SetupController setupController, IOptions<CourierSettings> courierSettings, ApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
            IRepository<Address> address, IRepository<CustomerInfo> customerInfo, IRepository<Fulfillment> fulfillment,
            IRepository<GeneralInfo> generalInfo, IRepository<OrderRoot> orderRoot, IRepository<ShippingInfo> shippingInfo,
            IRepository<TaxInfo> taxInfo, IRepository<TotalsInfo> totalsInfo, IRepository<Rishvi.Models.Item> item,
            IRepository<IntegrationSettings> integrationSettings, IRepository<LinnworksSettings> linnworksSettings, IRepository<Rishvi.Models.StreamSettings> streamSettings,
            IRepository<Rishvi.Models.SyncSettings> syncSettings,IRepository<Rishvi.Models.Ebay> ebay, SqlContext dbSqlCContext, ManageToken manageToken)

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
            _IntegrationSettings= integrationSettings; 
            _LinnworksSettings =  linnworksSettings;
            _StreamSettings    =  streamSettings;
            _SyncSettings      = syncSettings;
            _Ebay      = ebay;
            _manageToken = manageToken;
        }

        #region Ebay Api Function
        public string GenerateToken(string str1, string str2, string secretKey)
        {
            // Combine both strings
            string combinedString = str1 + str2;

            // Create HMACSHA256 hash with the secret key
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] tokenBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
                return Convert.ToBase64String(tokenBytes).Replace("+", "").Replace("/", "").Replace("=", "");
            }
        }
        
        
        public async Task<string> HttpPostXMLAsync(string XMLData, string URL, Dictionary<string, string> Header, bool DeleteFile = false)
        {
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(URL);
            webRequest.Method = "POST";
            webRequest.ContentType = "text/xml";
            string apiCallNAme = "";
            foreach (var keyVal in Header)
            {
                if (keyVal.Key == "X-EBAY-API-CALL-NAME")
                {
                    apiCallNAme = keyVal.Value;
                }
                webRequest.Headers.Add(keyVal.Key, keyVal.Value);
            }
            using (new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    streamWriter.Write(XMLData);
                    streamWriter.Close();
                }
            }
            using (HttpWebResponse webResponse = (HttpWebResponse)await webRequest.GetResponseAsync())
            {
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    string EbayResponse = await sr.ReadToEndAsync();

                    return EbayResponse;
                }
            }
        }
        
        #endregion
        
        

        
        #region Linnworks Get Order and Dispatch Function
        
        
        public async Task UpdateOrderExProperty(string linntoken, int orderid, Dictionary<string, string> values)
        {
            var obj = new LinnworksBaseStream(linntoken);
            var linnorderid = obj.Api.Orders.GetOrderDetailsByNumOrderId(orderid);
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
        }

        // Fix for CS0120: An object reference is required for the non-static field, method, or property 'CourierSettings.SelectedServiceId'

        // Assuming that `CourierSettings` is intended to be instantiated and used as an object, 
        // we need to create an instance of `CourierSettings` and access its `SelectedServiceId` property.

        public async Task UpdateLinnworksOrdersToStream(AuthorizationConfigClass auth, string OrderId, string StreamOrderId)
        {
            List<CourierService> services = Services.GetServices;
            var streamAuth = _manageToken.GetToken(auth);
            //var manageToken = new ManageToken(_ClientAuth, _unitOfWork);
            //var streamAuth = manageToken.GetToken(auth);

            CourierService selectedService = services.Find(s => s.ServiceUniqueId == CourierSettings.SelectedServiceId);
            bool existsInDb = _dbSqlCContext.ReportModel
                .Any(x => x.LinnNumOrderId == OrderId);

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
                        
                    }, auth.ClientId, streamAuth.AccessToken, selectedService, true, jsopndata.ShippingInfo.PostalServiceName.ToLower().Contains("pickup") ? "COLLECTION" : "DELIVERY", StreamOrderId);
                    streamOrderResponse.Item1.AuthorizationToken = auth.AuthorizationToken;
                    streamOrderResponse.Item1.ItemId = "";
                    if (streamOrderResponse.Item1.response == null)
                    {
                        await SaveStreamOrder(streamOrderResponse.Item2, auth.AuthorizationToken.ToString(), auth.Email, null, OrderId, "Error", "Error", "Error", OrderId);
                    }
                    else
                    {
                        await SaveStreamOrder(JsonConvert.SerializeObject(streamOrderResponse.Item1), auth.AuthorizationToken.ToString(), auth.Email, null, OrderId, streamOrderResponse.Item1.response.consignmentNo, streamOrderResponse.Item1.response.trackingId, streamOrderResponse.Item1.response.trackingURL, OrderId);
                    }
                }
                catch
                {

                }
            }
        }

        // Assuming that `CourierSettings` is intended to be instantiated and used as an object, 
        // we need to create an instance of `CourierSettings` and access its `SelectedServiceId` property.

        public async Task CreateLinnworksOrdersToStream(AuthorizationConfigClass auth, string OrderId)
        {
            

            // Proper Any query
            
            List<CourierService> services = Services.GetServices;
            var streamAuth = _manageToken.GetToken(auth);
            

            CourierService selectedService = services.Find(s => s.ServiceUniqueId == CourierSettings.SelectedServiceId);
            bool existsInDb = _dbSqlCContext.ReportModel
                .Any(x => x.LinnNumOrderId == OrderId);
            
            

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
                
                if (json != "null")
                {
                    try
                    {
                        var jsopndata = orderRoot;
                        int orderId = jsopndata.NumOrderId;
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
                                           Quantity = f.Quantity
                                       }).ToList()
                                  }},
                            ServiceConfigItems = new List<ServiceConfigItem>(),
                            OrderExtendedProperties = new List<Models.ExtendedProperty>(),
                            Phone = jsopndata.CustomerInfo.Address.PhoneNumber,
                            Region = jsopndata.CustomerInfo.Address.Region,
                            Town = jsopndata.CustomerInfo.Address.Town
                        }, auth.ClientId, streamAuth.AccessToken, selectedService, true, jsopndata.ShippingInfo.PostalServiceName.ToLower().Contains("pickup") ? "COLLECTION" : "DELIVERY", null);
                        streamOrderResponse.Item1.AuthorizationToken = auth.AuthorizationToken;
                        streamOrderResponse.Item1.ItemId = "";
                        if (streamOrderResponse.Item1.response == null)
                        {
                            await SaveStreamOrder(streamOrderResponse.Item2, auth.AuthorizationToken.ToString(), auth.Email, null, OrderId, "Error", "Error", "Error", OrderId);
                        }
                        else
                        {
                            await SaveStreamOrder(JsonConvert.SerializeObject(streamOrderResponse.Item1), auth.AuthorizationToken.ToString(), auth.Email, null, OrderId, streamOrderResponse.Item1.response.consignmentNo, streamOrderResponse.Item1.response.trackingId, streamOrderResponse.Item1.response.trackingURL, OrderId);
                        }
                    }
                    catch
                    {
                        throw new Exception("Order data not found for OrderId: " + OrderId);
                    }
                }
                else
                {
                        throw new Exception("Order data not found for OrderId: " + OrderId);
                }
            }
        }
        public async Task DispatchOrderInLinnworks(AuthorizationConfigClass _User, int OrderRef,string linntoken, 
            string Service, string TrackingNumber, string TrackingUrl,string dispatchdate)
        {
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
            await SaveLinnDispatch(JsonConvert.SerializeObject(orderdata), _User.AuthorizationToken.ToString(), _User.Email, OrderRef);

        }
        #endregion
        #region Stream Create Order & Get Order and Other Function
        public async Task CreateStreamWebhook(AuthorizationConfigClass _User, string eventname, string event_type, string url_path, string http_method, string content_type, string auth_header)
        {
            await _setupController.SubscribeWebhook(_User.AuthorizationToken, eventname, event_type, url_path, http_method, content_type, auth_header);
        }
        // this function is for save report data for specific user
        public async Task<StreamGetOrderResponse.Root> GetStreamOrder(AuthorizationConfigClass _User, string OrderId)
        {
            var streamAuth = _manageToken.GetToken(_User);
            //var manageToken = new ManageToken(_ClientAuth, _unitOfWork);
            //var streamAuth = manageToken.GetToken(_User);
            return StreamOrderApi.GetOrder(streamAuth.AccessToken, OrderId, _User.ClientId);
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
                    Stream =  stream,
                    Sync =  sync,
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
        public async Task SaveLinnOrder(string s, string AuthorizationToken, string email, string linnorderid = "")
        {
            InsertOrderFromJson(s);

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
                    NumOrderId = root.NumOrderId ,

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
                        Quantity = i.Quantity ,
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

                    // Process composite sub-items
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
                            UpdatedAt = null,
                            CompositeSubItems = new List<Rishvi.Models.Item>()
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
                Console.WriteLine("❌ Error inserting order: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("🔍 Inner: " + ex.InnerException.Message);
            }
        }
        
        
        
        
        public async Task SaveStreamOrder(string s, string AuthorizationToken, string email, string ebayorderid, string linnworksorderid, string consignmentid, string trackingnumber, string trackingurl, string order = "")
        {
            
            dynamic jsonData = JsonConvert.DeserializeObject(s);
            string extractedConsignmentNo = jsonData.response.consignmentNo;
            string extractedTrackingUrl = jsonData.response.trackingURL;
            string extractedTrackingId = jsonData.response.trackingId;

            var record = new StreamOrderRecord
            {
                Id = Guid.NewGuid(),
                JsonData = s,
                AuthorizationToken = AuthorizationToken,
                Email = email,
                EbayOrderId = ebayorderid,
                LinnworksOrderId = linnworksorderid,
                ConsignmentId = extractedConsignmentNo ?? consignmentid,
                TrackingNumber = trackingnumber,
                TrackingUrl = extractedTrackingUrl ?? trackingurl,
                TrackingId = extractedTrackingId,
                Order = order,
                CreatedAt = DateTime.UtcNow
            };
            _dbSqlCContext.StreamOrderRecord.Add(record);
            await _dbSqlCContext.SaveChangesAsync();
        
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
        
            // ✅ Now save to DB (no AWS S3 call)
            if (reportsToSave.Any())
            {
                await SaveReportDataForTEst(reportsToSave);
            }
        }
        
        public async Task SaveLinnDispatch(string s, string AuthorizationToken, string email, int linnorderid)
        {
            // var stream = new MemoryStream();
            // StreamWriter sw = new StreamWriter(stream);
            // sw.Write(s);
            // sw.Flush();
            // stream.Position = 0;
            // AwsS3.UploadFileToS3("Authorization", stream, "LinnDispatch/" + AuthorizationToken.ToString() + "_linndispatch_" + linnorderid + ".json");
            //

            InsertOrderFromJson(s);
            
            
            
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
        
        #endregion
        


    }

}
