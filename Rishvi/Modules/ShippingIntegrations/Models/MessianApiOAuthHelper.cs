using CsvHelper;
using FluentFTP;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.ShippingIntegrations.Api;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;
using static Rishvi.Modules.ShippingIntegrations.Api.MessinaController;

namespace Rishvi.Modules.ShippingIntegrations.Models
{


    public class MessianApiOAuthHelper
    {
        int SiteID = 2;
        private readonly MessinaSettings _settings;
        public MessianApiOAuthHelper(MessinaSettings settings)
        {
            _settings = settings;
        }

        public string GetSessionID(string AuthToken)
        {
            XmlDocument xmlDoc;
            string error = "";
            string strReq = @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <GetSessionIDRequest xmlns=""urn:ebay:apis:eBLBaseComponents"">
                          <RuName>" + _settings.ProdRedirectURL + @"</RuName>
                        </GetSessionIDRequest>";

            xmlDoc = MakeAPICall(strReq, "GetSessionID", error, AuthToken);
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

        public async Task<EbayAuthentication> GenerateToken(AuthorizationConfigClass _User, string SessionID)
        {
            string error = "";
            string strReq = @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <FetchTokenRequest xmlns=""urn:ebay:apis:eBLBaseComponents"">
                          <SessionID>" + SessionID + @"</SessionID>
                        </FetchTokenRequest>";

            XmlDocument xmlDoc = MakeAPICall(strReq, "FetchToken", error, _User.AuthorizationToken.ToString());

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
                if (keyVal.Key == ("X-EBAY-API-CALL-NAME"))
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
            using (HttpWebResponse webResponse = (HttpWebResponse)(await webRequest.GetResponseAsync()))
            {
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    string EbayResponse = await sr.ReadToEndAsync();

                    return EbayResponse;
                }
            }
        }


        public async Task PriceUpdate(AuthorizationConfigClass _User, List<string> ItemId, List<string> price)
        {
            StringBuilder xmlQuery = new StringBuilder();
            xmlQuery.AppendJoin("\n", new string[] {
                        "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                                "<ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\">",
                                "<RequesterCredentials>",
                                "<eBayAuthToken>"+_User.access_token+"</eBayAuthToken>",
                                "</RequesterCredentials>",
                                "<ErrorLanguage>en_US</ErrorLanguage>",
                                "<WarningLevel>Low</WarningLevel>"
                });
            for (var i = 0; i < ItemId.Count; i++)
            {
                xmlQuery.AppendJoin("\n", new string[] {
                                "<InventoryStatus>",
                                "<ItemID>"+ ItemId[i] +"</ItemID>",
                                "<StartPrice currencyID=\""+ "GBP"+"\">"+price[i]+"</StartPrice>",
                                "</InventoryStatus>"
                            });

            }
            xmlQuery.AppendJoin("\n", new string[] {
                                "</ReviseInventoryStatusRequest>"
                                });
            var ProductResp = await HttpPostXMLAsync(xmlQuery.ToString(), _settings.WebApiURL,
                        new Dictionary<string, string>() {
                                {"X-EBAY-API-SITEID","2" },
                                {"X-EBAY-API-COMPATIBILITY-LEVEL",_settings.TradingAPI_Version },
                                {"X-EBAY-API-CALL-NAME","ReviseInventoryStatus"}
                           }, true);

            SavePrice(ProductResp, _User.AuthorizationToken.ToString(), ItemId[0]);

        }


        public static void SavePrice(string s, string AuthorizationToken, string reference = "")
        {
            var stream = new MemoryStream();
            System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "Files/" + AuthorizationToken.ToString() + "_price_" + reference + ".json");

        }

        public void SyncLogs(string s, string AuthorizationToken, string reference = "")
        {
            var stream = new MemoryStream();
            System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "Sync/" + AuthorizationToken.ToString() + "_synclogs_" + reference + ".json");

        }

        public void SaveLogs(string s, string AuthorizationToken, string reference = "")
        {
            var stream = new MemoryStream();
            System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            AwsS3.UploadFileToS3("Authorization", stream, "Files/" + AuthorizationToken.ToString() + "_installlogs_" + reference + ".json");

        }

        private XmlDocument MakeAPICall(string requestBody, string callname, string error, string AuthToken)
        {
            XmlDocument xmlDoc = new XmlDocument();

            string APIServerURL = _settings.TradingAPI_ServerURL;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(APIServerURL);
            request.Headers.Add("X-EBAY-API-DEV-NAME", _settings.DeveloperId);
            request.Headers.Add("X-EBAY-API-APP-NAME", _settings.ProdClientId);
            request.Headers.Add("X-EBAY-API-CERT-NAME", _settings.ProdClientSecret);
            request.Headers.Add("X-EBAY-API-COMPATIBILITY-LEVEL", _settings.TradingAPI_Version);
            request.Headers.Add("X-EBAY-API-SITEID", SiteID.ToString());
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
                using (HttpWebResponse webResponse = (HttpWebResponse)(request.GetResponse()))
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


        public void DownloadFileFromFtpAsync(FtpClient ftpClient, string remoteFilePath, string localFilePath, AuthorizationConfigClass user)
        {
            try
            {
                ftpClient.Connect();
                ftpClient.DownloadFile(localFilePath, remoteFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
            }
            finally
            {
                ftpClient.Disconnect();
            }
        }

        public void ReadCsvAndBatchProcessAsync(string csvFilePath, int batchSize, AuthorizationConfigClass user)
        {
            var records = new List<CsvRecord>();
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var recordsList = csv.GetRecords<CsvRecord>().ToList();
                records.AddRange(recordsList);
            }

            Console.WriteLine("CSV Parsing Complete. Processing in batches...");

            ProcessInBatchesAsync(records, batchSize, user);
        }

        public void ProcessInBatchesAsync(List<CsvRecord> records, int batchSize, AuthorizationConfigClass user)
        {
            var batch = new List<CsvRecord>();

            for (int i = 0; i < records.Count; i++)
            {
                batch.Add(records[i]);

                if (batch.Count == batchSize || i == records.Count - 1)
                {
                    var t = Task.Run(async delegate
                    {
                        await Task.Delay(5000);
                    });
                    t.Wait();

                    // Pass the required '_settings' parameter to the constructor
                    new MessianApiOAuthHelper(_settings).ProcessBatchAsync(batch, user);
                    batch.Clear();
                }
            }
        }
        public async Task ProcessBatchAsync(List<CsvRecord> batch, AuthorizationConfigClass user)
        {

            await new MessianApiOAuthHelper(_settings).PriceUpdate(user, batch.Select(d => d.ItemId).ToList(), batch.Select(d => d.StartPrice).ToList());

        }

        public class CsvRecord
        {
            public string Action { get; set; }


            public string ItemId { get; set; }
            public string SiteId { get; set; }
            public string Currency { get; set; }
            public string StartPrice { get; set; }
            public string BuyItNow { get; set; }
            public string Quantity { get; set; }

            public string Relationship { get; set; }
            public string RelationshipDetails { get; set; }
            public string CustomLabel { get; set; }
            public string BestOfferEnabled { get; set; }
            public string MinimumBestOfferPrice { get; set; }
            public string BestOfferAutoAcceptPrice { get; set; }
        }

    }

}
