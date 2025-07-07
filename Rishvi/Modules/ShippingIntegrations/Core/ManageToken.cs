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

        public ManageToken(IUnitOfWork unitOfWork, IRepository<Rishvi.Models.Authorization> authorization)
        {
            _unitOfWork = unitOfWork;
            _authorization = authorization;
        }

        public TokenDetails GetToken(AuthorizationConfigClass authorizationConfig)
        {
            if (!string.IsNullOrEmpty(authorizationConfig.ClientId) && !string.IsNullOrEmpty(authorizationConfig.ClientSecret))
            {
                // Create an instance of ManageToken to call the non-static method
                //var manageTokenInstance = new ManageToken(null, null, null); // Pass appropriate arguments for IRepository and UnitOfWork if needed
                var _Resp = AuthorizeClient(authorizationConfig.ClientId, authorizationConfig.ClientSecret, authorizationConfig.AuthorizationToken);

                return new TokenDetails
                {
                    AccessToken = _Resp.AccessToken,
                    ExpiresIn = _Resp.ExpiresIn,
                    TokenType = _Resp.TokenType,
                    Scope = _Resp.Scope,
                };
            }
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
                    if (get_auth.ExpirationTime >= DateTime.UtcNow && !string.IsNullOrEmpty(get_auth.access_token))
                    {
                        authorizationModel.AccessToken = get_auth.access_token;
                        authorizationModel.ExpiresIn = (int)get_auth.expires_in;
                        authorizationModel.TokenType = get_auth.token_type;
                        authorizationModel.Scope = null;

                        isAuthorized = true;
                    }
                  
                       
                }


                //if (AwsS3.S3FileIsExists("Authorization", "StreamToken/" + ClientId + ".json").Result)
                //{
                //    string json = AwsS3.GetS3File("Authorization", "StreamToken/" + ClientId + ".json");
                //    authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(json);
                //    if (authorizationModel.ExpireTime >= DateTime.UtcNow && !string.IsNullOrEmpty(authorizationModel.AccessToken))
                //        isAuthorized = true;
                //    isAuthorized = false;
                //}

                if (!isAuthorized)
                {
                    string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                    //var client = new RestClient(ClientId.StartsWith("RIS") ? "https://www.demo.go2stream.net/api" : AppSettings.StreamApiBasePath);
                    var client = new RestClient(ClientId.StartsWith("RIS") ? StreamApiSettings.DemoUrl : AppSettings.StreamApiBasePath);
                    var request = new RestRequest(AWSParameter.GetConnectionString(AppSettings.StreamOAuthUrl), Method.Post);
                    request.AddJsonBody(new { grant_type = AWSParameter.GetConnectionString(AppSettings.GrantType), client_id = ClientId, client_secret = ClientSecret });
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Stream-Nonce", uniqueCode);
                    RestResponse<AuthorizationModel> response = client.Execute<AuthorizationModel>(request);

                    if (response.IsSuccessful)
                    {
                        authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(response.Content);

                        var jsonData = JsonConvert.SerializeObject(authorizationModel);
                        //Stream stream = AwsS3.GenerateStreamFromString(jsonData);
                        //AwsS3.UploadFileToS3("Authorization", stream, "StreamToken/" + ClientId + ".json");

                        get_auth.access_token = authorizationModel.AccessToken;
                        get_auth.token_type = authorizationModel.TokenType;
                        get_auth.UpdatedAt = DateTime.UtcNow;
                        get_auth.expires_in = authorizationModel.ExpiresIn;
                        get_auth.ExpirationTime = authorizationModel.ExpireTime;
                        _authorization.Update(get_auth);

                        _unitOfWork.Commit();
                    }
                    //else
                    //{
                    //    Stream stream = AwsS3.GenerateStreamFromString(response.Content);
                    //    AwsS3.UploadFileToS3("Authorization", stream, "StreamErrorToken/" + ClientId + "-" + uniqueCode + ".json");
                    //    Console.WriteLine("Error: " + response.ErrorMessage);
                    //}

                }
                return authorizationModel;
            }
            catch (WebException ex)
            {
                //string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                //if (ex.Response != null)
                //{
                //    using (Stream responseStream = ex.Response.GetResponseStream())
                //    {
                //        if (responseStream != null)
                //        {
                //            AwsS3.UploadFileToS3("Authorization", responseStream, "StreamErrorToken/" + ClientId + "-" + uniqueCode + ".json");
                //        }
                //    }
                //}
                //else
                //{
                //    Stream responseStream = AwsS3.GenerateStreamFromString(ex.ToString());
                //    AwsS3.UploadFileToS3("Authorization", responseStream, "StreamErrorToken/" + ClientId + "-" + uniqueCode + ".json");
                //    System.IO.File.WriteAllText("C:/Files/TheRange/User/Error.txt", ex.ToString());
                //}
            }
            return null;
        }
    }
}
