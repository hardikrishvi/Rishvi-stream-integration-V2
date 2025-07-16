using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rishvi.Models;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Core.Helper;
using Rishvi.Modules.ShippingIntegrations.Models;

namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/Config")]
    public class ConfigController : ControllerBase
    {
        private readonly AwsS3 _awsS3;
        private readonly ServiceHelper _serviceHelper;
        private readonly TradingApiOAuthHelper _tradingApiOAuthHelper;
        private readonly SqlContext _dbContext;
        public ConfigController(AwsS3 awsS3, ServiceHelper serviceHelper, TradingApiOAuthHelper tradingApiOAuthHelper, SqlContext dbContext)
        {
            _awsS3 = awsS3;
            _serviceHelper = serviceHelper;
            _tradingApiOAuthHelper = tradingApiOAuthHelper;
            _dbContext = dbContext;
        }

        [HttpPost, Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegistrationData request)
        {
            try
            {
                var transformedEmail = (request.Email);
                var existsInDb = _dbContext.IntegrationSettings
                    .Any(x => x.Email == transformedEmail);

                if (!existsInDb)
                {
                    request.Password = _serviceHelper.HashPassword(request.Password);

                    _tradingApiOAuthHelper.RegisterSave(JsonConvert.SerializeObject(request), "", transformedEmail, request.AuthorizationToken);
                    return Ok(new { message = "ok" });
                }
                else
                {
                    return Conflict(new { message = "Email already registered." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login([FromBody] RegistrationData value)
        {
            try
            {
                var transformedEmail = _serviceHelper.TransformEmail(value.Email);
                var getData = _dbContext.IntegrationSettings
                    .FirstOrDefault(x => x.Email == transformedEmail);
                if (getData == null)
                {
                    return NotFound("Email not registered.");
                }
                var res = _tradingApiOAuthHelper.GetRegistrationData(transformedEmail);

                if (res.Password == _serviceHelper.HashPassword(value.Password))
                {
                    return Ok(new { message = "OK" }); 
                }
                else
                {
                    return Unauthorized(new { message = "Incorrect Password." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (use ILogger)
                Console.WriteLine($"Error during login: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet, Route("Get/{email}")]
        public async Task<IActionResult> Get(string email)
        {
            try
            {
                var getData = _dbContext.IntegrationSettings
                    .FirstOrDefault(x => x.Email == email);
                
                if (getData != null)
                {
                    var output = _tradingApiOAuthHelper.GetRegistrationData(email);
                    // Ensure SyncModel is not null
                    output.Sync ??= new SyncModel();
                    return Ok(output);
                }
                else
                {
                    return NotFound(new { message = "User not found" });
                }
            }
            catch (Exception ex)
            {
                // Log the error (replace with ILogger for production use)
                Console.WriteLine($"Error retrieving user data: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost, Route("Save")]
        public async Task<IActionResult> Save([FromBody] RegistrationData value)
        {
            try
            {
                // Ensure Sync is initialized
                value.Sync ??= new SyncModel();

                var transformedEmail = _serviceHelper.TransformEmail(value.Email);
                var getData = _dbContext.IntegrationSettings
                    .FirstOrDefault(x => x.Email == transformedEmail);

                if (getData != null)
                {
                    // Update existing user data
                    getData.Name = value.Name;
                    getData.Password = _serviceHelper.HashPassword(value.Password);
                    getData.AuthorizationToken = value.AuthorizationToken;
                    getData.LinnworksSyncToken = value.LinnworksSyncToken;

                    // Map LinnworksModel to LinnworksSettings
                    getData.Linnworks = new LinnworksSettings
                    {
                        DownloadOrderFromStream = value.Linnworks.DownloadOrderFromStream,
                        DownloadOrderFromEbay = value.Linnworks.DownloadOrderFromEbay,
                        PrintLabelFromStream = value.Linnworks.PrintLabelFromStream,
                        PrintLabelFromLinnworks = value.Linnworks.PrintLabelFromLinnworks,
                        DispatchOrderFromStream = value.Linnworks.DispatchOrderFromStream,
                        DispatchOrderFromEbay = value.Linnworks.DispatchOrderFromEbay,
                        SendChangeToEbay = value.Linnworks.SendChangeToEbay,
                        SendChangeToStream = value.Linnworks.SendChangeToStream,
                        CreatedAt = getData.Linnworks?.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Map StreamModel to StreamSettings
                    getData.Stream = new StreamSettings
                    {
                        GetTrackingDetails = value.Stream.GetTrackingDetails,
                        EnableWebhook = value.Stream.EnableWebhook,
                        SendChangeFromLinnworksToStream = value.Stream.SendChangeFromLinnworksToStream,
                        SendChangesFromEbayToStream = value.Stream.SendChangesFromEbayToStream,
                        CreateProductToStream = value.Stream.CreateProductToStream,
                        DownloadProductFromStreamToLinnworks = value.Stream.DownloadProductFromStreamToLinnworks,
                        GetRoutePlanFromStream = value.Stream.GetRoutePlanFromStream,
                        GetDepotListFromStream = value.Stream.GetDepotListFromStream,
                        CreatedAt = getData.Stream?.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Map SyncModel to SyncSettings
                    getData.Sync = new SyncSettings
                    {
                        SyncEbayOrder = value.Sync.SyncEbayOrder,
                        SyncLinnworksOrder = value.Sync.SyncLinnworksOrder,
                        CreateEbayOrderToStream = value.Sync.CreateEbayOrderToStream,
                        CreateLinnworksOrderToStream = value.Sync.CreateLinnworksOrderToStream,
                        DispatchLinnworksOrderFromStream = value.Sync.DispatchLinnworksOrderFromStream,
                        DispatchEbayOrderFromStream = value.Sync.DispatchEbayOrderFromStream,
                        UpdateLinnworksOrderToStream = value.Sync.UpdateLinnworksOrderToStream,
                        CreatedAt = getData.Sync?.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Update the assignment to map properties from EbayModel to Ebay
                    getData.Ebay = new Ebay
                    {
                        DownloadOrderFromEbay = value.Ebay.DownloadOrderFromEbay,
                        SendOrderToStream = value.Ebay.SendOrderToStream,
                        UpdateInformationFromEbayToStream = value.Ebay.UpdateInformationFromEbayToStream,
                        DispatchOrderFromEbay = value.Ebay.DispatchOrderFromEbay,
                        UpdateTrackingDetailsFromStream = value.Ebay.UpdateTrackingDetailsFromStream,
                        CreatedAt = getData.Ebay?.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    getData.LastSyncOnDate = value.LastSyncOnDate;
                    getData.LastSyncOn = value.LastSyncOn;
                    getData.ebaypage = value.ebaypage;
                    getData.ebayhour = value.ebayhour;
                    getData.linnpage = value.linnpage;
                    getData.linnhour = value.linnhour;

                    _dbContext.IntegrationSettings.Update(getData);
                    await _dbContext.SaveChangesAsync();
                    return Ok(new { message = "User data saved successfully." });
                }
                else
                {
                    return NotFound(new { message = "Email not registered." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (replace with ILogger for production)
                Console.WriteLine($"Error while saving data: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
