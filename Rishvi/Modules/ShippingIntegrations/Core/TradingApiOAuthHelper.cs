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
namespace Rishvi.Modules.ShippingIntegrations.Core
{
    public class TradingApiOAuthHelper
    {
        private readonly ReportsController _reportsController;
        private readonly SetupController _setupController;
       // private readonly Guid _selectedServiceGuid = new Guid("6A476315-04DB-4D25-A25C-E6917A1BCAD9");
        public TradingApiOAuthHelper(ReportsController reportsController, SetupController setupController)
        {
            _reportsController = reportsController;
            _setupController = setupController;
           
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

        public async Task UpdateLinnworksOrdersToStream(AuthorizationConfigClass auth, string OrderId, string StreamOrderId)
        {
            List<CourierService> services = Services.GetServices;
            var streamAuth = ManageToken.GetToken(auth);
            //CourierService selectedService = services.Find(s => s.ServiceUniqueId == new Guid("6A476315-04DB-4D25-A25C-E6917A1BCAD9"));
            CourierService selectedService = services.Find(s => s.ServiceUniqueId == CourierSettings.SelectedServiceId);
            if (AwsS3.S3FileIsExists("Authorization", "LinnOrder/" + auth.AuthorizationToken.ToString() + "_linnorder_" + OrderId + ".json").Result)
            {
                var json = AwsS3.GetS3File("Authorization", "LinnOrder/" + auth.AuthorizationToken.ToString() + "_linnorder_" + OrderId + ".json");
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
                        Town = jsopndata.CustomerInfo.Address.Town
                    }, auth.ClientId, streamAuth.AccessToken, selectedService, true, jsopndata.ShippingInfo.PostalServiceName.ToLower().Contains("pickup") ? "COLLECTION" : "DELIVERY");
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
                            Town = jsopndata.CustomerInfo.Address.Town
                        }, auth.ClientId, streamAuth.AccessToken, selectedService, true, jsopndata.ShippingInfo.PostalServiceName.ToLower().Contains("pickup") ? "COLLECTION" : "DELIVERY");
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
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "Users/_register_" + email + ".json");
            var stream1 = new MemoryStream();
            StreamWriter sw1 = new StreamWriter(stream1);
            sw1.Write(s);
            sw1.Flush();
            stream1.Position = 0;
            //AwsS3.UploadFileToS3("Authorization", stream1, "Files/" + token.ToString() + ".json");
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
