using Hangfire;
using LinnworksAPI;
using LinnworksMacroHelpers.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rishvi.Models;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Models;
using System.Text;
using System.Text.Json;
using YamlDotNet.Core.Tokens;
using ZXing.Aztec.Internal;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/Sync")]
    public class SyncController : ControllerBase
    {
        //private readonly ConfigController _configController;
        //private readonly LinnworksController _linnworksController;
        //private readonly StreamController _streamController;
        //private readonly MessinaSettings _settings;
        //private readonly TradingApiOAuthHelper _tradingApiOAuthHelper;
        //private readonly IRepository<IntegrationSettings> _integrationSettingsRepository;
        //private readonly IUnitOfWork _unitOfWork;
        //private readonly IRepository<SyncSettings> _syncSettings;
        //private readonly SqlContext _dbSqlCContext;
        private readonly ConfigController _configController;
        private readonly LinnworksController _linnworksController;
        private readonly StreamController _streamController;
        private readonly MessinaSettings _settings;
        private readonly TradingApiOAuthHelper _tradingApiOAuthHelper;
        private readonly IRepository<IntegrationSettings> _integrationSettingsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly SqlContext _dbSqlCContext;
        public SyncController(ConfigController configController, LinnworksController linnworksController, StreamController streamController,
            IOptions<MessinaSettings> settings, TradingApiOAuthHelper tradingApiOAuthHelper, IRepository<IntegrationSettings> integrationSettingRepository,
            IUnitOfWork unitOfWork, SqlContext dbSqlCContext)
        {
            _configController = configController;
            _linnworksController = linnworksController;
            _streamController = streamController;
            _settings = settings.Value;
            _tradingApiOAuthHelper = tradingApiOAuthHelper;
            _integrationSettingsRepository = integrationSettingRepository;
            _unitOfWork = unitOfWork;
            _dbSqlCContext = dbSqlCContext;
        }


        //public SyncController(ConfigController configController,
        //    LinnworksController linnworksController,
        //    StreamController streamController, IOptions<MessinaSettings> settings, TradingApiOAuthHelper tradingApiOAuthHelper,
        //    IRepository<IntegrationSettings> integrationSettingRepository, IUnitOfWork unitOfWork, IRepository<SyncSettings> syncSettings,SqlContext dbSqlCContext)
        //{
        //    _configController = configController;
        //    _linnworksController = linnworksController;
        //    _streamController = streamController;
        //    _settings = settings.Value;
        //    _tradingApiOAuthHelper = tradingApiOAuthHelper;
        //    _integrationSettingsRepository = integrationSettingRepository;
        //    _unitOfWork = unitOfWork;
        //    _syncSettings = syncSettings;
        //    _dbSqlCContext = dbSqlCContext; 
        //}

        [HttpPost, Route("RunService/{service}")]
        public async Task<bool> RunService([FromBody] SyncReq value, string service)
        {
            string Email = "";
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
                        //await _linnworksController.GetLinnOrderForStream(AuthorizationToken, LinnworksSyncToken, value.orderids, 500, 10);
                    }
                    else if (service == "CreateLinnworksOrderToStream")
                    {
                        await _linnworksController.CreateLinnworksOrdersToStream(AuthorizationToken, value.orderids);
                    }
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
        //[HttpGet, Route("StartService")]
        //[AllowAnonymous]
        //public async Task<bool> StartService()
        //{
        //    string Email = "";
        //    try
        //    {
        //        //var listuser = await AwsS3.ListFilesInS3Folder("Authorization/Users");
        //        // var listuser = _integrationSettingsRepository.Get().ToList();
        //        //var listuser = _dbSqlCContext.IntegrationSettings.ToList();

        //        var listuser = _dbSqlCContext.IntegrationSettings.Include(o => o.Sync).Include(o => o.Linnworks).Include(o => o.Stream).Include(o => o.Ebay).ToList();

        //        foreach (var res in listuser)
        //        {
        //            Email = res.Email;

        //            //var userdata = AwsS3.GetS3File("Authorization", _user.Replace("Authorization/", ""));
        //            //var res = JsonConvert.DeserializeObject<IntegrationSettings>(_user);
        //            //var id = 
        //            //  res.Sync = _dbSqlCContext.SyncSettings.Where(x => x.Id == res.SyncId).FirstOrDefault();
        //            if (res.Sync == null)
        //            {
        //                res.Sync = new SyncSettings();
        //            }

        //            //if (res.Sync.SyncEbayOrder)
        //            //{
        //            //    await _ebayController.GetOrders(res.AuthorizationToken, "", res.ebayhour, res.ebaypage);
        //            //}
        //            if (res.Sync.SyncLinnworksOrder)
        //            {
        //                await _linnworksController.GetLinnOrderForStream(res.AuthorizationToken, res.LinnworksSyncToken, "", res.linnhour, res.linnpage);
        //            }
        //            //if (res.Sync.CreateEbayOrderToStream)
        //            //{
        //            //    await _streamController.CreateEbayOrdersToStream(res.AuthorizationToken, "");
        //            //}
        //            if (res.Sync.CreateLinnworksOrderToStream)
        //            {
        //                await _linnworksController.CreateLinnworksOrdersToStream(res.AuthorizationToken, "");
        //            }

        //            //if (res.Sync.DispatchEbayOrderFromStream)
        //            //{
        //            //    await _ebayController.DispatchOrderFromStream(res.AuthorizationToken, "");
        //            //}


        //            res.LastSyncOn = DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss");
        //            res.LastSyncOnDate = DateTime.Now;
        //            //await _configController.Save(res);
        //            _integrationSettingsRepository.Update(res);
        //            _unitOfWork.Commit();


        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        SqlHelper.SystemLogInsert("sync Method", null, null, null, "Sync", !string.IsNullOrEmpty(ex.ToString()) ? ex.ToString().Replace("'", "''") : null, true, Email);
        //        // Log the exception for debugging
        //        Console.WriteLine($"An error occurred: {ex.Message}");
        //        return false; // Indicate failure
        //    }
        //}

        [HttpGet, Route("StartService")]
        [AllowAnonymous]
        public async Task<bool> StartService()
        {
            try
            {
                //    var getdepots = StreamOrderApi.GetDepots("f8a28932bc88473d0724c44c1d4c1fd90845bebe", "RISHV000000000001", false);

                var listuser = _dbSqlCContext.Authorizations.OrderByDescending(x => x.Id).ToList();

                var uniqueEmailUsers = listuser
                    .GroupBy(x => x.Email)
                    .Select(g => g.Last())
                    .ToList();

                foreach (var res in uniqueEmailUsers)
                {
                    if (res.AutoOrderSync)
                    {
                        await _linnworksController.GetLinnOrderForStream(res, "");
                        await _linnworksController.CreateLinnworksOrdersToStream(res.AuthorizationToken, "");
                    }
                    res.UpdatedAt = DateTime.UtcNow;
                    _dbSqlCContext.Update(res);
                    _unitOfWork.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("sync Method", null, null, null, "Sync", !string.IsNullOrEmpty(ex.ToString()) ? ex.ToString().Replace("'", "''") : null, true, "All");
                // Log the exception for debugging
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false; // Indicate failure
            }
        }

        [HttpGet, Route("PluggableStartService")]
        [AllowAnonymous]
        public async Task<bool> PluggableStartService(string Email, string orderIds)
        {
            try
            {
                var user = _dbSqlCContext.Authorizations.Where(u => u.Email == Email).FirstOrDefault();

                await _linnworksController.GetLinnOrderForStream(user, orderIds);
                await _linnworksController.CreateLinnworksOrdersToStream(user.AuthorizationToken, orderIds);

                user.UpdatedAt = DateTime.UtcNow;
                _dbSqlCContext.Update(user);
                _unitOfWork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("sync Method", null, null, null, "Sync", !string.IsNullOrEmpty(ex.ToString()) ? ex.ToString().Replace("'", "''") : null, true, "All");
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
             "*/10 * * * *"  // Every 30 minutes
             );

            return Ok("Recurring job setup complete.");
        }

        private async Task SaveNewIdentifier(LinnworksBaseStream obj, string identifierTag)
        {
            string requestJson = $"identifierTag: {identifierTag}";
            var standardizedIdentifierTag = identifierTag.ToUpper();
            var img = $"https://stream-shipping-integration-file-storage.s3.eu-west-2.amazonaws.com/Images/{identifierTag.Replace(" ", "")}.png";
            await Task.Run(() =>
            {

                obj.Api.OpenOrders.SaveIdentifier(new SaveIdentifiersRequest
                {
                    Identifier = new Identifier
                    {
                        Name = identifierTag.ToUpper(),
                        Tag = identifierTag.ToUpper(),
                        IsCustom = true,
                        ImageId = Guid.NewGuid(),
                        ImageUrl = img
                    }
                });
            });
            SqlHelper.SystemLogInsert("SaveNewIdentifier", null, requestJson, null, "Success", $"New identifier {standardizedIdentifierTag} saved successfully.", false, obj.Api.GetSessionId().ToString());

        }


        [HttpGet("setup-PostalService-job")]
        [AllowAnonymous]
        public IActionResult SetupPostalServiceJob()
        {
            RecurringJob.AddOrUpdate<SyncController>(
             "Order-PostalService",
             x => x.PostalService(),
             "0 6 * * *"  // Every 30 minutes
             );

            return Ok("Recurring job setup complete.");
        }


        [HttpPost, Route("PostalService")]
        [AllowAnonymous]
        public async Task<IActionResult> PostalService()
        {
            try
            {
                var listuser = _dbSqlCContext.Authorizations.ToList();

                var uniqueEmailUsers = listuser
                    .GroupBy(x => x.Email)
                    .Select(g => g.Last())
                    .ToList();


                foreach (var item in uniqueEmailUsers)
                {
                    var obj = new LinnworksBaseStream(item.LinnworksToken);

                    string Runidentifier = "RUN 01,RUN 02,RUN 03,RUN 04,RUN 05,RUN 06,RUN 07,RUN 08";
                    var identifiers = obj.Api.OpenOrders.GetIdentifiers();

                    foreach (var strrun in Runidentifier.Split(','))
                    {

                        if (!identifiers.Any(d => d.Tag == strrun))
                        {
                            // Save the new identifier
                            SqlHelper.SystemLogInsert("UpdateOrderIdentifier", null, item.Email, null, "SaveIdentifier", $"Saving new identifier {strrun}.", false, item.LinnworksToken);
                            await SaveNewIdentifier(obj, strrun);
                        }
                    }

                    string Days = "SUNDAY,MONDAY,TUESDAY,WEDNESDAY,THRUSDAY,FRIDAY,SATURDAY";


                    foreach (var strrun1 in Days.Split(','))
                    {

                        if (!identifiers.Any(d => d.Tag == strrun1))
                        {
                            // Save the new identifier
                            SqlHelper.SystemLogInsert("UpdateOrderIdentifier", null, item.Email, null, "SaveIdentifier", $"Saving new identifier {strrun1}.", false, item.LinnworksToken);
                            await SaveNewIdentifier(obj, strrun1);
                        }
                    }
                }




                // var list_user = await _dbSqlCContext.Authorizations.ToListAsync();  // Use async to avoid blocking
                var list_user = await _dbSqlCContext.Authorizations
     .OrderByDescending(x => x.Id) // or x.CreatedAt
     .ToListAsync();

                foreach (var user in list_user)
                {
                    var obj = new LinnworksBaseStream(user.LinnworksToken);

                    ProxiedWebRequest request = new ProxiedWebRequest
                    {
                        Url = "https://eu-ext.linnworks.net/api/ShippingService/GetIntegrations",
                        Method = "POST"
                    };
                    request.Headers.Add("Authorization", obj.Api.GetSessionId().ToString());

                    var upload = obj.ProxyFactory.WebRequest(request);
                    var data = Encoding.UTF8.GetString(upload.RawResponse);

                    string accountId = user.AccountName;

                    using JsonDocument doc = JsonDocument.Parse(data);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        var result = root.EnumerateArray()
                            .FirstOrDefault(x =>
                                x.TryGetProperty("Vendor", out var vendor) && vendor.GetString().Contains("Stream Shipping") &&
                                x.TryGetProperty("AccountId", out var account) && account.GetString() == accountId
                            );

                        if (result.ValueKind == JsonValueKind.Object &&
                            result.TryGetProperty("pkShippingAPIConfigId", out var pkShippingAPIConfigIdProp) &&
                            pkShippingAPIConfigIdProp.TryGetInt32(out int pkShippingAPIConfigId))
                        {
                            // Save to user and commit
                            user.ShippingApiConfigId = pkShippingAPIConfigId;
                            _dbSqlCContext.Update(user);
                            await _unitOfWork.CommitAsync();

                            // Fetch Postal Services
                            ProxiedWebRequest requestb = new ProxiedWebRequest
                            {
                                Url = "https://eu-ext.linnworks.net/api/ShippingService/GetPostalServices",
                                Method = "POST"
                            };
                            requestb.Headers.Add("Authorization", obj.Api.GetSessionId().ToString());

                            var uploadb = obj.ProxyFactory.WebRequest(requestb);
                            using var doc1 = JsonDocument.Parse(uploadb.RawResponse);
                            string jsonString = Encoding.UTF8.GetString(uploadb.RawResponse);
                            var root1 = doc1.RootElement;

                            if (root1.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in root1.EnumerateArray())
                                {
                                    if (item.TryGetProperty("fkShippingAPIConfigId", out var configIdElement) &&
                                        configIdElement.TryGetInt32(out int configId) &&
                                        configId == pkShippingAPIConfigId)
                                    {
                                        string serviceName = item.TryGetProperty("PostalServiceName", out var nameEl) &&
                                                             nameEl.ValueKind == JsonValueKind.String
                                            ? nameEl.GetString()
                                            : null;

                                        string serviceId = item.TryGetProperty("pkPostalServiceId", out var idEl) &&
                                                           idEl.ValueKind == JsonValueKind.String
                                            ? idEl.GetString()
                                            : null;

                                        var get_service = await _dbSqlCContext.PostalServices
                                            .Where(x => x.PostalServiceId == serviceId &&
                                                        x.AuthorizationToken == user.AuthorizationToken)
                                            .FirstOrDefaultAsync();

                                        if (get_service == null)
                                        {
                                            var postalService = new PostalServices
                                            {
                                                AuthorizationToken = user.AuthorizationToken,
                                                PostalServiceId = serviceId,
                                                PostalServiceName = serviceName,
                                                CreatedAt = DateTime.UtcNow,
                                                UpdatedAt = DateTime.UtcNow
                                            };

                                            await _dbSqlCContext.PostalServices.AddAsync(postalService);
                                        }
                                    }
                                }
                            }

                            await _unitOfWork.CommitAsync();
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1));
                }


                //            foreach (var user in list_user)
                //            {
                //                var obj = new LinnworksBaseStream(user.LinnworksToken);

                //                ProxiedWebRequest request = new ProxiedWebRequest
                //                {
                //                    Url = "https://eu-ext.linnworks.net/api/ShippingService/GetIntegrations",
                //                    Method = "POST"
                //                };
                //                request.Headers.Add("Authorization", obj.Api.GetSessionId().ToString());

                //                // Perform the web request and get the response
                //                var upload = obj.ProxyFactory.WebRequest(request);
                //                var data = Encoding.UTF8.GetString(upload.RawResponse);

                //                string accountId = user.AccountName;

                //                // Parse the JSON
                //                //using JsonDocument doc = JsonDocument.Parse(data);
                //                //var root = doc.RootElement.EnumerateArray();

                //                // Parse the JSON response
                //                using JsonDocument doc = JsonDocument.Parse(data);
                //                var root = doc.RootElement;


                //                if (root.ValueKind == JsonValueKind.Array)
                //                {
                //                    // Process the array if the root is an array
                //                    //var result = root.EnumerateArray()
                //                    //    .FirstOrDefault(x =>
                //                    //        x.TryGetProperty("Vendor", out var vendor) && vendor.GetString() == "Stream Shipping" &&
                //                    //        x.TryGetProperty("AccountId", out var account) && account.GetString() == accountId
                //                    //    );

                //                    var matches = root.EnumerateArray()
                //.Where(x =>
                //    x.TryGetProperty("Vendor", out var vendor) && vendor.GetString() == "Stream Shipping")
                //.ToList(); // Check if this already filters anything

                //                    var result = matches
                //                        .Where(x =>
                //                            x.TryGetProperty("AccountId", out var account) && account.GetString() == accountId)
                //                        .ToList(); // Then check this


                //                    // If no result is found, handle it accordingly
                //                    //if (result.ValueKind != JsonValueKind.Undefined)
                //                    //{
                //                    //    // Safely extract the pkShippingAPIConfigId
                //                    //    if (result.TryGetProperty("pkShippingAPIConfigId", out var pkShippingAPIConfigIdProp))
                //                    //    {
                //                    //        int pkShippingAPIConfigId = pkShippingAPIConfigIdProp.GetInt32(); // GetInt32() is a better fit for an ID.

                //                    //        // Update the Authorization with the ShippingApiConfigId
                //                    //        user.ShippingApiConfigId = pkShippingAPIConfigId;
                //                    //        _dbSqlCContext.Update(user);
                //                    //        await _unitOfWork.CommitAsync();  // Async commit

                //                    //        // Fetch Postal Services
                //                    //        ProxiedWebRequest requestb = new ProxiedWebRequest
                //                    //        {
                //                    //            Url = "https://eu-ext.linnworks.net/api/ShippingService/GetPostalServices",
                //                    //            Method = "POST"
                //                    //        };
                //                    //        requestb.Headers.Add("Authorization", obj.Api.GetSessionId().ToString());

                //                    //        // Perform the web request and get the response
                //                    //        var uploadb = obj.ProxyFactory.WebRequest(requestb);

                //                    //        using var doc1 = JsonDocument.Parse(uploadb.RawResponse);
                //                    //        var root1 = doc1.RootElement;

                //                    //        if (root1.ValueKind == JsonValueKind.Array)
                //                    //        {
                //                    //            foreach (var item in root1.EnumerateArray())
                //                    //            {
                //                    //                if (item.TryGetProperty("fkShippingAPIConfigId", out var configIdElement) &&
                //                    //                    configIdElement.GetInt16() == pkShippingAPIConfigId)
                //                    //                {
                //                    //                    string serviceName = item.TryGetProperty("PostalServiceName", out var nameEl)
                //                    //                        ? nameEl.GetString() : null;

                //                    //                    string serviceId = item.TryGetProperty("pkPostalServiceId", out var idEl)
                //                    //                        ? idEl.GetString() : null;

                //                    //                    var get_service = await _dbSqlCContext.PostalServices
                //                    //                        .Where(x => x.PostalServiceId == serviceId && x.AuthorizationToken == user.AuthorizationToken)
                //                    //                        .FirstOrDefaultAsync();  // Use async query

                //                    //                    if (get_service == null)
                //                    //                    {
                //                    //                        // Create new entity
                //                    //                        var postalService = new PostalServices
                //                    //                        {
                //                    //                            AuthorizationToken = user.AuthorizationToken, // use your existing token variable
                //                    //                            PostalServiceId = serviceId,
                //                    //                            PostalServiceName = serviceName,
                //                    //                            CreatedAt = DateTime.UtcNow,
                //                    //                            UpdatedAt = DateTime.UtcNow
                //                    //                        };

                //                    //                        // Add to DbContext
                //                    //                        await _dbSqlCContext.PostalServices.AddAsync(postalService);  // Async insert
                //                    //                    }
                //                    //                }
                //                    //            }
                //                    //        }

                //                    //        await _unitOfWork.CommitAsync();  // Async commit after all the changes

                //                    //    }
                //                    //    else
                //                    //    {
                //                    //        throw new Exception("Missing 'pkShippingAPIConfigId' property in the response.");
                //                    //    }
                //                    //}
                //                    if (result.ValueKind == JsonValueKind.Object &&
                //result.TryGetProperty("pkShippingAPIConfigId", out var pkShippingAPIConfigIdProp) &&
                //pkShippingAPIConfigIdProp.TryGetInt32(out int pkShippingAPIConfigId))
                //                    {
                //                        user.ShippingApiConfigId = pkShippingAPIConfigId;
                //                        _dbSqlCContext.Update(user);
                //                        await _unitOfWork.CommitAsync();

                //                        ProxiedWebRequest requestb = new ProxiedWebRequest
                //                        {
                //                            Url = "https://eu-ext.linnworks.net/api/ShippingService/GetPostalServices",
                //                            Method = "POST"
                //                        };
                //                        requestb.Headers.Add("Authorization", obj.Api.GetSessionId().ToString());

                //                        var uploadb = obj.ProxyFactory.WebRequest(requestb);

                //                        using var doc1 = JsonDocument.Parse(uploadb.RawResponse);
                //                        var root1 = doc1.RootElement;

                //                        if (root1.ValueKind == JsonValueKind.Array)
                //                        {
                //                            foreach (var item in root1.EnumerateArray())
                //                            {
                //                                if (item.TryGetProperty("fkShippingAPIConfigId", out var configIdElement) &&
                //                                    configIdElement.TryGetInt32(out int configId) &&
                //                                    configId == pkShippingAPIConfigId)
                //                                {
                //                                    string serviceName = item.TryGetProperty("PostalServiceName", out var nameEl) &&
                //                                                         nameEl.ValueKind == JsonValueKind.String
                //                                                         ? nameEl.GetString() : null;

                //                                    string serviceId = item.TryGetProperty("pkPostalServiceId", out var idEl) &&
                //                                                       idEl.ValueKind == JsonValueKind.String
                //                                                       ? idEl.GetString() : null;

                //                                    var get_service = await _dbSqlCContext.PostalServices
                //                                        .Where(x => x.PostalServiceId == serviceId && x.AuthorizationToken == user.AuthorizationToken)
                //                                        .FirstOrDefaultAsync();

                //                                    if (get_service == null)
                //                                    {
                //                                        var postalService = new PostalServices
                //                                        {
                //                                            AuthorizationToken = user.AuthorizationToken,
                //                                            PostalServiceId = serviceId,
                //                                            PostalServiceName = serviceName,
                //                                            CreatedAt = DateTime.UtcNow,
                //                                            UpdatedAt = DateTime.UtcNow
                //                                        };

                //                                        await _dbSqlCContext.PostalServices.AddAsync(postalService);
                //                                    }
                //                                }
                //                            }
                //                        }

                //                        await _unitOfWork.CommitAsync();
                //                    }

                //                }

                //                await Task.Delay(TimeSpan.FromMinutes(1));
                //            }

                return Ok("Postal service processed successfully.");
            }
            catch (Exception ex)
            {
                // Log the error using a better logging mechanism (e.g., ILogger, Serilog)
                SqlHelper.SystemLogInsert("Postal Service Method", null, null, null, "Postal Service", !string.IsNullOrEmpty(ex.ToString()) ? ex.ToString().Replace("'", "''") : null, true, "All");

                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet, Route("StartOtherService")]
        [AllowAnonymous]
        public async Task<bool> StartOtherServices()
        {
            string Email = "";
            try
            {
                //var listuser = await AwsS3.ListFilesInS3Folder("Authorization/Users");
                // var listuser = _integrationSettingsRepository.Get().ToList();
                //var listuser = _dbSqlCContext.IntegrationSettings.ToList();

                var listuser = _dbSqlCContext.IntegrationSettings.Include(o => o.Sync).Include(o => o.Linnworks).Include(o => o.Stream).Include(o => o.Ebay).ToList();

                foreach (var res in listuser)
                {
                    Email = res.Email;

                    //var userdata = AwsS3.GetS3File("Authorization", _user.Replace("Authorization/", ""));
                    //var res = JsonConvert.DeserializeObject<IntegrationSettings>(_user);
                    //var id = 
                    //  res.Sync = _dbSqlCContext.SyncSettings.Where(x => x.Id == res.SyncId).FirstOrDefault();
                    if (res.Sync == null)
                    {
                        res.Sync = new SyncSettings();
                    }


                    if (res.Sync.UpdateLinnworksOrderToStream)
                    {
                        await _linnworksController.UpdateLinnworksOrdersToStream(res.AuthorizationToken, res.LinnworksSyncToken, "");
                    }
                    if (res.Sync.DispatchLinnworksOrderFromStream)
                    {
                        await _linnworksController.DispatchLinnworksOrdersFromStream(res.AuthorizationToken, "", res.LinnworksSyncToken);
                    }

                    res.LastSyncOn = DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss");
                    res.LastSyncOnDate = DateTime.Now;
                    _integrationSettingsRepository.Update(res);
                    _unitOfWork.Commit();


                }

                return true;
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("UpdateOrder", null, null, null, "Sync", !string.IsNullOrEmpty(ex.ToString()) ? ex.ToString().Replace("'", "''") : null, true, Email);
                // Log the exception for debugging
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false; // Indicate failure
            }
        }

        [HttpGet("setup-Other-recurring-job")]
        [AllowAnonymous]
        public IActionResult SetupOtherRecurringJob()
        {
            RecurringJob.AddOrUpdate<SyncController>(
     "Order-update-dispatch-linnworks-stream",
     x => x.StartService(),
    "0 * * * *"  // Every 30 minutes
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
