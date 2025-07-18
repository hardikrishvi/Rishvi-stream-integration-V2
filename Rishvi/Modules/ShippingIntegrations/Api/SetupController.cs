using LinnworksAPI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rishvi.Models;
using Rishvi.Modules.Core.Authorization;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.BaseClasses;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using System.Security.Cryptography;

namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/Setup")]
    public class SetupController : ControllerBase
    {
        private readonly IAuthorizationToken _authorizationToken;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ManageToken _manageToken;
        private readonly IRepository<Authorization> _authorizationRepository;
        public SetupController(IAuthorizationToken authorizationToken, IUnitOfWork unitOfWork, ManageToken manageToken, IRepository<Authorization> authorizationRepository)
        {
            _authorizationToken = authorizationToken;
            _unitOfWork = unitOfWork;
            _manageToken = manageToken;
            _authorizationRepository = authorizationRepository;
        }

        [HttpPost, Route("AddNewUser")]
        public async Task<AddNewUserResponse> AddNewUser([FromBody] AddNewUserRequest request)
        {
            SqlHelper.SystemLogInsert("AddNewUser", null, null, JsonConvert.SerializeObject(request), "AddNewUser", JsonConvert.SerializeObject(request), false, "clientId");
            try
            {

              
                // EmailHelper.SendEmail("stream Add User", JsonConvert.SerializeObject(request));
                // Validate input fields
                if (string.IsNullOrWhiteSpace(request.Email))
                    return new AddNewUserResponse("Invalid Email");

                if (string.IsNullOrWhiteSpace(request.AccountName))
                    return new AddNewUserResponse("Invalid AccountName");

                if (string.IsNullOrWhiteSpace(request.LinnworksUniqueIdentifier))
                    return new AddNewUserResponse("Invalid LinnworksUniqueIdentifier");

                // Create a new authorization configuration
                AuthorizationConfigClass newConfig = _authorizationToken.CreateNew(
                    request.Email,
                    "NULL",
                    request.LinnworksUniqueIdentifier,
                    request.AccountName
                );

             

                //obj.Api.Orders
                // Return a successful response
                return await Task.FromResult(new AddNewUserResponse
                {
                    AuthorizationToken = newConfig.AuthorizationToken.ToString()
                });
            }
            catch (Exception ex)
            {
                // Handle exceptions gracefully
                return new AddNewUserResponse("AddNewUser error: " + ex.Message);
            }
        }

        [HttpPost, Route("UserConfig")]
        public UserConfigResponse UserConfig([FromBody] UserConfigRequest request)
        {
            SqlHelper.SystemLogInsert("UserConfig", null, null, JsonConvert.SerializeObject(request), "UserConfigStart", JsonConvert.SerializeObject(request), false, "clientId");
            try
            {
              //  var obj = new LinnworksBaseStream(request.AuthorizationToken);
                // Authenticate the user
                AuthorizationConfigClass auth = _authorizationToken.Load(request.AuthorizationToken);
                var authEntity = _authorizationRepository.Get(x => x.AuthorizationToken == request.AuthorizationToken).FirstOrDefault();
                if (auth == null || authEntity == null)
                {
                    return new UserConfigResponse()
                    {
                        IsError = true,
                        ErrorMessage = $"Authorization failed for token {request.AuthorizationToken}"
                    };
                }
                SqlHelper.SystemLogInsert("UserConfigauthEntity", null, null, JsonConvert.SerializeObject(authEntity), "UserConfigStart", JsonConvert.SerializeObject(authEntity), false, "clientId");
                // If config is not activated (Wizard Stage - integration steps)
                if (!auth.IsConfigActive)
                {
                    // Assign a default configuration stage for new integrations
                    if (string.IsNullOrEmpty(auth.ConfigStatus))
                    {
                        auth.ConfigStatus = "ContactStage";
                        authEntity.ConfigStatus = "ContactStage";
                        //auth.Save();
                        _authorizationRepository.Update(authEntity);
                        _unitOfWork.Commit();
                    }

                    // Handle specific config stage
                    if (auth.ConfigStatus == "ContactStage")
                    {
                        var ret = new UserConfigResponse()
                        {
                            ConfigStage = Models.ConfigStageClasses.ContactStage.GetContactStage,
                            ConfigStatus = "ContactStage"
                        };
                        SqlHelper.SystemLogInsert("UserConfig ContactStage", null, null, JsonConvert.SerializeObject(ret), "UserConfigsucc", JsonConvert.SerializeObject(ret), false, "clientId");
                        return ret; 
                    }
                    SqlHelper.SystemLogInsert("UserConfig ContactStage", null, null, JsonConvert.SerializeObject(authEntity), "UserConfigsucc", auth.ConfigStatus, false, "clientId");
                    // Return error for unhandled config stages
                    return new UserConfigResponse($"Config stage is not handled: {auth.ConfigStatus}");
                }
                else
                {
                    // Active configuration stage (completed integration)
                    var dta = new UserConfigResponse()
                    {
                        ConfigStage = Models.ConfigStageClasses.UserConfigStage.GetUserConfigStage(auth),
                        IsConfigActive = true, // MUST SET THIS TO TRUE for the config to be treated as completed
                        ConfigStatus = "CONFIG"
                    };
                    SqlHelper.SystemLogInsert("UserConfig ContactStage", null, null, JsonConvert.SerializeObject(dta), "UserConfigsucc", auth.ConfigStatus, false, "clientId");
                    return dta;
                }
            }
            catch (Exception ex)
            {
                // Log the error (replace with proper logging framework)
                Console.WriteLine($"An error occurred: {ex.Message}");
                SqlHelper.SystemLogInsert("UserConfig error", null, null, "JsonConvert.SerializeObject(dta)", "UserConfigsucc", ex.Message, true, "clientId");
                // Return error response
                return new UserConfigResponse()
                {
                    IsError = true,
                    ErrorMessage = $"UserConfig error: {ex.Message}"
                };
            }
        }

        [HttpPost, Route("UpdateConfig")]
        public UpdateConfigResponse UpdateConfig([FromBody] UpdateConfigRequest request)
        {
            SqlHelper.SystemLogInsert("UpdateConfig", null, null, JsonConvert.SerializeObject(request), "UpdateConfig", JsonConvert.SerializeObject(request), false, "clientId");
            try
            {
                // Authenticate the user and load the config file
                AuthorizationConfigClass auth = _authorizationToken.Load(request.AuthorizationToken);
                var authEntity = _authorizationRepository.Get(x => x.AuthorizationToken == request.AuthorizationToken).FirstOrDefault();
                if (auth == null || authEntity == null)
                {
                    return new UpdateConfigResponse($"Authorization failed for token {request.AuthorizationToken}");
                }

                // Ensure the config stage matches the one sent in the request
                if (auth.ConfigStatus != request.ConfigStatus)
                {
                    return new UpdateConfigResponse("Current config stage does not match the request stage.");
                }

                // Handle the "ContactStage" configuration stage
                if (auth.ConfigStatus == "ContactStage")
                {
                    // Update configuration details from request items
                    auth.AccountName = GetConfigValue(request, "NAME");
                    auth.CompanyName = GetConfigValue(request, "COMPANY");
                    auth.AddressLine1 = GetConfigValue(request, "ADDRESS1");
                    auth.AddressLine2 = GetConfigValue(request, "ADDRESS2");
                    auth.AddressLine3 = GetConfigValue(request, "ADDRESS3");
                    auth.City = GetConfigValue(request, "CITY");
                    auth.County = GetConfigValue(request, "REGION");
                    auth.CountryCode = GetConfigValue(request, "COUNTRY");
                    auth.ContactPhoneNo = GetConfigValue(request, "TELEPHONE");
                    auth.PostCode = GetConfigValue(request, "POSTCODE");
                    auth.ClientId = GetConfigValue(request, "ClientId");
                    auth.ClientSecret = GetConfigValue(request, "ClientSecret");
                    auth.AutoOrderSync =  Convert.ToBoolean(GetConfigValue(request, "AutoOrderSync"));
                    auth.AutoOrderDespatchSync = Convert.ToBoolean(GetConfigValue(request, "AutoOrderDespatchSync"));
                    auth.UseDefaultLocation = Convert.ToBoolean(GetConfigValue(request, "UseDefaultLocation"));
                    auth.DefaultLocation = GetConfigValue(request, "DefaultLocation");
                    auth.LinnHour = Convert.ToInt32(GetConfigValue(request, "LinnHour"));
                    auth.SendChangeToStream = Convert.ToBoolean(GetConfigValue(request, "SendChangeToStream"));
                    auth.HandsOnDate = Convert.ToBoolean(GetConfigValue(request, "HandsOnDate"));

                    // Mark config as active and update status
                    auth.ConfigStatus = "CONFIG";
                    auth.IsConfigActive = true;
                    //auth.Save();

                    authEntity.AccountName = GetConfigValue(request, "NAME");
                    authEntity.CompanyName = GetConfigValue(request, "COMPANY");
                    authEntity.AddressLine1 = GetConfigValue(request, "ADDRESS1");
                    authEntity.AddressLine2 = GetConfigValue(request, "ADDRESS2");
                    authEntity.AddressLine3 = GetConfigValue(request, "ADDRESS3");
                    authEntity.City = GetConfigValue(request, "CITY");
                    authEntity.County = GetConfigValue(request, "REGION");
                    authEntity.CountryCode = GetConfigValue(request, "COUNTRY");
                    authEntity.ContactPhoneNo = GetConfigValue(request, "TELEPHONE");
                    authEntity.PostCode = GetConfigValue(request, "POSTCODE");
                    authEntity.ClientId = GetConfigValue(request, "ClientId");
                    authEntity.ClientSecret = GetConfigValue(request, "ClientSecret");
                   
                    authEntity.AutoOrderSync = Convert.ToBoolean(GetConfigValue(request, "AutoOrderSync"));
                    authEntity.AutoOrderDespatchSync = Convert.ToBoolean(GetConfigValue(request, "AutoOrderDespatchSync"));
                    authEntity.UseDefaultLocation = Convert.ToBoolean(GetConfigValue(request, "UseDefaultLocation"));
                    authEntity.DefaultLocation = GetConfigValue(request, "DefaultLocation");
                    authEntity.LinnHour = Convert.ToInt32(GetConfigValue(request, "LinnHour"));
                    authEntity.SendChangeToStream = Convert.ToBoolean(GetConfigValue(request, "SendChangeToStream"));
                    authEntity.HandsOnDate = Convert.ToBoolean(GetConfigValue(request, "HandsOnDate"));
                    authEntity.ConfigStatus = "CONFIG";
                    authEntity.IsConfigActive = true;
                    _authorizationRepository.Update(authEntity);
                  
                    _unitOfWork.Commit();

                    return new UpdateConfigResponse();
                }
                // Handle the "CONFIG" stage or active configurations
                else if (auth.ConfigStatus == "CONFIG" || auth.IsConfigActive)
                {
                    // Allow changes to certain config properties
                    auth.AccountName = GetConfigValue(request, "NAME");
                    auth.AddressLine1 = GetConfigValue(request, "ADDRESS1");
                    //auth.Save();

                    authEntity.AccountName = GetConfigValue(request, "NAME");
                    authEntity.AddressLine1 = GetConfigValue(request, "ADDRESS1");
                    _authorizationRepository.Update(authEntity);
                    _unitOfWork.Commit();

                    return new UpdateConfigResponse();
                }
                else
                {
                    // Return error for unhandled config stages
                    return new UpdateConfigResponse($"{auth.ConfigStatus} is not handled.");
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions gracefully
                return new UpdateConfigResponse($"Unhandled exception saving user config: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to retrieve the selected value for a given config item ID.
        /// </summary>
        /// <param name="request">UpdateConfigRequest object.</param>
        /// <param name="configItemId">The ID of the config item to find.</param>
        /// <returns>The selected value of the config item, or an empty string if not found.</returns>
        private string GetConfigValue(UpdateConfigRequest request, string configItemId)
        {
            var configItem = request.ConfigItems.Find(s => s.ConfigItemId == configItemId);
            return configItem?.SelectedValue ?? string.Empty;
        }

        [HttpPost, Route("ConfigDelete")]
        public ConfigDeleteResponse ConfigDelete([FromBody] ConfigDeleteRequest request)
        {
            try
            {
                // Load and authenticate the configuration using the authorization token
                AuthorizationConfigClass auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                {
                    return new ConfigDeleteResponse($"Authorization failed for token {request.AuthorizationToken}");
                }

                // Delete the configuration for the given authorization token
                _authorizationToken.Delete(request.AuthorizationToken);

                // Return a successful response
                return new ConfigDeleteResponse();
            }
            catch (Exception ex)
            {
                // Handle and return errors gracefully
                return new ConfigDeleteResponse($"Delete Config error: {ex.Message}");
            }
        }

        [HttpPost, Route("UserAvailableServices")]
        public UserAvailableServicesResponse UserAvailableServices([FromBody] UserAvailableServicesRequest request)
        {
            // Authenticate the user using the provided authorization token
            AuthorizationConfigClass auth = _authorizationToken.Load(request.AuthorizationToken);
            if (auth == null)
            {
                return new UserAvailableServicesResponse($"Authorization failed for token {request.AuthorizationToken}");
            }

            // Return available services if the user is authenticated
            return new UserAvailableServicesResponse()
            {
                Services = Services.GetServices
            };
        }

        [HttpPost, Route("ExtendedPropertyMapping")]
        public ExtendedPropertyMappingResponse ExtendedPropertyMapping([FromBody] ExtendedPropertyMappingRequest request)
        {
            // Load and authenticate the user configuration using the provided authorization token
            AuthorizationConfigClass auth = _authorizationToken.Load(request.AuthorizationToken);
            if (auth == null)
            {
                // Return an error response if authentication fails
                return new ExtendedPropertyMappingResponse($"Authorization failed for token {request.AuthorizationToken}");
            }

            // Create and return the mapping response
            return new ExtendedPropertyMappingResponse()
            {
                Items = new List<ExtendedPropertyMapping>
                {
                    new ExtendedPropertyMapping
                    {
                        PropertyName = "SafePlace1",
                        PropertyTitle = "Safe Place note",
                        PropertyType = "ORDER",
                        PropertyDescription = "Safe place note for delivery"
                    },
                    new ExtendedPropertyMapping
                    {
                        PropertyName = "ExtendedCover",
                        PropertyTitle = "Extended Cover flag",
                        PropertyType = "ITEM",
                        PropertyDescription = "Identifies whether the item requires Extended Cover. Set to 1 if required."
                    }
                }
            };
        }

        [HttpGet, Route("SubscribeWebhook")]
        public async Task<ExtendedPropertyMappingResponse> SubscribeWebhook(
            string authToken,
            string eventname,
            string event_type,
            string url_path,
            string http_method,
            string content_type,
            string auth_header)
        {
            try
            {
                // Load and authenticate the user configuration using the provided authorization token
                AuthorizationConfigClass auth = _authorizationToken.Load(authToken);
                if (auth == null)
                {
                    // Return an error response if authentication fails
                    return new ExtendedPropertyMappingResponse($"Authorization failed for token {authToken}");
                }

                // Retrieve stream authorization token
                var streamAuth = _manageToken.GetToken(auth);
                //var manageToken = new ManageToken(_ClientAuth, _unitOfWork);
                //var streamAuth = manageToken.GetToken(auth);

                // Subscribe to the webhook using the StreamOrder API
                StreamOrderApi.WebhookSubscribe(
                    streamAuth.AccessToken,
                    authToken,
                    eventname,
                    event_type,
                    url_path,
                    http_method,
                    content_type,
                    auth_header,
                    auth.ClientId
                );

                // Return a successful response
                return new ExtendedPropertyMappingResponse();
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors gracefully
                return new ExtendedPropertyMappingResponse($"Unhandled exception in SubscribeWebhook: {ex.Message}");
            }
        }

    }
}
