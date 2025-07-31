using Amazon.Runtime.Internal;
using Hangfire.Storage;
using Newtonsoft.Json;
using RestSharp;
using Rishvi.Models;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using Rishvi_Vault;
using System.Net;
using System.Text;
using ThirdParty.Json.LitJson;

namespace Rishvi.Modules.ShippingIntegrations.Core
{
    public class ManageToken
    {
        private readonly IRepository<Rishvi.Models.Authorization> _authorization;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ManageToken> _logger;

        public ManageToken(IUnitOfWork unitOfWork, IRepository<Rishvi.Models.Authorization> authorization, ILogger<ManageToken> logger)
        {
            _unitOfWork = unitOfWork;
            _authorization = authorization;
            _logger = logger;
        }

        public TokenDetails GetToken(Rishvi.Models.Authorization authorizationConfig)
        {
            _logger.LogInformation("GetToken called with ClientId: {ClientId}", authorizationConfig.ClientId);
            if (!string.IsNullOrEmpty(authorizationConfig.ClientId) && !string.IsNullOrEmpty(authorizationConfig.ClientSecret))
            {
                // Create an instance of ManageToken to call the non-static method
                //var manageTokenInstance = new ManageToken(null, null, null); // Pass appropriate arguments for IRepository and UnitOfWork if needed
                var _Resp = AuthorizeClient(authorizationConfig.ClientId, authorizationConfig.ClientSecret, authorizationConfig.AuthorizationToken);
                _logger.LogInformation("Authorization response received for ClientId: {ClientId}", authorizationConfig.ClientId);
                return new TokenDetails
                {
                    AccessToken = _Resp.AccessToken,
                    ExpiresIn = _Resp.ExpiresIn,
                    TokenType = _Resp.TokenType,
                    Scope = _Resp.Scope,
                };
            }
            _logger.LogWarning("ClientId or ClientSecret is null or empty for authorizationConfig: {AuthorizationConfig}", authorizationConfig);
            return null;
        }

        private AuthorizationModel AuthorizeClient(string ClientId, string ClientSecret, string AuthorizationToken)
        {
            try
            {
                bool isAuthorized = false;
                AuthorizationModel authorizationModel = new AuthorizationModel();
                var get_auth = _authorization.Get().Where(x => x.ClientId == ClientId && x.AuthorizationToken == AuthorizationToken).FirstOrDefault();
                if (get_auth != null)
                {
                    if (get_auth.ExpirationTime >= DateTime.Now && !string.IsNullOrEmpty(get_auth.access_token))
                    {
                        authorizationModel.AccessToken = get_auth.access_token;
                        authorizationModel.ExpiresIn = (int)get_auth.expires_in;
                        authorizationModel.TokenType = get_auth.token_type;
                        authorizationModel.Scope = null;

                        isAuthorized = true;
                    }
                }

                if (!isAuthorized)
                {
                    string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                    //var client = new RestClient(ClientId.StartsWith("RIS") ? "https://www.demo.go2stream.net/api" : AppSettings.StreamApiBasePath);
                    var API = get_auth.IsLiveAccount ? AppSettings.StreamApiBasePath : StreamApiSettings.DemoUrl;
                    var client = new RestClient(API);
                    var request = new RestRequest(AppSettings.StreamOAuthUrl, Method.Post);
                    request.AddJsonBody(new { grant_type = AppSettings.GrantType, client_id = ClientId, client_secret = ClientSecret });
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Stream-Nonce", uniqueCode);
                    RestResponse<AuthorizationModel> response = client.Execute<AuthorizationModel>(request);

                    if (response.IsSuccessful)
                    {
                        authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(response.Content);

                        var jsonData = JsonConvert.SerializeObject(authorizationModel);

                        get_auth.access_token = authorizationModel.AccessToken;
                        get_auth.token_type = authorizationModel.TokenType;
                        get_auth.UpdatedAt = DateTime.UtcNow;
                        get_auth.expires_in = authorizationModel.ExpiresIn;
                        get_auth.ExpirationTime = authorizationModel.ExpireTime;
                        _authorization.Update(get_auth);

                        _unitOfWork.Commit();
                    }
                    
                }
                return authorizationModel;
            }
            catch (WebException ex)
            {
                SqlHelper.SystemLogInsert("ManageToken", null, null, ClientId, "AuthorizeClient", ex.Message, true, ClientId);
                return null;
            }
            
        }
    }
}
