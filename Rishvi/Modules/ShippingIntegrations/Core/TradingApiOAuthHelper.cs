﻿using LinnworksAPI;
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
        private readonly IRepository<Address> _Address;
        private readonly IRepository<CustomerInfo> _CustomerInfo;
        private readonly IRepository<Fulfillment> _Fulfillment;
        private readonly IRepository<GeneralInfo> _GeneralInfo;
        private readonly IRepository<OrderRoot> _OrderRoot;
        private readonly IRepository<ShippingInfo> _ShippingInfo;
        private readonly IRepository<TaxInfo> _TaxInfo;
        private readonly IRepository<TotalsInfo> _TotalsInfo;
        private readonly IRepository<Rishvi.Models.Item> _Item;
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly IRepository<Rishvi.Models.IntegrationSettings> _IntegrationSettings;
        private readonly IRepository<Rishvi.Models.LinnworksSettings> _LinnworksSettings;
        private readonly IRepository<Rishvi.Models.StreamSettings> _StreamSettings;
        private readonly IRepository<Rishvi.Models.SyncSettings> _SyncSettings;
        private readonly IRepository<Rishvi.Models.Ebay> _Ebay;

        public TradingApiOAuthHelper(ReportsController reportsController, SetupController setupController, IOptions<CourierSettings> courierSettings, ApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
            IRepository<Address> address, IRepository<CustomerInfo> customerInfo, IRepository<Fulfillment> fulfillment,
            IRepository<GeneralInfo> generalInfo, IRepository<OrderRoot> orderRoot, IRepository<ShippingInfo> shippingInfo,
            IRepository<TaxInfo> taxInfo, IRepository<TotalsInfo> totalsInfo, IRepository<Rishvi.Models.Item> item,
            IRepository<IntegrationSettings> integrationSettings, IRepository<LinnworksSettings> linnworksSettings, IRepository<Rishvi.Models.StreamSettings> streamSettings,
            IRepository<Rishvi.Models.SyncSettings> syncSettings,IRepository<Rishvi.Models.Ebay> ebay)

        {
            _reportsController = reportsController;
            _setupController = setupController;
            _dbContext = dbContext;
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
        public bool ValidateToken(string str1, string str2, string token, string secretKey)
        {
            // Generate a new token from the provided strings
            string generatedToken = GenerateToken(str1, str2, secretKey);

            // Compare the generated token with the provided token
            return generatedToken == token;
        }
        public string GetSessionID(string AuthToken, string ProdRedirectURL, string TradingAPI_ServerURL, string DeveloperId, string ProdClientId, string ProdClientSecret, string TradingAPI_Version)
        {
            XmlDocument xmlDoc;
            string error = "";
            string strReq = @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <GetSessionIDRequest xmlns=""urn:ebay:apis:eBLBaseComponents"">
                          <RuName>" + ProdRedirectURL + @"</RuName>
                        </GetSessionIDRequest>";

            xmlDoc = MakeAPICall(strReq, "GetSessionID", error, AuthToken, TradingAPI_ServerURL, DeveloperId, ProdClientId, ProdClientSecret, TradingAPI_Version);
            if (error == "")
            {
                XmlNode root = xmlDoc["GetSessionIDResponse"];
                if (root["Errors"] != null)
                {
                    string errorCode = root["Errors"]["ErrorCode"].InnerText;
                    string errorShort = root["Errors"]["ShortMessage"].InnerText;
                    string errorLong = root["Errors"]["LongMessage"].InnerText;
                    throw new Exception(errorCode + " ERROR: " + errorShort + "\n" + errorLong);
                }
                else
                {
                    return root["SessionID"].InnerText;
                }
            }
            else
            {
                throw new Exception(error);
            }
        }
        public async Task<EbayAuthentication> GenerateToken(AuthorizationConfigClass _User, string SessionID, string TradingAPI_ServerURL, string DeveloperId, string ProdClientId, string ProdClientSecret, string TradingAPI_Version)
        {
            string error = "";
            string strReq = @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <FetchTokenRequest xmlns=""urn:ebay:apis:eBLBaseComponents"">
                          <SessionID>" + SessionID + @"</SessionID>
                        </FetchTokenRequest>";

            XmlDocument xmlDoc = MakeAPICall(strReq, "FetchToken",
                error, _User.AuthorizationToken.ToString(), TradingAPI_ServerURL, DeveloperId, ProdClientId, ProdClientSecret, TradingAPI_Version);

            if (error == "")
            {
                XmlNode root = xmlDoc["FetchTokenResponse"];
                if (root["Errors"] != null)
                {
                    string errorCode = root["Errors"]["ErrorCode"].InnerText;
                    string errorShort = root["Errors"]["ShortMessage"].InnerText;
                    string errorLong = root["Errors"]["LongMessage"].InnerText;
                    throw new Exception(errorCode + " ERROR: " + errorShort + "\n" + errorLong);
                }
                else
                {

                    var EbayAuthenticationApiResponse = new EbayAuthentication
                    {
                        access_token = root["eBayAuthToken"].InnerText,
                        ExpirationTime = root["HardExpirationTime"].InnerText,
                        token_type = "User Access Token"
                    };
                    _User.IsConfigActive = true;
                    _User.IntegratedDateTime = DateTime.UtcNow;
                    _User.access_token = root["eBayAuthToken"].InnerText;
                    _User.ExpirationTime = root["HardExpirationTime"].InnerText;
                    _User.token_type = "User Access Token";
                    _User.Save();
                    return EbayAuthenticationApiResponse;
                }
            }
            else
            {
                throw new Exception(error);
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
        private XmlDocument MakeAPICall(string requestBody, string callname,
         string error, string AuthToken, string TradingAPI_ServerURL, string DeveloperId, string ProdClientId, string ProdClientSecret, string TradingAPI_Version)
        {
            XmlDocument xmlDoc = new XmlDocument();

            string APIServerURL = TradingAPI_ServerURL;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(APIServerURL);
            request.Headers.Add("X-EBAY-API-DEV-NAME", DeveloperId);
            request.Headers.Add("X-EBAY-API-APP-NAME", ProdClientId);
            request.Headers.Add("X-EBAY-API-CERT-NAME", ProdClientSecret);
            request.Headers.Add("X-EBAY-API-COMPATIBILITY-LEVEL", TradingAPI_Version);
            request.Headers.Add("X-EBAY-API-SITEID", "2");
            request.Headers.Add("X-EBAY-API-CALL-NAME", callname);
            request.Method = "POST";
            request.ContentType = "text/xml";
            try
            {
                using (new MemoryStream())
                {
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(requestBody);
                        streamWriter.Close();
                    }
                }
                using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                    {
                        string responsefromchanne = sr.ReadToEnd();
                        xmlDoc.LoadXml(responsefromchanne);
                        SaveLogs(responsefromchanne, AuthToken);
                    }
                }

            }
            catch (Exception Ex)
            {
                SaveLogs(Ex.ToString(), AuthToken);
                error = Ex.Message;
            }
            return xmlDoc;
        }
        #endregion
        #region Ebay Get Order and Order Dispatch Function
        //public async Task GetEbayOrdersFromApi(AuthorizationConfigClass _User, int FromHour, string ordersids,
        //  string webApiURL, string TradingAPI_Version,int PerPage)
        //{
        //    var orderlist = string.IsNullOrEmpty(ordersids)  ? new List<string>() : Regex.Split(ordersids, ",").ToList();
        //    string FromDate = DateTime.UtcNow.AddHours(-FromHour).ToString("yyyy-MM-dd") + "T00:00:00.000Z";
        //    string ToDate = DateTime.UtcNow.ToString("yyyy-MM-dd") + "T23:59:59.000Z";
        //    string PageTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\">"
        //                                        + @"<RequesterCredentials>
        //                                            <eBayAuthToken>##AuthToken##</eBayAuthToken>
        //                                          </RequesterCredentials>
	       //                                         <ErrorLanguage>en_US</ErrorLanguage>
	       //                                         <WarningLevel>High</WarningLevel> 
        //                                          ##FromTo##
        //                                          <OrderRole>Seller</OrderRole>
        //                                          ##OrderList##
        //                                          <OrderStatus>Completed</OrderStatus>
        //                                          <Pagination>
        //                                            <EntriesPerPage>##NumberOfRecord##</EntriesPerPage>
        //                                            <PageNumber>##PageNum##</PageNumber>
        //                                          </Pagination>
        //                                        </GetOrdersRequest>";
        //    string orderarrtemp = "";
        //    if (orderlist.Count > 0)
        //    {
        //        orderarrtemp = "<OrderIDArray>";
        //        foreach (var order in orderlist)
        //        {
        //            orderarrtemp += @"<OrderID>" + order + "</OrderID>";
        //        }
        //        orderarrtemp += "</OrderIDArray>";
        //    }
        //    string fromto = "";
        //    if (orderlist.Count == 0)
        //    {
        //        fromto = "<CreateTimeFrom>##FromDate##</CreateTimeFrom><CreateTimeTo>##ToDate##</CreateTimeTo>";
        //    }
        //    PageTemplate = PageTemplate.Replace("##FromTo##", fromto);
        //    PageTemplate = PageTemplate.Replace("##OrderList##", orderarrtemp);
        //    PageTemplate = PageTemplate.Replace("##AuthToken##", _User.access_token);
        //    PageTemplate = PageTemplate.Replace("##NumberOfRecord##", PerPage.ToString());
        //    PageTemplate = PageTemplate.Replace("##PageNum##", "1");
        //    PageTemplate = PageTemplate.Replace("##FromDate##", FromDate);
        //    PageTemplate = PageTemplate.Replace("##ToDate##", ToDate);
        //    XmlDocument ResponseXml = new XmlDocument();
        //    var ProductResp = await HttpPostXMLAsync(PageTemplate, webApiURL, new Dictionary<string, string>() {
        //            {"X-EBAY-API-SITEID","2" },
        //            {"X-EBAY-API-COMPATIBILITY-LEVEL",TradingAPI_Version },
        //            {"X-EBAY-API-CALL-NAME","GetOrders"}
        //               }, true);
        //    ResponseXml.LoadXml(ProductResp);

        //    XmlNode root = ResponseXml["GetOrdersResponse"];
        //    if (root["Errors"] != null)
        //    {
        //        string errorCode = root["Errors"]["ErrorCode"].InnerText;
        //    }
        //    if (root["Ack"] != null && root["Ack"].InnerText.Equals("Success", StringComparison.OrdinalIgnoreCase) || root["Ack"].InnerText.Equals("Warning", StringComparison.OrdinalIgnoreCase))
        //    {
        //        int TotalPage = Convert.ToInt32(root["PaginationResult"]["TotalNumberOfPages"].InnerText);
        //        if (root["PaginationResult"]["TotalNumberOfEntries"].InnerText != "0")
        //        {
        //            XmlNodeList _Orders = ((XmlElement)root).GetElementsByTagName("Order");
        //            foreach (XmlNode _Ord in _Orders)
        //            {
        //                if (!AwsS3.S3FileIsExists("Authorization", "EbayOrder/" + _User.AuthorizationToken.ToString() + "_ebayorder_" + _Ord["OrderID"].InnerText.ToString() + ".json").Result)
        //                {
        //                    var json = JsonConvert.SerializeXmlNode(_Ord);
        //                    try
        //                    {
        //                        var jsopndata = JsonConvert.DeserializeObject<EbayOrderResponse.Root>(json);
        //                        await SaveEbayOrder(json, _User.AuthorizationToken.ToString(), _User.Email, jsopndata.Order.TransactionArray.Transaction.OrderLineItemID, _Ord["OrderID"].InnerText.ToString());
        //                    }
        //                    catch
        //                    {
        //                        var jsopndata = JsonConvert.DeserializeObject<EbayOrderResponseV2.Root>(json);
        //                        await SaveEbayOrder(json, _User.AuthorizationToken.ToString(), _User.Email, jsopndata.Order.TransactionArray.Transaction.OrderLineItemID, _Ord["OrderID"].InnerText.ToString());
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        public async Task<List<ShippingTag>> GetShipping(AuthorizationConfigClass _User, string TradingAPI_ServerURL
           , string DeveloperId, string ProdClientId, string ProdClientSecret, string TradingAPI_Version)
        {
            List<ShippingTag> ShippingTags = new List<ShippingTag>();
            string PageTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?><GeteBayDetailsRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\">" +
                                                      "<RequesterCredentials>" +
                                                        "<eBayAuthToken>##AuthToken##</eBayAuthToken>" +
                                                      "</RequesterCredentials>" +
                                                        "<ErrorLanguage>en_US</ErrorLanguage>" +
                                                        "<WarningLevel>High</WarningLevel>" +
                                                      "<DetailName>ShippingServiceDetails</DetailName>" +
                                                    "</GeteBayDetailsRequest>";
            PageTemplate = PageTemplate.Replace("##AuthToken##", _User.access_token);
            XmlDocument ResponseXml = new XmlDocument();
            var ProductResp = await HttpPostXMLAsync(PageTemplate, TradingAPI_ServerURL, new Dictionary<string, string>() {
                    {"X-EBAY-API-DEV-NAME",DeveloperId},
                    {"X-EBAY-API-APP-NAME",ProdClientId},
                    {"X-EBAY-API-CERT-NAME",ProdClientSecret},
                    {"X-EBAY-API-SITEID","2"},
                    {"X-EBAY-API-COMPATIBILITY-LEVEL",TradingAPI_Version },
                    {"X-EBAY-API-CALL-NAME","GeteBayDetails"}
                       }, true);
            ResponseXml.LoadXml(ProductResp);
            XmlNode root = ResponseXml["GeteBayDetailsResponse"];
            if (root["Ack"].InnerText == "Failure")
            {
                string errorCode = root["Errors"]["ErrorCode"].InnerText;
                if (errorCode == "841" || errorCode == "16110" || errorCode == "17470" || errorCode == "931")
                {

                }
            }
            else if (root["Ack"] != null && root["Ack"].InnerText.Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                XmlNodeList OptionDetails = ((XmlElement)root).GetElementsByTagName("ShippingServiceDetails");
                foreach (XmlNode __Option in OptionDetails)
                {
                    ShippingTags.Add(new ShippingTag
                    {
                        FriendlyName = __Option["Description"] == null ? __Option["ShippingService"].InnerText : __Option["Description"].InnerText,
                        Tag = __Option["ShippingService"].InnerText,
                        Site = __Option["ShippingCarrier"] == null ? "" : __Option["ShippingCarrier"].InnerText
                    });
                }
            }
            SaveShipping(JsonConvert.SerializeObject(ShippingTags), _User.AuthorizationToken.ToString(), "");
            return ShippingTags;
        }
        //public async Task DispatchEbayOrdersFromStream(AuthorizationConfigClass _User, string orderids, string webApiURL, string TradingAPI_Version)
        //{
        //    var orderlist = Regex.Split(orderids, ",");
        //    foreach (var ebayorderid in orderlist)
        //    {
        //        if (!AwsS3.S3FileIsExists("Authorization", "EbayDispatch/" + _User.AuthorizationToken + "_ebaydispatch_" + ebayorderid + ".json").Result)
        //        {
        //            if (AwsS3.S3FileIsExists("Authorization", "EbayStreamOrder/" + "_streamorder_" + ebayorderid + ".json").Result)
        //            {
        //                var jsondata = AwsS3.GetS3File("Authorization", "EbayStreamOrder/" + "_streamorder_" + ebayorderid + ".json");
        //                var streamdata = JsonConvert.DeserializeObject<StreamOrderRespModel.Root>(jsondata);
        //                if (streamdata != null)
        //                {
        //                    await DispatchOrderInEbay(_User, ebayorderid, null, "Stream", streamdata.response.trackingId, streamdata.response.trackingURL, webApiURL, TradingAPI_Version);
        //                }
        //            }
        //        }
        //    }
        //}

        public async Task DispatchLinnOrdersFromStream(AuthorizationConfigClass _User, string orderids, string linntoken)
        {
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
                            await DispatchOrderInLinnworks(_User, Convert.ToInt32(linnorderid), "Stream", streamdata.response.trackingId, streamdata.response.trackingURL, linntoken,null);
                        }
                    }
                }
            }
        }
        //public async Task DispatchOrderInEbay(AuthorizationConfigClass _User, string OrderRef,
        //string ItemId, string Service, string TrackingNumber, string TrackingUrl, string webApiURL, string TradingAPI_Version)
        //{
        //    if (string.IsNullOrEmpty(ItemId))
        //    {
        //        if (AwsS3.S3FileIsExists("Authorization", "EbayOrder/" + _User.AuthorizationToken.ToString() + "_ebayorder_" + OrderRef + ".json").Result)
        //        {
        //            var json = AwsS3.GetS3File("Authorization", "EbayOrder/" + _User.AuthorizationToken.ToString() + "_ebayorder_" + OrderRef + ".json");
        //            try
        //            {
        //                var jsopndata = JsonConvert.DeserializeObject<EbayOrderResponse.Root>(json);
        //                ItemId = jsopndata.Order.TransactionArray.Transaction.OrderLineItemID;
        //            }
        //            catch
        //            {
        //                var jsopndata = JsonConvert.DeserializeObject<EbayOrderResponseV2.Root>(json);
        //                ItemId = jsopndata.Order.TransactionArray.Transaction.OrderLineItemID;
        //            }
        //        }

        //    }
        //    StringBuilder xmlQuery = new StringBuilder();
        //    xmlQuery.AppendJoin("\n", new string[] {
        //                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
        //                "<CompleteSaleRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\">",
        //                "  <RequesterCredentials>",
        //                "    <eBayAuthToken>"+ _User.access_token +"</eBayAuthToken>",
        //                "  </RequesterCredentials>",
        //                "	<ErrorLanguage>en_US</ErrorLanguage>",
        //                "  <WarningLevel>Low</WarningLevel>"
        //        });

        //    xmlQuery.AppendJoin("\n", new string[] {
        //                        "  <ItemID>"+ItemId.Split("-")[0]+"</ItemID>",
        //                "  <Shipment>",
        //                        "    <Notes>"+TrackingNumber +"</Notes>",
        //                " <ShipmentTrackingDetails> ",
        //                        "   <ShipmentTrackingNumber>"+TrackingNumber +"</ShipmentTrackingNumber>",
        //                        "<ShippingCarrierUsed>"+Service+ "</ShippingCarrierUsed>",
        //                        "<ShipmentTrackingURL>"+TrackingUrl+ "</ShipmentTrackingURL>",
        //                        "   </ShipmentTrackingDetails> ",
        //                        "  </Shipment>",
        //                        "  <Shipped>true</Shipped>",
        //                        "  <TransactionID>"+ItemId.Split("-")[1]+ "</TransactionID>",
        //                        "</CompleteSaleRequest>"
        //                    });
        //    var ProductResp = await HttpPostXMLAsync(xmlQuery.ToString(), webApiURL,
        //                new Dictionary<string, string>() {
        //                        {"X-EBAY-API-SITEID","2" },
        //                        {"X-EBAY-API-COMPATIBILITY-LEVEL",TradingAPI_Version },
        //                        {"X-EBAY-API-CALL-NAME","CompleteSale"}
        //                   }, true);

        //    await SaveEbayDispatch(ProductResp, _User.AuthorizationToken.ToString(), _User.Email, OrderRef);

        //}
        //public async Task CreateEbayOrdersToStream(AuthorizationConfigClass _User, string OrderId)
        //{
        //    List<CourierService> services = Services.GetServices;
        //    var streamAuth = ManageToken.GetToken(_User);

        //    CourierService selectedService = services.Find(s => s.ServiceUniqueId == new Guid("6A476315-04DB-4D25-A25C-E6917A1BCAD9"));

        //    if (AwsS3.S3FileIsExists("Authorization", "EbayOrder/" + _User.AuthorizationToken.ToString() + "_ebayorder_" + OrderId + ".json").Result)
        //    {
        //        var json = AwsS3.GetS3File("Authorization", "EbayOrder/" + _User.AuthorizationToken.ToString() + "_ebayorder_" + OrderId + ".json");
        //        try
        //        {
        //            var jsopndata = JsonConvert.DeserializeObject<EbayOrderResponse.Root>(json);

        //            var streamOrderResponse = StreamOrderApi.CreateOrder(new GenerateLabelRequest()
        //            {
        //                AuthorizationToken = _User.AuthorizationToken,
        //                AddressLine1 = jsopndata.Order.ShippingAddress.Street1,
        //                AddressLine2 = jsopndata.Order.ShippingAddress.Street2,
        //                AddressLine3 = "",
        //                Postalcode = jsopndata.Order.ShippingAddress.PostalCode,
        //                CompanyName = jsopndata.Order.ShippingAddress.Name,
        //                CountryCode = jsopndata.Order.ShippingAddress.Country,
        //                DeliveryNote = "",
        //                ServiceId = new Guid("6A476315-04DB-4D25-A25C-E6917A1BCAD9"),
        //                Email = _User.Email,
        //                Name = jsopndata.Order.ShippingAddress.Name,
        //                OrderReference = jsopndata.Order.OrderID,
        //                OrderId = 0,
        //                Packages = new List<Package>() { new Package() {
        //                                PackageDepth = 0,
        //                                PackageHeight  = 0,PackageWeight = 0 ,PackageWidth = 0,
        //                             Items = new List<Item>()
        //                             {
        //                               new Item(){
        //                                   ProductCode =  jsopndata.Order.TransactionArray.Transaction.Item.SKU,
        //                                   ItemName = jsopndata.Order.TransactionArray.Transaction.Item.Title,
        //                                   Quantity = 1
        //                               }
        //                            }.ToList()  }},
        //                ServiceConfigItems = new List<ServiceConfigItem>(),
        //                OrderExtendedProperties = new List<Models.ExtendedProperty>(),
        //                Phone = jsopndata.Order.ShippingAddress.Phone,
        //                Region = jsopndata.Order.ShippingAddress.StateOrProvince,
        //                Town = jsopndata.Order.ShippingAddress.CityName
        //            }, _User.ClientId, streamAuth.AccessToken, selectedService, true, "DELIVERY");
        //            streamOrderResponse.Item1.AuthorizationToken = _User.AuthorizationToken;
        //            streamOrderResponse.Item1.ItemId = jsopndata.Order.TransactionArray.Transaction.OrderLineItemID;
        //            if (streamOrderResponse.Item1.response == null)
        //            {
        //                await SaveStreamOrder(streamOrderResponse.Item2, _User.AuthorizationToken.ToString(), _User.Email, OrderId, null, "Error", "Error", "Error", OrderId);
        //            }
        //            else
        //            {
        //                await SaveStreamOrder(JsonConvert.SerializeObject(streamOrderResponse.Item1), _User.AuthorizationToken.ToString(), _User.Email, OrderId, null, streamOrderResponse.Item1.response.consignmentNo, streamOrderResponse.Item1.response.trackingId, streamOrderResponse.Item1.response.trackingURL, OrderId);
        //            }


        //        }
        //        catch
        //        {
        //            var jsopndata = JsonConvert.DeserializeObject<EbayOrderResponseV2.Root>(json);
        //            var streamOrderResponse = StreamOrderApi.CreateOrder(new GenerateLabelRequest()
        //            {
        //                AuthorizationToken = _User.AuthorizationToken,
        //                AddressLine1 = jsopndata.Order.ShippingAddress.Street1,
        //                AddressLine2 = jsopndata.Order.ShippingAddress.Street2,
        //                AddressLine3 = "",
        //                CompanyName = jsopndata.Order.ShippingAddress.Name,
        //                CountryCode = jsopndata.Order.ShippingAddress.Country,
        //                DeliveryNote = "",
        //                Postalcode = jsopndata.Order.ShippingAddress.PostalCode,
        //                ServiceId = new Guid("6A476315-04DB-4D25-A25C-E6917A1BCAD9"),
        //                Email = _User.Email,
        //                Name = jsopndata.Order.ShippingAddress.Name,
        //                OrderReference = jsopndata.Order.OrderID,
        //                OrderId = 0,
        //                Packages = new List<Package>() { new Package() {
        //                                PackageDepth = 0,
        //                                PackageHeight  = 0,PackageWeight = 0 ,PackageWidth = 0,
        //                             Items = new List<Item>()
        //                             {
        //                               new Item(){
        //                                   ProductCode =  jsopndata.Order.TransactionArray.Transaction.Item.SKU,
        //                                   ItemName = jsopndata.Order.TransactionArray.Transaction.Item.Title,
        //                                   Quantity = 1
        //                               }
        //                            }.ToList()  }},
        //                ServiceConfigItems = new List<ServiceConfigItem>(),
        //                OrderExtendedProperties = new List<Models.ExtendedProperty>(),
        //                Phone = jsopndata.Order.ShippingAddress.Phone,
        //                Region = jsopndata.Order.ShippingAddress.StateOrProvince,
        //                Town = jsopndata.Order.ShippingAddress.CityName
        //            }, _User.ClientId, streamAuth.AccessToken, selectedService, true, "DELIVERY");
        //            streamOrderResponse.Item1.AuthorizationToken = _User.AuthorizationToken;
        //            streamOrderResponse.Item1.ItemId = jsopndata.Order.TransactionArray.Transaction.OrderLineItemID;
        //            if (streamOrderResponse.Item1.response == null)
        //            {
        //                await SaveStreamOrder(streamOrderResponse.Item2, _User.AuthorizationToken.ToString(), _User.Email, OrderId, null, "Error", "Error", "Error", OrderId);
        //            }
        //            else
        //            {
        //                await SaveStreamOrder(JsonConvert.SerializeObject(streamOrderResponse.Item1), _User.AuthorizationToken.ToString(), _User.Email, OrderId, null, streamOrderResponse.Item1.response.consignmentNo, streamOrderResponse.Item1.response.trackingId, streamOrderResponse.Item1.response.trackingURL, OrderId);
        //            }

        //        }
        //    }
        //}
        #endregion
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
            var streamAuth = ManageToken.GetToken(auth);

            CourierService selectedService = services.Find(s => s.ServiceUniqueId == CourierSettings.SelectedServiceId);

            if (AwsS3.S3FileIsExists("Authorization", "LinnOrder/" + auth.AuthorizationToken.ToString() + "_linnorder_" + OrderId + ".json").Result)
            {
                var json = AwsS3.GetS3File("Authorization", "LinnOrder/" + auth.AuthorizationToken.ToString() + "_linnorder_" + OrderId + ".json");
                int orderId = Convert.ToInt32(StreamOrderId);
                try
                {
                    var jsopndata = JsonConvert.DeserializeObject<OpenOrder>(json);
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
            List<CourierService> services = Services.GetServices;
            var streamAuth = ManageToken.GetToken(auth);

            CourierService selectedService = services.Find(s => s.ServiceUniqueId == CourierSettings.SelectedServiceId);

            if (AwsS3.S3FileIsExists("Authorization", "LinnOrder/" + auth.AuthorizationToken.ToString() + "_linnorder_" + OrderId + ".json").Result)
            {
                var json = AwsS3.GetS3File("Authorization", "LinnOrder/" + auth.AuthorizationToken.ToString() + "_linnorder_" + OrderId + ".json");

                if (json != "null")
                {
                    try
                    {
                        var jsopndata = JsonConvert.DeserializeObject<OpenOrder>(json);
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

                    }
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
            var list = obj.Api.Orders.SetOrderShippingInfo(orderdata.OrderId, new UpdateOrderShippingInfoRequest() { 
             ItemWeight = orderdata.ShippingInfo.ItemWeight,
             ManualAdjust = orderdata.ShippingInfo.ManualAdjust,
             PostageCost = orderdata.ShippingInfo.PostageCost,  
             PostalServiceId = orderdata.ShippingInfo.PostalServiceId,
             TotalWeight = orderdata.ShippingInfo.TotalWeight,
             TrackingNumber = TrackingNumber
            });
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
            var streamAuth = ManageToken.GetToken(_User);
            return StreamOrderApi.GetOrder(streamAuth.AccessToken, OrderId, _User.ClientId);
        }
        #endregion
        #region Save s3 data
        public void RegisterSave(string s, string AuthorizationToken, string email = "",string token = "")
        {
            RegisterSaveFromJson(s);
            var Stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(Stream);
            sw.Write(s);;
            sw.Flush();
            Stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", Stream, "Users/_register_" + email + ".json");
            var stream1 = new MemoryStream();
            StreamWriter sw1 = new StreamWriter(stream1);
            sw1.Write(s);
            sw1.Flush();
            stream1.Position = 0;
            //AwsS3.UploadFileToS3("Authorization", stream1, "Files/" + token.ToString() + ".json");
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
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            return stream;
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
        public async Task SaveLinnOrder(string s, string AuthorizationToken, string email, string linnorderid = "")
        {
            InsertOrderFromJson(s);
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "LinnOrder/" + AuthorizationToken.ToString() + "_linnorder_" + linnorderid + ".json");
            var alldata = _reportsController.GetReportData(new ReportModelReq() { email = email }).Result;
            if (alldata.Count(f => f.LinnNumOrderId == linnorderid) == 0)
            {
                alldata.Add(new ReportModel()
                {
                    _id = Guid.NewGuid().ToString(),
                    updatedDate = DateTime.Now,
                    email = email,
                    DownloadLinnOrderInSystem = DateTime.Now,
                    createdDate = DateTime.Now,
                    AuthorizationToken = AuthorizationToken,
                    LinnNumOrderId = linnorderid,
                    LinnOrderDetailsJson = "LinnOrder/" + AuthorizationToken.ToString() + "_linnorder_" + linnorderid + ".json"
                });
                await SaveReportData(JsonConvert.SerializeObject(alldata), email);
            }
            else
            {
                alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid).LinnOrderDetailsJson = "LinnOrder/" + AuthorizationToken.ToString() + "_linnorder_" + linnorderid + ".json";
                alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid).DownloadLinnOrderInSystem = DateTime.Now;
                alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid).updatedDate = DateTime.Now;
               await SaveReportData(JsonConvert.SerializeObject(alldata), email);
            }
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
                    NumOrderId = root.NumOrderId ?? 0,

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
                        Quantity = i.Quantity ?? 0,
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
                            Quantity = c.Quantity ?? 0,
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
        
        public async Task SaveEbayOrder(string s, string AuthorizationToken, string email, string orderlineitemid, string ebayorderid = "")
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "EbayOrder/" + AuthorizationToken.ToString() + "_ebayorder_" + ebayorderid + ".json");
            var alldata = _reportsController.GetReportData(new ReportModelReq() { email = email }).Result;
            if (alldata.Count(d => d.EbayChannelOrderRef == ebayorderid) == 0)
            {
                alldata.Add(new ReportModel()
                {
                    _id = Guid.NewGuid().ToString(),
                    AuthorizationToken = AuthorizationToken,
                    createdDate = DateTime.Now,
                    updatedDate = DateTime.Now,
                    OrderLineItemId = orderlineitemid,
                    DownloadEbayOrderInSystem = DateTime.Now,
                    EbayOrderDetaailJson = "EbayOrder/" + AuthorizationToken.ToString() + "_ebayorder_" + ebayorderid + ".json",
                    EbayChannelOrderRef = ebayorderid,
                    email = email
                });
                await SaveReportData(JsonConvert.SerializeObject(alldata), email);
            }
            else
            {
                alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).updatedDate = DateTime.Now;
                alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).OrderLineItemId = orderlineitemid;
                alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).EbayOrderDetaailJson = "EbayOrder/" + AuthorizationToken.ToString() + "_ebayorder_" + ebayorderid + ".json";
                alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).IsEbayOrderDispatchFromStream = true;
                alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).DownloadEbayOrderInSystem = DateTime.Now;
                await SaveReportData(JsonConvert.SerializeObject(alldata), email);
            }
        }
        public async Task SaveEbayDispatch(string s, string AuthorizationToken, string email, string reference = "")
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "EbayDispatch/" + AuthorizationToken.ToString() + "_ebaydispatch_" + reference + ".json");
            var alldata = _reportsController.GetReportData(new ReportModelReq() { email = email }).Result;
            if (alldata.Count(d => d.EbayChannelOrderRef == reference) == 0)
            {
                alldata.Add(new ReportModel()
                {
                    _id = Guid.NewGuid().ToString(),
                    AuthorizationToken = AuthorizationToken,
                    createdDate = DateTime.Now,
                    updatedDate = DateTime.Now,
                    DispatchEbayOrderFromStream = DateTime.Now,
                    IsEbayOrderDispatchFromStream = true,
                    DispatchOrderInEbayJson = "EbayDispatch/" + AuthorizationToken.ToString() + "_ebaydispatch_" + reference + ".json",
                    EbayChannelOrderRef = reference,
                    email = email
                });
                await SaveReportData(JsonConvert.SerializeObject(alldata), email);
            }
            else
            {
                alldata.FirstOrDefault(f => f.EbayChannelOrderRef == reference).updatedDate = DateTime.Now;
                alldata.FirstOrDefault(f => f.EbayChannelOrderRef == reference).DispatchOrderInEbayJson = "EbayDispatch/" + AuthorizationToken.ToString() + "_ebaydispatch_" + reference + ".json";
                alldata.FirstOrDefault(f => f.EbayChannelOrderRef == reference).IsEbayOrderDispatchFromStream = true;
                alldata.FirstOrDefault(f => f.EbayChannelOrderRef == reference).DispatchEbayOrderFromStream = DateTime.Now;
                await SaveReportData(JsonConvert.SerializeObject(alldata), email);
            }
        }
        public async Task SaveStreamOrder(string s, string AuthorizationToken, string email, string ebayorderid, string linnworksorderid, string consignmentid, string trackingnumber, string trackingurl, string order = "")
        {
            Stream stream1 = GenerateStreamFromString(s);
            AwsS3.UploadFileToS3("Authorization", stream1, "UserStreamOrder/" + AuthorizationToken + "_streamorder_" + order + ".json");
            Stream stream2 = GenerateStreamFromString(s);
            AwsS3.UploadFileToS3("Authorization", stream2, "StreamOrder/" + "_streamorder_" + order + ".json");
            if (ebayorderid != null)
            {
                Stream stream3 = GenerateStreamFromString(s);
                AwsS3.UploadFileToS3("Authorization", stream3, "EbayStreamOrder/" + "_streamorder_" + ebayorderid + ".json");
            }
            if (linnworksorderid != null)
            {
                Stream stream4 = GenerateStreamFromString(s);
                AwsS3.UploadFileToS3("Authorization", stream4, "LinnStreamOrder/" + "_streamorder_" + linnworksorderid + ".json");
            }

            var alldata = _reportsController.GetReportData(new ReportModelReq() { email = email }).Result;
            if (ebayorderid != null)
            {
                if (alldata.Count(f => f.EbayChannelOrderRef == ebayorderid) == 0)
                {
                    alldata.Add(new ReportModel()
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
                        StreamOrderCreateJson = "UserStreamOrder/" + AuthorizationToken.ToString() + "_streamorder_" + order + ".json"
                    });
                    await SaveReportData(JsonConvert.SerializeObject(alldata), email);
                }
                else
                {
                    alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).CreateEbayOrderInStream = DateTime.Now;
                    alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).IsEbayOrderCreatedInStream = true;
                    alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).StreamOrderCreateJson = "UserStreamOrder/" + AuthorizationToken.ToString() + "_streamorder_" + order + ".json";
                    alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).StreamOrderId = order;
                    alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).StreamConsignmentId = consignmentid;
                    alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).StreamTrackingNumber = trackingnumber;
                    alldata.FirstOrDefault(f => f.EbayChannelOrderRef == ebayorderid).StreamTrackingURL = trackingurl;
                    await SaveReportData(JsonConvert.SerializeObject(alldata), email);
                }
            }
            else if (linnworksorderid != null)
            {
                if (alldata.Count(f => f.LinnNumOrderId == linnworksorderid) == 0)
                {
                    alldata.Add(new ReportModel()
                    {
                        _id = Guid.NewGuid().ToString(),
                        updatedDate = DateTime.Now,
                        createdDate = DateTime.Now,
                        AuthorizationToken = AuthorizationToken,
                        StreamOrderId = order,
                        StreamConsignmentId = consignmentid,
                        StreamTrackingNumber = trackingnumber,
                        StreamTrackingURL = trackingurl,
                        LinnNumOrderId = linnworksorderid,
                        IsLinnOrderCreatedInStream = true,
                        email = email,
                        CreateLinnOrderInStream = DateTime.Now,
                        StreamOrderCreateJson = "UserStreamOrder/" + AuthorizationToken + "_streamorder_" + order + ".json"
                    });
                    await SaveReportData(JsonConvert.SerializeObject(alldata), email);
                }
                else
                {
                    alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).CreateLinnOrderInStream = DateTime.Now;
                    alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).IsLinnOrderCreatedInStream = true;
                    alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).StreamOrderCreateJson = "UserStreamOrder/" + AuthorizationToken + "_streamorder_" + order + ".json";
                    alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).StreamOrderId = order;
                    alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).StreamConsignmentId = consignmentid;
                    alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).StreamTrackingNumber = trackingnumber;
                    alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).StreamTrackingURL = trackingurl;
                    alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).updatedDate = DateTime.Now;
                    await SaveReportData(JsonConvert.SerializeObject(alldata), email);
                }
            }
        }
        public async Task SaveLinnChangeOrder(string s, string AuthorizationToken, string order = "")
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "LinnChange/" + AuthorizationToken.ToString() + "_linnchangeorder_" + order + ".json");

        }
        public async Task SaveLinnDispatch(string s, string AuthorizationToken, string email, int linnorderid)
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "LinnDispatch/" + AuthorizationToken.ToString() + "_linndispatch_" + linnorderid + ".json");
            var alldata = _reportsController.GetReportData(new ReportModelReq() { email = email }).Result;
            if (alldata.Count(d => d.LinnNumOrderId == linnorderid.ToString()) == 0)
            {
                alldata.Add(new ReportModel()
                {
                    _id = Guid.NewGuid().ToString(),
                    AuthorizationToken = AuthorizationToken,
                    createdDate = DateTime.Now,
                    updatedDate = DateTime.Now,
                    DispatchLinnOrderFromStream = DateTime.Now,
                    IsLinnOrderDispatchFromStream = true,
                    DispatchOrderInLinnJson = "LinnDispatch/" + AuthorizationToken.ToString() + "_linndispatch_" + linnorderid + ".json",
                    EbayChannelOrderRef = linnorderid.ToString(),
                    email = email
                });
                await SaveReportData(JsonConvert.SerializeObject(alldata), email);
            }
            else
            {
                alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid.ToString()).updatedDate = DateTime.Now;
                alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid.ToString()).DispatchOrderInLinnJson = "LinnDispatch/" + AuthorizationToken.ToString() + "_linndispatch_" + linnorderid + ".json";
                alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid.ToString()).IsLinnOrderDispatchFromStream = true;
                alldata.FirstOrDefault(f => f.LinnNumOrderId == linnorderid.ToString()).DispatchLinnOrderFromStream = DateTime.Now;
                await SaveReportData(JsonConvert.SerializeObject(alldata), email);
            }
        }

        public async Task SaveShipping(string s, string AuthorizationToken, string reference = "")
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "EbayShipping/" + AuthorizationToken.ToString() + "_ebayshipping_" + reference + ".json");

        }
        public async Task SaveWebhook(string s, string AuthorizationToken, string reference = "")
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "Webhook/" + AuthorizationToken.ToString() + "_webhook_" + reference + ".json");



            //var subscription = new Subscription()
            //{
            //    party_id = output.webhook.subscription.party_id,
            //    @event = output.webhook.subscription.@event,
            //    event_type = output.webhook.subscription.event_type,
            //    url_path = output.webhook.subscription.url_path,
            //    http_method = output.webhook.subscription.http_method
            //};
            //var @event = new Event
            //{
            //    event_code = output.webhook.@event.event_code,
            //    event_code_desc = output.webhook.@event.event_code_desc,
            //    event_desc = output.webhook.@event.event_desc,
            //    event_date = output.webhook.@event.event_date,
            //    event_time = output.webhook.@event.event_time,
            //    event_text = output.webhook.@event.event_text,
            //    event_link = output.webhook.@event.event_link
            //};
            //var run = new Run
            //{
            //    loadId = output.webhook.run.loadId,
            //    status = output.webhook.run.status,
            //    description = output.webhook.run.description
            //};
            //_subscription.Add(subscription);
            //_event.Add(@event);
            //_run.Add(run);
            //foreach (var order in output.webhook.orders)
            //{
            //    var webhookOrder = new WebhookOrder
            //    {
            //        sequence = order.sequence,
            //        order = order.order
            //    };
            //    _webhookOrder.Add(webhookOrder);
            //}

            //_unitOfWork.Context.SaveChanges();
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
        #endregion
        #region Messian function
       
        #endregion


    }

}
