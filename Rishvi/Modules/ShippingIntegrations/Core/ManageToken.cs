using Amazon.Runtime.Internal;
using Hangfire.Storage;
using Newtonsoft.Json;
using RestSharp;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using Rishvi_Vault;
using System.Net;
using System.Text;
using ThirdParty.Json.LitJson;

namespace Rishvi.Modules.ShippingIntegrations.Core
{
    public static class ManageToken
    {

        public static TokenDetails GetToken(AuthorizationConfigClass authorizationConfig)
        {
            if (!string.IsNullOrEmpty(authorizationConfig.ClientId) && !string.IsNullOrEmpty(authorizationConfig.ClientSecret))
            {
                var _Resp = AuthorizeClient(authorizationConfig.ClientId, authorizationConfig.ClientSecret);

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

        private static AuthorizationModel AuthorizeClient(string ClientId, string ClientSecret)
        {
            try
            {
                bool isAuthorized = false;
                AuthorizationModel authorizationModel = new AuthorizationModel();
                if (AwsS3.S3FileIsExists("Authorization", "StreamToken/" + ClientId + ".json").Result)
                {
                    string json = AwsS3.GetS3File("Authorization", "StreamToken/" + ClientId + ".json");
                    authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(json);
                    if (authorizationModel.ExpireTime >= DateTime.UtcNow && !string.IsNullOrEmpty(authorizationModel.AccessToken))
                        isAuthorized = true;
                    isAuthorized = false;
                }

                if (!isAuthorized)
                {
                    string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                    var client = new RestClient(ClientId.StartsWith("RIS") ? "https://www.demo.go2stream.net/api" : AppSettings.StreamApiBasePath);
                    var request = new RestRequest(AWSParameter.GetConnectionString(AppSettings.StreamOAuthUrl), Method.Post);
                    request.AddJsonBody(new { grant_type = AWSParameter.GetConnectionString(AppSettings.GrantType), client_id = ClientId, client_secret= ClientSecret });
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Stream-Nonce", uniqueCode);
                    RestResponse<AuthorizationModel> response = client.Execute<AuthorizationModel>(request);

                    if (response.IsSuccessful)
                    {
                        authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(response.Content);

                        var jsonData = JsonConvert.SerializeObject(authorizationModel);
                        Stream stream = AwsS3.GenerateStreamFromString(jsonData);
                        AwsS3.UploadFileToS3("Authorization", stream, "StreamToken/" + ClientId + ".json");

                        //if (string.IsNullOrEmpty(authorizationModel.AccessToken))
                        //    AuthorizeClient(ClientId, ClientSecret);
                    }
                    else
                    {
                        Stream stream = AwsS3.GenerateStreamFromString(response.Content);
                        AwsS3.UploadFileToS3("Authorization", stream, "StreamErrorToken/" + ClientId + "-" + uniqueCode + ".json");
                        Console.WriteLine("Error: " + response.ErrorMessage);
                    }

                    //authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(response.Content);
                    ////authorizationModel.ExpireTime = DateTime.Now.AddSeconds(3600);

                    //var jsonData = JsonConvert.SerializeObject(authorizationModel);
                    //Stream stream = AwsS3.GenerateStreamFromString(jsonData);
                }
                return authorizationModel;
            }
            catch (WebException ex)
            {
                string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                if (ex.Response != null)
                {
                    using (Stream responseStream = ex.Response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            AwsS3.UploadFileToS3("Authorization", responseStream, "StreamErrorToken/" + ClientId + "-" + uniqueCode + ".json");
                            //var streamReader = new StreamReader(responseStream);
                            //string json = streamReader.ReadToEnd();

                            //System.IO.File.WriteAllText("C:/Files/TheRange/User/Error.txt", json);
                            //streamReader.Close();
                        }
                    }
                }
                else
                {
                    Stream responseStream = AwsS3.GenerateStreamFromString(ex.ToString());
                    AwsS3.UploadFileToS3("Authorization", responseStream, "StreamErrorToken/" + ClientId + "-" + uniqueCode + ".json");
                    System.IO.File.WriteAllText("C:/Files/TheRange/User/Error.txt", ex.ToString());
                }
            }
            return null;
        }
    }
}
