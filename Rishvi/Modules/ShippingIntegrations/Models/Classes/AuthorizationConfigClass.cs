using Newtonsoft.Json;
using Rishvi.Modules.Core.Aws;

namespace Rishvi.Modules.ShippingIntegrations.Models.Classes
{
    public class AuthorizationConfigClass
    {
        public string Email { get; set; }
        public string LinnworksUniqueIdentifier { get; set; }
        public DateTime IntegratedDateTime = DateTime.UtcNow;
        public string AuthorizationToken { get; set; }
        public string AccountName { get; set; }
        public Boolean IsConfigActive = false;
        public string ConfigStatus = "";
        public string AddressLine1 = "";
        public string CompanyName = "";
        public string AddressLine2 = "";
        public string AddressLine3 = "";
        public string City = "";
        public string ContactName = "";
        public string ContactPhoneNo = "";
        public string CountryCode = "GB";
        public string County = "";
        public string PostCode = "";
        public string SessionID = "";
        public string LabelReference = "";
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string access_token { get; set; }
        public string ExpirationTime { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public int refresh_token_expires_in { get; set; }
        public string token_type { get; set; }

        public string FtpHost { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }

        public int FtpPort { get; set; }

        public string LinnworksToken { get; set; }
        public string LinnworksServer { get; set; }
        public string LinnRefreshToken { get; set; }

        public string fulfiilmentLocation { get; set; }
        public bool AutoOrderSync { get; set; } = false;
        public bool AutoOrderDespatchSync { get; set; } = false;
        public bool PartyFileCreated { get; set; }
        public bool UseDefaultLocation { get; set; } = false;
        public string? DefaultLocation { get; set; } = "";
        public int LinnHour { get; set; } = 24000;
        public int LinnPage { get; set; } = 20;
        public bool SendChangeToStream { get; set; } = true;
        public bool HandsOnDate { get; set; } = false;
        public void Save()
        {
            var jsonData = JsonConvert.SerializeObject(this);
            Stream stream = GenerateStreamFromString(jsonData);
            AwsS3.UploadFileToS3("Authorization", stream, "Files/" + this.AuthorizationToken.ToString() + ".json");
        }

        public Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(s);
            sw.Flush();
            stream.Position = 0;
            return stream;
        }
    }


}
