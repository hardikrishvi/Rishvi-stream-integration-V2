using Newtonsoft.Json;
using Rishvi.Models;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using SkiaSharp;
using System.IO;
using ThirdParty.Json.LitJson;

namespace Rishvi.Modules.ShippingIntegrations.Models.Classes
{
    public class AuthorizationConfig : IAuthorizationToken
    {
        private readonly IRepository<Authorization> _authorization;
        private readonly IUnitOfWork _unitOfWork;
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
        public AuthorizationConfig(IRepository<Authorization> authorization, IUnitOfWork unitOfWork)
        {
            _authorization = authorization;
            _unitOfWork = unitOfWork;
        }
        public AuthorizationConfigClass Load(string AuthorizationToken)
        {
            if (string.IsNullOrWhiteSpace(AuthorizationToken))
                throw new ArgumentNullException("authorizationToken");

            if (AwsS3.S3FileIsExists("Authorization", "Files/" + AuthorizationToken + ".json").Result)
            {
                
                string json = AwsS3.GetS3File("Authorization", "Files/" + AuthorizationToken + ".json");
                AuthorizationConfigClass output = JsonConvert.DeserializeObject<AuthorizationConfigClass>(json);

                //Get authorization data from database
                var get_auth = _authorization.GetByToken(Guid.Parse(AuthorizationToken));

                if (output.PartyFileCreated == false)
                {
                    output.PartyFileCreated = true;
                    var updatedJson = JsonConvert.SerializeObject(output);

                    AwsS3.UploadFileToS3("Authorization", output.GenerateStreamFromString(updatedJson), $"StreamParty/{output.ClientId}.json");
                    AwsS3.UploadFileToS3("Authorization", output.GenerateStreamFromString(updatedJson), $"Files/{output.AuthorizationToken}.json");

                    //Add authorization to database
                    var auth = new Authorization
                    {
                        IntegratedDateTime = output.IntegratedDateTime,
                        AuthorizationToken = Guid.Parse(output.AuthorizationToken),
                        Email = output.Email,
                        ClientId = output.ClientId,
                        ClientSecret = output.ClientSecret,
                        SessionID = output.SessionID,
                        LinnworksUniqueIdentifier = Guid.Parse(output.LinnworksUniqueIdentifier),
                        AccountName = output.AccountName,
                        IsConfigActive = output.IsConfigActive,
                        ConfigStatus = output.ConfigStatus,
                        AddressLine1 = output.AddressLine1,
                        CompanyName = output.CompanyName,
                        AddressLine2 = output.AddressLine2,
                        AddressLine3 = output.AddressLine3,
                        City = output.City,
                        ContactName = output.ContactName,
                        ContactPhoneNo = output.ContactPhoneNo,
                        CountryCode = output.CountryCode,
                        County = output.County,
                        PostCode = output.PostCode,
                        LabelReference = output.LabelReference,
                        access_token = Guid.Parse(output.access_token),
                        ExpirationTime = DateTime.Parse(output.ExpirationTime),
                        expires_in = output.expires_in,
                        refresh_token = Guid.Parse(output.refresh_token),
                        refresh_token_expires_in = output.refresh_token_expires_in,
                        token_type = output.token_type,
                        FtpHost = output.FtpHost,
                        FtpUsername = output.FtpUsername,
                        FtpPassword = output.FtpPassword,
                        FtpPort = output.FtpPort,
                        LinnworksToken = Guid.Parse(output.LinnworksToken),
                        LinnworksServer = output.LinnworksServer,
                        LinnRefreshToken = Guid.Parse(output.LinnRefreshToken),
                        fulfiilmentLocation = output.fulfiilmentLocation,
                        PartyFileCreated = output.PartyFileCreated,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _authorization.Add(auth);
                    _unitOfWork.Commit();
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
