using Newtonsoft.Json;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Helpers;
using SkiaSharp;
using System.IO;
using ThirdParty.Json.LitJson;

namespace Rishvi.Modules.ShippingIntegrations.Models.Classes
{
    public class AuthorizationConfig : IAuthorizationToken
    {
        public string Email { get; set; }
        public string LinnworksUniqueIdentifier { get; set; }
        public DateTime IntegratedDateTime = DateTime.UtcNow;
        public Guid AuthorizationToken { get; set; }
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
        public string LabelReference = "";
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public AuthorizationConfigClass Load(string AuthorizationToken)
        {
            if (string.IsNullOrWhiteSpace(AuthorizationToken))
                throw new ArgumentNullException("authorizationToken");

            if (AwsS3.S3FileIsExists("Authorization", "Files/" + AuthorizationToken + ".json").Result)
            {
                
                string json = AwsS3.GetS3File("Authorization", "Files/" + AuthorizationToken + ".json");
                AuthorizationConfigClass output = JsonConvert.DeserializeObject<AuthorizationConfigClass>(json);
                if (output.PartyFileCreated == false)
                {
                    output.PartyFileCreated = true;
                    var updatedJson = JsonConvert.SerializeObject(output);

                    AwsS3.UploadFileToS3("Authorization", output.GenerateStreamFromString(updatedJson), $"StreamParty/{output.ClientId}.json");
                    AwsS3.UploadFileToS3("Authorization", output.GenerateStreamFromString(updatedJson), $"Files/{output.AuthorizationToken}.json");
                }
                return output;
            }
            else
            {
                return null;
            }

        }

        public void Delete(string AuthorizationToken)
        {
            if (AwsS3.S3FileIsExists("Authorization", "Files/" + AuthorizationToken + ".json").Result)
            {
                AwsS3.DeleteImageToAws("Authorization", "Files/" + AuthorizationToken + ".json");
            }
        }

        public AuthorizationConfigClass CreateNew(string email, string SessionID,
            string LinnworksUniqueIdentifier, string accountName, string clientid = null, string secret = null,
            string state = null)
        {
            AuthorizationConfigClass output = new AuthorizationConfigClass();
            if (state == null)
            {
                output.AuthorizationToken = Guid.NewGuid().ToString();
            }
            else
            {
                output.AuthorizationToken = state;
            }
            output.Email = email;
            output.ClientId = clientid;
            output.ClientSecret = secret;
            output.SessionID = SessionID;
            output.LinnworksUniqueIdentifier = LinnworksUniqueIdentifier;
            output.AccountName = accountName;
            output.Save();
            return output;
        }
    }
}
