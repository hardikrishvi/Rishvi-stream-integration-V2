﻿using Hangfire;
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
using YamlDotNet.Core.Tokens;
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
                        await _linnworksController.GetLinnOrderForStream(AuthorizationToken, LinnworksSyncToken, value.orderids, 500, 10);
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
        [HttpGet, Route("StartService")]
        [AllowAnonymous]
        public async Task<bool> StartService()
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
              
                    //if (res.Sync.DispatchEbayOrderFromStream)
                    //{
                    //    await _ebayController.DispatchOrderFromStream(res.AuthorizationToken, "");
                    //}
                  

                    res.LastSyncOn = DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss");
                    res.LastSyncOnDate = DateTime.Now;
                    //await _configController.Save(res);
                    _integrationSettingsRepository.Update(res);
                    _unitOfWork.Commit();

                   
                }

                return true;
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("sync Method", null, null, null, "Sync", !string.IsNullOrEmpty(ex.ToString()) ? ex.ToString().Replace("'", "''") : null, true, Email);
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
