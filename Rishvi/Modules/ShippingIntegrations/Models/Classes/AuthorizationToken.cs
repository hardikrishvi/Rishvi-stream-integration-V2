using Azure.Core;
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
        public Rishvi.Models.Authorization Load(string AuthorizationToken)
        {
            if (string.IsNullOrWhiteSpace(AuthorizationToken))
                throw new ArgumentNullException("authorizationToken");

            var get_auth = _authorization.Get()
                                .FirstOrDefault(x => x.AuthorizationToken == AuthorizationToken);

            if (get_auth == null)
            {
                return null;
            }


            // Map database entity to your DTO
            var output = new Rishvi.Models.Authorization
            {
                IntegratedDateTime = get_auth.IntegratedDateTime ?? DateTime.UtcNow,
                AuthorizationToken = get_auth.AuthorizationToken ?? "",
                Email = get_auth.Email ?? "",
                ClientId = get_auth.ClientId ?? "",
                ClientSecret = get_auth.ClientSecret ?? "",
                SessionID = get_auth.SessionID ?? "",
                LinnworksUniqueIdentifier = get_auth.LinnworksUniqueIdentifier ?? "",
                AccountName = get_auth.AccountName ?? "",
                IsConfigActive = get_auth.IsConfigActive ?? false,
                ConfigStatus = get_auth.ConfigStatus ?? "",
                AddressLine1 = get_auth.AddressLine1 ?? "",
                CompanyName = get_auth.CompanyName ?? "",
                AddressLine2 = get_auth.AddressLine2 ?? "",
                AddressLine3 = get_auth.AddressLine3 ?? "",
                City = get_auth.City ?? "",
                ContactName = get_auth.ContactName ?? "",
                ContactPhoneNo = get_auth.ContactPhoneNo ?? "",
                CountryCode = get_auth.CountryCode ?? "GB",
                County = get_auth.County ?? "",
                PostCode = get_auth.PostCode ?? "",
                LabelReference = get_auth.LabelReference ?? "",
                access_token = get_auth.access_token ?? "",
                ExpirationTime = get_auth.ExpirationTime, 
                expires_in = get_auth.expires_in ?? 0,
                refresh_token = get_auth.refresh_token ?? "",
                refresh_token_expires_in = get_auth.refresh_token_expires_in ?? 0,
                token_type = get_auth.token_type ?? "",
                FtpHost = get_auth.FtpHost ?? "",
                FtpUsername = get_auth.FtpUsername ?? "",
                FtpPassword = get_auth.FtpPassword ?? "",
                FtpPort = get_auth.FtpPort ?? 0,
                LinnworksToken = get_auth.LinnworksToken ?? "",
                LinnworksServer = get_auth.LinnworksServer ?? "",
                LinnRefreshToken = get_auth.LinnRefreshToken ?? "",
                fulfiilmentLocation = get_auth.fulfiilmentLocation ?? "",
                PartyFileCreated = get_auth.PartyFileCreated ?? false,
                AutoOrderSync = get_auth.AutoOrderSync,
                AutoOrderDespatchSync = get_auth.AutoOrderDespatchSync,
                UseDefaultLocation = get_auth.UseDefaultLocation,
                DefaultLocation = get_auth.DefaultLocation ?? "",
                LinnDays = get_auth.LinnDays,
                LinnPage = get_auth.LinnPage,
                SendChangeToStream = get_auth.SendChangeToStream,
                HandsOnDate = get_auth.HandsOnDate,
                ShippingApiConfigId = get_auth.ShippingApiConfigId,
                IsLiveAccount = get_auth.IsLiveAccount,
            };
            return output;
        }

        public void Delete(string AuthorizationToken)
        {
            //if (AwsS3.S3FileIsExists("Authorization", "Files/" + AuthorizationToken + ".json").Result)
            //{
            //    AwsS3.DeleteImageToAws("Authorization", "Files/" + AuthorizationToken + ".json");
            //}

            if (string.IsNullOrWhiteSpace(AuthorizationToken))
                throw new ArgumentNullException(nameof(AuthorizationToken));

            // Fetch from database
            var existingAuth = _authorization.Get()
                                .FirstOrDefault(x => x.AuthorizationToken == AuthorizationToken);

            if (existingAuth != null)
            {
                _authorization.Delete(existingAuth);
                _unitOfWork.Commit();
            }
        }

        public Rishvi.Models.Authorization CreateNew(string email, string SessionID,
            string LinnworksUniqueIdentifier, string accountName, string clientid = "null", string secret = "null",
            string state = "null")
        {
            var new_auth = new Authorization()
            {
                Email = email,
                SessionID = SessionID,
                LinnworksUniqueIdentifier = LinnworksUniqueIdentifier,
                AccountName = accountName,
                ClientId = clientid,
                ClientSecret = secret,
                AuthorizationToken = state ?? Guid.NewGuid().ToString(),
                CreatedAt = DateTime.Now
            };
            _authorization.Add(new_auth);
            _unitOfWork.Commit();

            var output = new Rishvi.Models.Authorization
            {
                AuthorizationToken = new_auth.AuthorizationToken,
                Email = new_auth.Email,
                ClientId = new_auth.ClientId,
                ClientSecret = new_auth.ClientSecret,
                SessionID = new_auth.SessionID,
                LinnworksUniqueIdentifier = new_auth.LinnworksUniqueIdentifier,
                AccountName = new_auth.AccountName
            };
            SqlHelper.SystemLogInsert("AddNewUser", null, null, JsonConvert.SerializeObject(output), "AuthorizeToken", JsonConvert.SerializeObject(output), false, "clientId");
            return output;

        }
    }
}
