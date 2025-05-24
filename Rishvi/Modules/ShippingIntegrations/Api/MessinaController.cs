using Amazon.Runtime.Internal;
using Amazon.SimpleSystemsManagement.Model;
using Autofac.Core;
using Azure.Core;
using FluentFTP;
using LinnworksAPI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rishvi.Modules.Core.Authorization;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Core.Helper;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.BaseClasses;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using CsvHelper;
using ThirdParty.Json.LitJson;
using System.Security.Policy;
using Microsoft.Extensions.Options;

namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/Messina")]
    public class MessinaController : ControllerBase
    {
        //public static string host_name = "https://vetuzxfjukz4x63zcpr3433g7u0xncyc.lambda-url.eu-west-2.on.aws";
        //internal static string TradingAPI_ServerURL = "https://api.ebay.com/ws/api.dll";
        
        //internal static string DeveloperId = "56fda7f3-e803-4bbd-b6dd-c7eb377a971c";
        //internal static string ProdClientId = "RishviLt-Rishvite-PRD-edec1ac47-c979c619";
        //internal static string ProdRedirectURL = "Rishvi_Ltd-RishviLt-Rishvi-gcubzb";
        //internal static string ProdClientSecret = "PRD-dec1ac4746bd-577f-47d5-ba68-7eb9";
        //internal static string TradingAPI_Version = "1227";
        //internal static string webApiURL = "https://api.ebay.com/ws/api.dll";

        private readonly IAuthorizationToken _authorizationToken;
        private readonly MessinaSettings _settings;
        public MessinaController(IAuthorizationToken authorizationToken, IOptions<MessinaSettings> settings)
        {
            _authorizationToken = authorizationToken;
            _settings = settings.Value;
        }

        [HttpGet(), Route("Landing")]
        public async Task<ContentResult> Landing(string token, string email, string accountname)
        {
            try
            {
                string path = "<script  nonce=\"avsm220214\">document.location=\"{0}\"</script>";
                email = HttpUtility.UrlEncode(email);
                var helper = new MessianApiOAuthHelper(_settings);
                string newToken = Guid.NewGuid().ToString();
                var SessionID = helper.GetSessionID(newToken);
                AuthorizationConfigClass newConfig = _authorizationToken.CreateNew(email, SessionID, newToken, accountname, "", "", token);
                var AuthorizeUrl = "https://signin.ebay.com/ws/eBayISAPI.dll?SignIn&runame={RU_NAME}&SessID={SESSION_ID}";
                return new ContentResult
                {
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.Redirect,
                    Content = String.Format(path, AuthorizeUrl.Replace("{SESSION_ID}", SessionID)
                    .Replace("{RU_NAME}", _settings.ProdRedirectURL))
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet(), Route("Install")]
        public async Task<ContentResult> Install(string token)
        {
            string path = "<script  nonce=\"avsm220214\">document.location=\"{0}\"</script>";
            var user = _authorizationToken.Load(token);
            if (!string.IsNullOrEmpty(user.SessionID))
            {
                await new MessianApiOAuthHelper(_settings).GenerateToken(user, user.SessionID);
            }
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.Redirect,
                Content = String.Format(path, _settings.HostName + "/messinacomplete.html")
            };
        }

        [HttpGet(), Route("PriceUpdate")]
        public async Task PriceUpdate(string token,string filename)
        {
            var user = _authorizationToken.Load(token);
            if (!string.IsNullOrEmpty(user.access_token))
            {
                user.FtpPassword = String.IsNullOrEmpty(user.FtpPassword) ? _settings.FtpPassword : user.FtpPassword;
                user.FtpUsername = String.IsNullOrEmpty(user.FtpUsername) ? _settings.FtpUsername : user.FtpUsername;
                user.FtpHost = String.IsNullOrEmpty(user.FtpHost) ? _settings.FtpHost : user.FtpHost;
                var ftpConfig = new FtpClient(user.FtpHost)
                {
                    Credentials = new System.Net.NetworkCredential(user.FtpUsername,user.FtpPassword)
                };
                string remoteFilePath = "/Messina/" + filename + ".csv"; // Replace with your FTP file path
                string localFilePath = "local-file.csv"; // Local path to save the file
                new MessianApiOAuthHelper(_settings).DownloadFileFromFtpAsync(ftpConfig, remoteFilePath, localFilePath, user);
                new MessianApiOAuthHelper(_settings).ReadCsvAndBatchProcessAsync(localFilePath, 3, user);
            }
        }
    }   
}
