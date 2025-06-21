using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.ShippingIntegrations.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using YamlDotNet.Core.Tokens;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using Hangfire;


namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/Sync")]
    public class SyncController : ControllerBase
    {
        private readonly ConfigController _configController;
        private readonly LinnworksController _linnworksController;
        private readonly StreamController _streamController;
        private readonly MessinaSettings _settings;

        public SyncController(ConfigController configController,
            LinnworksController linnworksController,
            StreamController streamController, IOptions<MessinaSettings> settings)
        {
            _configController = configController;
            _linnworksController = linnworksController;
            _streamController = streamController;
            _settings = settings.Value;
        }

        [HttpPost, Route("RunService/{service}")]
        public async Task<bool> RunService([FromBody] SyncReq value, string service)
        {
            try
            {
                if (value.orderids != null && value.orderids != "")
                {
                    var data = await _configController.Get(value.email);
                    string LinnworksSyncToken = "";
                    string AuthorizationToken = "";

                    if (data is OkObjectResult okResult)
                    {
                        var getData = okResult.Value as RegistrationData;
                        LinnworksSyncToken = getData?.LinnworksSyncToken ?? string.Empty;
                        AuthorizationToken = getData?.AuthorizationToken ?? string.Empty;
                    }

                    //if (service == "SyncEbayOrder")
                    //{
                    //    await _ebayController.GetOrders(AuthorizationToken, value.orderids, 500, 10);
                    //}
                    if (service == "SyncLinnworksOrder")
                    {
                        await _linnworksController.GetLinnOrderForStream(AuthorizationToken, LinnworksSyncToken, value.orderids, 500, 10);
                    }
                    //else if (service == "CreateEbayOrderToStream")
                    //{
                    //    await _linnworksController.CreateEbayOrdersToStream(AuthorizationToken, value.orderids);
                    //}
                    else if (service == "CreateLinnworksOrderToStream")
                    {
                        await _linnworksController.CreateLinnworksOrdersToStream(AuthorizationToken, value.orderids);
                    }
                    else if (service == "DispatchLinnworksOrderFromStream")
                    {
                        await _linnworksController.DispatchLinnworksOrdersFromStream(AuthorizationToken, value.orderids, LinnworksSyncToken);
                    }
                    //else if (service == "DispatchEbayOrderFromStream")
                    //{
                    //    await _ebayController.DispatchOrderFromStream(AuthorizationToken, value.orderids);
                    //}
                    else
                    {
                        // Optionally handle unknown service types here
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false; // Return false if an error occurs
            }
        }

        // run service
        [HttpGet, Route("StartService")]
        [AllowAnonymous]
        public async Task<bool> StartService()
        {
            try
            {
                var listuser = await AwsS3.ListFilesInS3Folder("Authorization/Users");
                foreach (var _user in listuser)
                {
                    var userdata = AwsS3.GetS3File("Authorization", _user.Replace("Authorization/", ""));
                    var res = JsonConvert.DeserializeObject<RegistrationData>(userdata);

                    if (res.Sync == null)
                    {
                        res.Sync = new SyncModel();
                    }

                    //if (res.Sync.SyncEbayOrder)
                    //{
                    //    await _ebayController.GetOrders(res.AuthorizationToken, "", res.ebayhour, res.ebaypage);
                    //}
                    if (res.Sync.SyncLinnworksOrder)
                    {
                        await _linnworksController.GetLinnOrderForStream(res.AuthorizationToken, res.LinnworksSyncToken, "", res.linnhour, res.linnpage);
                    }
                    //if (res.Sync.CreateEbayOrderToStream)
                    //{
                    //    await _streamController.CreateEbayOrdersToStream(res.AuthorizationToken, "");
                    //}
                    if (res.Sync.CreateLinnworksOrderToStream)
                    {
                        await _linnworksController.CreateLinnworksOrdersToStream(res.AuthorizationToken, "");
                    }
                    if (res.Sync.UpdateLinnworksOrderToStream)
                    {
                        await _linnworksController.UpdateLinnworksOrdersToStream(res.AuthorizationToken, "");
                    }
                    //if (res.Sync.DispatchEbayOrderFromStream)
                    //{
                    //    await _ebayController.DispatchOrderFromStream(res.AuthorizationToken, "");
                    //}
                    if (res.Sync.DispatchLinnworksOrderFromStream)
                    {
                        await _linnworksController.DispatchLinnworksOrdersFromStream(res.AuthorizationToken, "", res.LinnworksSyncToken);
                    }

                    res.LastSyncOn = DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss");
                    res.LastSyncOnDate = DateTime.Now;
                    await _configController.Save(res);

                    new MessianApiOAuthHelper().SyncLogs(
                        DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                        res.AuthorizationToken,
                        DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")
                    );
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false; // Indicate failure
            }
        }

        [HttpGet("setup-recurring-job")]
        [AllowAnonymous]
        public IActionResult SetupRecurringJob()
        {
            RecurringJob.AddOrUpdate<SyncController>(
     "Order-sync-linn-to-stream",
     x => x.StartService(),
     "*/30 * * * *"  // Every 30 minutes
 );

            return Ok("Recurring job setup complete.");
        }
    }

    public class SyncReq
    {
        public string email { get; set; }

        public string orderids { get; set; }
    }
}
