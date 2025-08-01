﻿using LinnworksAPI;
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
        private readonly ILogger<SetupController> _logger;
        public SetupController(IAuthorizationToken authorizationToken, IUnitOfWork unitOfWork, ManageToken manageToken, IRepository<Authorization> authorizationRepository, ILogger<SetupController> logger)
        {
            _authorizationToken = authorizationToken;
            _unitOfWork = unitOfWork;
            _manageToken = manageToken;
            _authorizationRepository = authorizationRepository;
            _logger = logger;
        }

        [HttpPost, Route("AddNewUser")]
        public async Task<AddNewUserResponse> AddNewUser([FromBody] AddNewUserRequest request)
        {
            SqlHelper.SystemLogInsert("AddNewUser", null, null, JsonConvert.SerializeObject(request), "AddNewUser", JsonConvert.SerializeObject(request), false, "clientId");
            try
            {
                _logger.LogInformation("AddNewUser request received: {Request}", JsonConvert.SerializeObject(request));

                // EmailHelper.SendEmail("stream Add User", JsonConvert.SerializeObject(request));
                // Validate input fields
                if (string.IsNullOrWhiteSpace(request.Email))
                    return new AddNewUserResponse("Invalid Email");

                if (string.IsNullOrWhiteSpace(request.AccountName))
                    return new AddNewUserResponse("Invalid AccountName");

                if (string.IsNullOrWhiteSpace(request.LinnworksUniqueIdentifier))
                    return new AddNewUserResponse("Invalid LinnworksUniqueIdentifier");

                // Create a new authorization configuration
                Rishvi.Models.Authorization newConfig = _authorizationToken.CreateNew(
                    request.Email,
                    "NULL",
                    request.LinnworksUniqueIdentifier,
                    request.AccountName
                );

                _logger.LogInformation("New authorization configuration created: {Config}", JsonConvert.SerializeObject(newConfig));

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
                _logger.LogError(ex, "An error occurred while processing AddNewUser request: {Request}", JsonConvert.SerializeObject(request));
            }
        }

        [HttpPost, Route("UserConfig")]
        public UserConfigResponse UserConfig([FromBody] UserConfigRequest request)
        {
            SqlHelper.SystemLogInsert("UserConfig", null, null, JsonConvert.SerializeObject(request), "UserConfigStart", JsonConvert.SerializeObject(request), false, "clientId");
            try
            {
                _logger.LogInformation("UserConfig request received: {Request}", JsonConvert.SerializeObject(request));
                //  var obj = new LinnworksBaseStream(request.AuthorizationToken);
                // Authenticate the user
                Rishvi.Models.Authorization auth = _authorizationToken.Load(request.AuthorizationToken);
                var authEntity = _authorizationRepository.Get(x => x.AuthorizationToken == request.AuthorizationToken).FirstOrDefault();
                if (auth == null || authEntity == null)
                {
                    _logger.LogWarning("Authorization failed for token: {Token}", request.AuthorizationToken);
                    return new UserConfigResponse()
                    {
                        IsError = true,
                        ErrorMessage = $"Authorization failed for token {request.AuthorizationToken}"
                    };
                }
                SqlHelper.SystemLogInsert("UserConfigauthEntity", null, null, JsonConvert.SerializeObject(authEntity), "UserConfigStart", JsonConvert.SerializeObject(authEntity), false, "clientId");
                // If config is not activated (Wizard Stage - integration steps)
                if ((bool)!auth.IsConfigActive)
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
                            //ConfigStage = Models.ConfigStageClasses.ContactStage.GetContactStage,
                            ConfigStage = Models.ConfigStageClasses.UserConfigStage.GetUserConfigStage(auth),
                            ConfigStatus = "ContactStage"
                        };
                        SqlHelper.SystemLogInsert("UserConfig ContactStage", null, null, JsonConvert.SerializeObject(ret), "UserConfigsucc", JsonConvert.SerializeObject(ret), false, "clientId");
                        _logger.LogInformation("Returning ContactStage configuration: {ConfigStage}", JsonConvert.SerializeObject(ret.ConfigStage));
                        return ret;
                    }
                    SqlHelper.SystemLogInsert("UserConfig ContactStage", null, null, JsonConvert.SerializeObject(authEntity), "UserConfigsucc", auth.ConfigStatus, false, "clientId");
                    _logger.LogInformation("Returning configuration for stage: {ConfigStatus}", auth.ConfigStatus);
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
                    _logger.LogInformation("Returning active configuration: {ConfigStage}", JsonConvert.SerializeObject(dta.ConfigStage));
                    return dta;
                }
            }
            catch (Exception ex)
            {
                // Log the error (replace with proper logging framework)
                Console.WriteLine($"An error occurred: {ex.Message}");
                SqlHelper.SystemLogInsert("UserConfig error", null, null, "JsonConvert.SerializeObject(dta)", "UserConfigsucc", ex.Message, true, "clientId");
                _logger.LogError(ex, "An error occurred while processing UserConfig request: {Request}", JsonConvert.SerializeObject(request));
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
                _logger.LogInformation("UpdateConfig request received: {Request}", JsonConvert.SerializeObject(request));
                // Authenticate the user and load the config file
                Rishvi.Models.Authorization auth = _authorizationToken.Load(request.AuthorizationToken);
                var authEntity = _authorizationRepository.Get(x => x.AuthorizationToken == request.AuthorizationToken).FirstOrDefault();
                if (auth == null || authEntity == null)
                {
                    _logger.LogWarning("Authorization failed for token: {Token}", request.AuthorizationToken);
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
                    authEntity.LabelReference = GetConfigValue(request, "LABELREFERENCE");
                    authEntity.AutoOrderSync = Convert.ToBoolean(GetConfigValue(request, "AutoOrderSync"));
                    authEntity.AutoOrderDespatchSync = Convert.ToBoolean(GetConfigValue(request, "AutoOrderDespatchSync"));
                    authEntity.UseDefaultLocation = Convert.ToBoolean(GetConfigValue(request, "UseDefaultLocation"));
                    authEntity.DefaultLocation = GetConfigValue(request, "DefaultLocation");
                    authEntity.LinnDays = Convert.ToInt32(GetConfigValue(request, "LinnDays"));
                    authEntity.SendChangeToStream = Convert.ToBoolean(GetConfigValue(request, "SendChangeToStream"));
                    authEntity.HandsOnDate = Convert.ToBoolean(GetConfigValue(request, "HandsOnDate"));
                    authEntity.IsLiveAccount = Convert.ToBoolean(GetConfigValue(request, "IsLiveAccount"));
                    authEntity.ConfigStatus = "CONFIG";
                    authEntity.IsConfigActive = true;
                    _authorizationRepository.Update(authEntity);

                    _unitOfWork.Commit();
                    _logger.LogInformation("Configuration updated successfully for ContactStage: {ConfigStatus}", auth.ConfigStatus);
                    return new UpdateConfigResponse();
                }
                // Handle the "CONFIG" stage or active configurations
                else if (auth.ConfigStatus == "CONFIG" || (auth.IsConfigActive.HasValue && auth.IsConfigActive.Value))
                {
                    // Allow changes to certain config properties
                    auth.AccountName = GetConfigValue(request, "NAME");
                    auth.AddressLine1 = GetConfigValue(request, "ADDRESS1");
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
                    authEntity.LabelReference = GetConfigValue(request, "LABELREFERENCE");
                    authEntity.AutoOrderSync = Convert.ToBoolean(GetConfigValue(request, "AutoOrderSync"));
                    authEntity.AutoOrderDespatchSync = Convert.ToBoolean(GetConfigValue(request, "AutoOrderDespatchSync"));
                    authEntity.UseDefaultLocation = Convert.ToBoolean(GetConfigValue(request, "UseDefaultLocation"));
                    authEntity.DefaultLocation = GetConfigValue(request, "DefaultLocation");
                    authEntity.LinnDays = Convert.ToInt32(GetConfigValue(request, "LinnDays"));
                    authEntity.SendChangeToStream = Convert.ToBoolean(GetConfigValue(request, "SendChangeToStream"));
                    authEntity.HandsOnDate = Convert.ToBoolean(GetConfigValue(request, "HandsOnDate"));
                    authEntity.IsLiveAccount = Convert.ToBoolean(GetConfigValue(request, "IsLiveAccount"));
                    _authorizationRepository.Update(authEntity);
                    _unitOfWork.Commit();
                    _logger.LogInformation("Configuration updated successfully for CONFIG stage: {ConfigStatus}", auth.ConfigStatus);
                    return new UpdateConfigResponse();
                }
                else
                {
                    _logger.LogWarning("Unhandled config stage: {ConfigStatus}", auth.ConfigStatus);
                    // Return error for unhandled config stages
                    return new UpdateConfigResponse($"{auth.ConfigStatus} is not handled.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing UpdateConfig request: {Request}", JsonConvert.SerializeObject(request));
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
            SqlHelper.SystemLogInsert("ConfigDelete", null, null, JsonConvert.SerializeObject(request), "ConfigDelete Request", null, false, "clientId");
            try
            {
                _logger.LogInformation("ConfigDelete request received: {Request}", JsonConvert.SerializeObject(request));
                // Load and authenticate the configuration using the authorization token
                Rishvi.Models.Authorization auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                {
                    _logger.LogInformation("ConfigDelete - Unuthorized fro token - {AuthorizationToken}", request.AuthorizationToken);
                    return new ConfigDeleteResponse($"Authorization failed for token {request.AuthorizationToken}");
                }

                // Delete the configuration for the given authorization token
                _authorizationToken.Delete(request.AuthorizationToken);
                _logger.LogInformation("Config Delete Successfull for json {request}", request);
                // Return a successful response
                return new ConfigDeleteResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in ConfigDelete");
                SqlHelper.SystemLogInsert("ConfigDelete", null, null, JsonConvert.SerializeObject(request), "ConfigDelete Error", ex.ToString(), false, "clientId");
                // Handle and return errors gracefully
                return new ConfigDeleteResponse($"Delete Config error: {ex.Message}");
            }
        }

        [HttpPost, Route("UserAvailableServices")]
        public UserAvailableServicesResponse UserAvailableServices([FromBody] UserAvailableServicesRequest request)
        {
            _logger.LogInformation("UserAvailableServices request received: {Request}", JsonConvert.SerializeObject(request));
            // Authenticate the user using the provided authorization token
            Rishvi.Models.Authorization auth = _authorizationToken.Load(request.AuthorizationToken);
            if (auth == null)
            {
                _logger.LogWarning("Authorization failed for token: {Token}", request.AuthorizationToken);
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
            _logger.LogInformation("ExtendedPropertyMapping request received: {Request}", JsonConvert.SerializeObject(request));
            // Load and authenticate the user configuration using the provided authorization token
            Rishvi.Models.Authorization auth = _authorizationToken.Load(request.AuthorizationToken);
            if (auth == null)
            {
                _logger.LogWarning("Authorization failed for token: {Token}", request.AuthorizationToken);
                // Return an error response if authentication fails
                return new ExtendedPropertyMappingResponse($"Authorization failed for token {request.AuthorizationToken}");
            }

            _logger.LogInformation("Authorization successful for token: {Token}", request.AuthorizationToken);
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
                _logger.LogInformation("SubscribeWebhook request received with authToken: {AuthToken}, eventname: {EventName}, event_type: {EventType}, url_path: {UrlPath}, http_method: {HttpMethod}, content_type: {ContentType}, auth_header: {AuthHeader}",
                    authToken, eventname, event_type, url_path, http_method, content_type, auth_header);
                // Load and authenticate the user configuration using the provided authorization token
                Rishvi.Models.Authorization auth = _authorizationToken.Load(authToken);
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
                    auth.ClientId,
                    auth.IsLiveAccount
                );
                _logger.LogInformation("Webhook subscription successful for authToken: {AuthToken}, eventname: {EventName}, event_type: {EventType}, url_path: {UrlPath}, http_method: {HttpMethod}, content_type: {ContentType}, auth_header: {AuthHeader}",
                    authToken, eventname, event_type, url_path, http_method, content_type, auth_header);
                // Return a successful response
                return new ExtendedPropertyMappingResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing SubscribeWebhook request: {AuthToken}, {EventName}, {EventType}, {UrlPath}, {HttpMethod}, {ContentType}, {AuthHeader}",
                    authToken, eventname, event_type, url_path, http_method, content_type, auth_header);
                // Handle any unexpected errors gracefully
                return new ExtendedPropertyMappingResponse($"Unhandled exception in SubscribeWebhook: {ex.Message}");
            }
        }

    }
}
