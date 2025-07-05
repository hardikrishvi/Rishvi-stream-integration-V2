using LinnworksAPI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using System.Text.RegularExpressions;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Core.Helper;
using static Rishvi.Modules.ShippingIntegrations.Core.Helper.ServiceHelper;
using Rishvi.Domain.DTOs.Webhook;
using Rishvi.Domain.DTOs.Event;
using Rishvi.Domain.DTOs.Run;
using Rishvi.Domain.DTOs.Subscription;
using Rishvi.Models;
using Rishvi.Modules.Core.Data;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.Core.Helpers;

namespace Rishvi.Modules.ShippingIntegrations.Api
{

    [Route("api/Linnworks")]
    public class LinnworksController : ControllerBase
    {
        private readonly TradingApiOAuthHelper _tradingApiOAuthHelper;
        private readonly ConfigController _configController;
        private readonly StreamController _streamController;
        private readonly IServiceHelper _serviceHelper;
        private readonly ReportsController _reportsController;
        private readonly IAuthorizationToken _authToken;
        private readonly IRepository<Subscription> _subscription;
        private readonly IRepository<WebhookOrder> _webhookOrder;
        private readonly IRepository<Run> _run;
        private readonly IRepository<Event> _event;
        private readonly IUnitOfWork _unitOfWork;
        private readonly SqlContext _dbSqlCContext;
        public LinnworksController(TradingApiOAuthHelper tradingApiOAuthHelper, IAuthorizationToken authToken,
            ConfigController configController, StreamController streamController,
            IServiceHelper serviceHelper, ReportsController reportsController, IRepository<Subscription> subscription, IUnitOfWork unitOfWork,
            IRepository<WebhookOrder> webhookOrder, IRepository<Run> run, IRepository<Event> event_repo, SqlContext dbSqlCContext)
        {
            _tradingApiOAuthHelper = tradingApiOAuthHelper;
            _configController = configController;
            _streamController = streamController;
            _serviceHelper = serviceHelper;
            _reportsController = reportsController;
            _authToken = authToken;
            _subscription = subscription;
            _unitOfWork = unitOfWork;
            _webhookOrder = webhookOrder;
            _run = run;
            _event = event_repo;
            _dbSqlCContext = dbSqlCContext;
        }

        [HttpPost, Route("GetLinnOrderForStream")]
        public async Task<IActionResult> GetLinnOrderForStream(string token, string linntoken, string orderids, int linnhour, int linnpage)
        {
            try
            {
                var user = _authToken.Load(token);
                linntoken = string.IsNullOrEmpty(linntoken) ? user.LinnworksToken : linntoken;

                if (String.IsNullOrEmpty(linntoken))
                {
                    return BadRequest("Linnworks token is missing.");
                }

                var obj = new LinnworksBaseStream(linntoken);


                if (!String.IsNullOrEmpty(orderids))
                {
                    var orderlist = Regex.Split(orderids, ",");

                    foreach (var linnorderid in orderlist)
                    {
                        var orderdata = obj.Api.Orders.GetOrderDetailsByNumOrderId(Convert.ToInt32(linnorderid));
                        var newjson = JsonConvert.SerializeObject(orderdata);
                        await _tradingApiOAuthHelper.SaveLinnOrder(newjson, token, user.Email, linnorderid.ToString());
                    }
                }
                else
                {
                    var filters = _serviceHelper.CreateFilters(linnhour);

                    var allorder = obj.Api.Orders.GetAllOpenOrders(filters, null, Guid.Empty, "");
                    var allorderdetails = obj.Api.Orders.GetOrders(allorder.Skip(0).Take(linnpage).ToList(), Guid.Empty, true, true);
                    foreach (var _order in allorderdetails)
                    {
                        var fileName = $"LinnOrder/{token}_linnorder_{_order.NumOrderId}.json";
                        if (!await AwsS3.S3FileIsExists("Authorization", fileName))
                        {
                            var newjson = JsonConvert.SerializeObject(_order);
                            await _tradingApiOAuthHelper.SaveLinnOrder(newjson, token, user.Email, _order.NumOrderId.ToString());
                        }
                    }
                }
                return Ok("Orders processed successfully.");
            }
            catch (Exception ex)
            {
                // Log the error (use ILogger for production)
                Console.WriteLine($"Error while processing Linn orders: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }

        }

        [HttpGet, Route("CreateLinnworksOrdersToStream")]
        public async Task<IActionResult> CreateLinnworksOrdersToStream(string token, string orderids)
        {
            try
            {
                // Load user configuration
                var user = _authToken.Load(token);
                if (string.IsNullOrEmpty(user?.ClientId))
                {
                    return BadRequest("Invalid client ID.");
                }

                if (string.IsNullOrEmpty(orderids))
                {
                    // Get pending orders if no order IDs are provided
                    var reportData = await _reportsController.GetReportData(new ReportModelReq { email = user.Email });

                    var pendingOrders = reportData.Where(f => !f.IsLinnOrderCreatedInStream && !string.IsNullOrEmpty(f.LinnNumOrderId));
                    foreach (var pendingOrder in pendingOrders)
                    {
                        await _tradingApiOAuthHelper.CreateLinnworksOrdersToStream(user, pendingOrder.LinnNumOrderId.ToString());
                    }
                }
                else
                {
                    // Process provided order IDs
                    var orderidlist = Regex.Split(orderids, ",");
                    foreach (var _ord in orderidlist)
                    {
                        await _tradingApiOAuthHelper.CreateLinnworksOrdersToStream(user, _ord);
                    }
                }
                return Ok("Linnworks orders successfully created.");
            }
            catch (Exception ex)
            {
                // Log exception (replace Console.WriteLine with ILogger for production)
                Console.WriteLine($"Error creating Linnworks orders: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }

        }

        [HttpGet, Route("UpdateLinnworksOrdersToStream")]
        public async Task<IActionResult> UpdateLinnworksOrdersToStream(string token, string orderids)
        {
            try
            {
                // Load user configuration
                var user = _authToken.Load(token);
                if (string.IsNullOrEmpty(user?.ClientId))
                {
                    return BadRequest("Invalid client ID.");
                }
                if (string.IsNullOrEmpty(orderids))
                {
                    // Get pending orders if no order IDs are provided
                    var reportData = _dbSqlCContext.ReportModel
                        .Where(x => x.email == user.Email)
                        .ToList();
                    
                    var pendingOrders = reportData.Where(f => f.IsLinnOrderCreatedInStream && !f.IsLinnOrderDispatchFromStream && !string.IsNullOrEmpty(f.LinnNumOrderId));
                    foreach (var pendingOrder in pendingOrders)
                    {
                        await _tradingApiOAuthHelper.UpdateLinnworksOrdersToStream(user, pendingOrder.LinnNumOrderId.ToString(), pendingOrder.StreamOrderId);
                    }
                }
                else
                {
                    
                    var orderidlist = Regex.Split(orderids, ",");
                    
                    var reportData = await _dbSqlCContext.ReportModel
                        .Where(f => 
                            f.IsLinnOrderCreatedInStream && 
                            orderidlist.Contains(f.LinnNumOrderId))
                        .ToListAsync();
                    
                    foreach (var _ord in orderidlist)
                    {
                        
                        
                        var linnOrders = reportData
                            .FirstOrDefault(f => f.LinnNumOrderId == _ord);
                        if (linnOrders != null)
                        {
                            await _tradingApiOAuthHelper.UpdateLinnworksOrdersToStream(user, _ord, linnOrders.StreamOrderId);
                        }
                    }
                }
                return Ok("Linnworks orders successfully created.");
            }
            catch (Exception ex)
            {
                // Log exception (replace Console.WriteLine with ILogger for production)
                Console.WriteLine($"Error creating Linnworks orders: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }

        }

        [HttpGet(), Route("DispatchLinnworksOrdersFromStream")]
        public async Task<IActionResult> DispatchLinnworksOrdersFromStream(string token, string orderids, string linntoken)
        {
            try
            {
                // Load user configuration
                AuthorizationConfigClass user = _authToken.Load(token);
                if (string.IsNullOrEmpty(user?.ClientId))
                {
                    return BadRequest("Invalid or missing client ID.");
                }

                if (string.IsNullOrEmpty(orderids))
                {
                    // Process orders from report if no order IDs are provided
                    //var reportData = await _reportsController.GetReportData(new ReportModelReq { email = user.Email });
                    var reportData = _dbSqlCContext.ReportModel
                        .Where(x => x.email == user.Email)
                        .ToList();
                    var dispatchlinnfromstream = reportData.Where(f => !f.IsLinnOrderDispatchFromStream && !string.IsNullOrEmpty(f.LinnNumOrderId));

                    foreach (var _linnord in dispatchlinnfromstream)
                    {
                        await DispatchOrderInner(user, _linnord.LinnNumOrderId, linntoken, token);
                    }

                }
                else
                {
                    // Process provided order IDs
                    var orderidlist = Regex.Split(orderids, ",");
                    foreach (var _ord in orderidlist)
                    {
                        await DispatchOrderInner(user, _ord, linntoken, token);
                    }
                }
                return Ok("Orders dispatched successfully.");
            }
            catch (Exception ex)
            {
                // Log the error (replace Console.WriteLine with ILogger for production)
                Console.WriteLine($"Error dispatching Linnworks orders: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        private async Task DispatchOrderInner(AuthorizationConfigClass user, string orderId, string linntoken, string token)
        {
            //var linnDispatchPath = $"LinnDispatch/{token}_linndispatch_{orderId}.json";
            var linnStreamPath = $"LinnStreamOrder/_streamorder_{orderId}.json";
            var streamOrderExists = await _dbSqlCContext.StreamOrderRecord
                .AnyAsync(x => x.LinnworksOrderId == orderId);
            
            var dispatchDone = await _dbSqlCContext.ReportModel
                .Where(x => x.LinnNumOrderId == orderId)
                .Select(x => x.IsLinnOrderDispatchFromStream)
                .FirstOrDefaultAsync();

            if (!dispatchDone  && streamOrderExists)
            {
                var jsonData = AwsS3.GetS3File("Authorization", linnStreamPath);
                var streamData = JsonConvert.DeserializeObject<StreamOrderRespModel.Root>(jsonData);

                if (streamData != null && orderId.IsValidInt32())
                {
                    await _tradingApiOAuthHelper.DispatchOrderInLinnworks(
                        user,
                        orderId.ToInt32(),
                        linntoken,
                        "Stream",
                        streamData.response.trackingId,
                        streamData.response.trackingURL,
                        null
                    );
                }
            }
        }

        [HttpGet, Route("DispatchOrder")]
        public async Task<IActionResult> DispatchOrder([FromQuery] OrderDispatchReq value)
        {
            try
            {
                // Validate the token and user configuration
                var user = _authToken.Load(value.token);
                if (string.IsNullOrEmpty(user?.access_token))
                {
                    return BadRequest("Invalid or missing access token.");
                }

                // Validate the order reference
                if (!value.IsValidInt32())
                {
                    return BadRequest("Invalid order reference.");
                }

                // Perform the dispatch action
                await _tradingApiOAuthHelper.DispatchOrderInLinnworks(
                    user,
                    Convert.ToInt32(value.orderref),
                    value.itemid,
                    value.service,
                    value.trackingnumber,
                    value.trackingurl,
                    null
                );

                return Ok("Order dispatched successfully.");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error dispatching order: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while dispatching the order.");
            }
        }

        [NonAction]
        public async Task UpdateOrderIdentifier(string linntoken, int orderid, string identifier)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(linntoken))
                {
                    throw new ArgumentException("Invalid Linnworks token.");
                }
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    throw new ArgumentException("Identifier cannot be null or empty.");
                }

                // Initialize Linnworks API object
                var obj = new LinnworksBaseStream(linntoken);

                // Fetch the Linnworks order details
                var linnOrderDetails = obj.Api.Orders.GetOrderDetailsByNumOrderId(orderid);
                if (linnOrderDetails == null)
                {
                    throw new Exception($"Order with ID {orderid} not found.");
                }

                // Standardize identifier tag
                var identifierTag = identifier.ToUpper();

                // Check if the identifier already exists
                var identifiers = obj.Api.OpenOrders.GetIdentifiers();
                if (!identifiers.Any(d => d.Tag == identifierTag))
                {
                    // Save the new identifier
                    await SaveNewIdentifier(obj, identifierTag);
                }

                // Assign the identifier to the order
                obj.Api.OpenOrders.AssignOrderIdentifier(new ChangeOrderIdentifierRequest
                {
                    OrderIds = new[] { linnOrderDetails.OrderId },
                    Tag = identifierTag
                });
            }
            catch (Exception ex)
            {
                var webhookOrder1 = new WebhookOrder
                {
                    sequence = 1,
                    order = $"Token:{ex.ToString()}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _webhookOrder.Add(webhookOrder1);
                _unitOfWork.Context.SaveChanges();
                // Log the error (replace with ILogger for production)
                Console.WriteLine($"Error updating order identifier: {ex.Message}");
                throw; // Re-throw the exception for further handling if needed
            }
        }
        private async Task SaveNewIdentifier(LinnworksBaseStream obj, string identifierTag)
        {
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
                        ImageUrl = $"https://stream-api-stg.rishvi.app/{identifierTag}.png"
                    }
                });
            });
        }

        [HttpGet, Route("CreateIdentifier")]
        public async Task<IActionResult> CreateIdentifier(string linntoken, string identifier)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(linntoken))
                {
                    return BadRequest("Invalid Linnworks token.");
                }

                if (string.IsNullOrWhiteSpace(identifier))
                {
                    return BadRequest("Identifier cannot be null or empty.");
                }
                // Handle "days" case
                if (identifier.ToLower() == "days")
                {
                    var daysOfWeek = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                    foreach (var day in daysOfWeek)
                    {
                        await _serviceHelper.ManageIdentifier(linntoken, day);
                    }
                }
                else
                {
                    // Handle single identifier
                    await _serviceHelper.ManageIdentifier(linntoken, identifier);
                }
                return Ok("Identifier(s) processed successfully.");

            }
            catch (Exception ex)
            {
                // Log the error (replace Console.WriteLine with ILogger in production)
                Console.WriteLine($"Error creating identifier: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet, Route("SubscribeWebhook")]
        public async Task<IActionResult> SubscribeWebhook(string token)
        {
            try
            {
                // Validate token
                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest("Token is required.");
                }

                var user = _authToken.Load(token);
               
                var streamAuth = _manageToken.GetToken(user);
                if (string.IsNullOrWhiteSpace(streamAuth?.AccessToken))
                {
                    return BadRequest("Invalid or missing access token.");
                }

                // Define webhooks
                var webhooks = new List<WebhookSubscription>
                {
                    new("ORDERSTATUS", "CONSHEADER", "https://stream-api-stg.rishvi.app/api/Linnworks/webhook"),
                    new("ITEMSTATUS", "CONSITEM", "https://stream-api-stg.rishvi.app/api/Linnworks/webhook"),
                    new("GROUPSTATUS", "LOADALLOC", "https://stream-api-stg.rishvi.app/api/Linnworks/webhook"),
                    new("LOADALLOCPUP", "CONSHEADER", "https://stream-api-stg.rishvi.app/api/Linnworks/webhook"),
                    new("GROUPSTATUS", "LOADALLOCTRUNK", "https://stream-api-stg.rishvi.app/Linnworks/webhook"),
                    new("RUNSTATUS", "LOADHEADER", "https://stream-api-stg.rishvi.app/api/Linnworks/webhook")
                };
                // delete all webhook

                // Create webhooks
                foreach (var webhook in webhooks)
                {
                    await _serviceHelper.CreateWebhook(user, webhook, token);
                }

                return Ok("Webhooks subscribed successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (replace with ILogger for production)
                Console.WriteLine($"Error subscribing to webhooks: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [Route("webhook")]
        public async Task<ActionResult<Dictionary<string, string>>> webhook()
        {
            string data = (await new StreamReader(Request.Body).ReadToEndAsync()).ToString().Trim();
            var query = new Dictionary<string, string>();
            foreach (var sItem in Request.Query.Keys)
            {
                query.Add(sItem, Request.Query[sItem].ToString());
            }
            var output = JsonConvert.DeserializeObject<WebhookResponse.Root>(data);
            await _tradingApiOAuthHelper.SaveWebhook(data, output.webhook.subscription.party_id, DateTime.Now.ToString("ddMMyyyyhhmmss"));
            //SqlHelper.SystemLogInsert("Webhook_riddhi", "", JsonConvert.SerializeObject(output).Replace("'", "''"), "", "Webhook", "", false);

            var subscription = new Subscription()
            {
                party_id = output.webhook.subscription.party_id,
                @event = output.webhook.subscription.@event,
                event_type = output.webhook.subscription.event_type,
                url_path = output.webhook.subscription.url_path,
                http_method = output.webhook.subscription.http_method,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };
            var @event = new Event
            {
                event_code = output.webhook.@event.event_code,
                event_code_desc = output.webhook.@event.event_code_desc,
                event_desc = output.webhook.@event.event_desc,
                event_date = output.webhook.@event.event_date,
                event_time = output.webhook.@event.event_time,
                event_text = output.webhook.@event.event_text,
                event_link = output.webhook.@event.event_link,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };
            var run = new Run
            {
                loadId = output.webhook.run.loadId,
                status = output.webhook.run.status,
                description = output.webhook.run.description,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };
            _subscription.Add(subscription);
            _event.Add(@event);
            _run.Add(run);
            foreach (var order in output.webhook.orders)
            {
                var webhookOrder = new WebhookOrder
                {
                    sequence = order.sequence,
                    order = order.order + "_H_" + output.webhook.@event.event_code,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _webhookOrder.Add(webhookOrder);
            }

            _unitOfWork.Context.SaveChanges();
            //EmailHelper.SendEmail("Req Json", data);
            try
            {


                if (output.webhook.@event.event_code == "LOCKPLANNING" ||
                    output.webhook.@event.event_code == "PLANNEDDELIVERY" ||
                    output.webhook.@event.event_code == "PLANNEDGROUP")
                {
                    var webhookOrder = new WebhookOrder
                    {
                        sequence = 0,
                        order = "Add Event Code",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _webhookOrder.Add(webhookOrder);
                    _unitOfWork.Context.SaveChanges();
                    if (output.webhook.orders != null)
                    {
                        foreach (var strorder in output.webhook.orders)
                        {
                            // need to update on linn order
                            string Stream_runloadid = output.webhook.run.loadId;
                            string Stream_runstatus = output.webhook.run.status;
                            string Stream_rundescription = output.webhook.run.description;
                            string Stream_orderid = strorder.order;
                            // call order api to get driver detail or driver detail
                            string json = AwsS3.GetS3File("Authorization", "StreamParty/" + output.webhook.subscription.party_id + ".json");
                            var user = JsonConvert.DeserializeObject<AuthorizationConfigClass>(json);
                            if (user == null)
                            {
                                var webhookOrder2 = new WebhookOrder
                                {
                                    sequence = 0,
                                    order = "User is null",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };
                                _webhookOrder.Add(webhookOrder2);
                                _unitOfWork.Context.SaveChanges();
                            }

                            var logindata = await _configController.Get(user.Email);
                            var strorderdaat = await _streamController.GetStreamOrder(user.AuthorizationToken, Stream_orderid);

                            if (strorderdaat != null)
                            {
                                if (strorderdaat.response.valid)
                                {
                                    foreach (var gr in strorderdaat.response.order.groups)
                                    {
                                        string Stream_trackingURL = strorderdaat.response.order.trackingURL;
                                        string Stream_trackingId = strorderdaat.response.order.trackingId;
                                        string Stream_driverName = gr.runDetails.driverName;
                                        string Stream_driver = gr.runDetails.driver;
                                        string Stream_vehicle = gr.runDetails.vehicle;
                                        string Stream_vehicleName = gr.runDetails.vehicleName;
                                        string Stream_status = gr.status;
                                        string Stream_driverNotes = gr.driverNotes;
                                        string Stream_estimateArrivalDateTime = gr.estimateArrivalDateTime != "0" ? gr.estimateArrivalDateTime : gr.planned.fromDateTime.Replace("T00-01Z", "");
                                        string Stream_vehicleType = gr.runDetails.vehicleType;
                                        string Stream_dispatched = gr.runDetails.dispatched ? "Yes" : "No";
                                        string Stream_departed = gr.runDetails.departed ? "Yes" : "No";
                                        string Stream_completed = gr.runDetails.completed ? "Yes" : "No";
                                        string Stream_startActualDateTime = gr.runDetails.start.actualDateTime;
                                        string Stream_startPlannedDateTime = gr.runDetails.start.plannedDateTime;
                                        string Stream_endActualDateTime = gr.runDetails.end.actualDateTime;
                                        string Stream_endPlannedDateTime = gr.runDetails.end.plannedDateTime;
                                        string linnworksorderid = strorderdaat.response.order.header.orderNo;

                                        string LinnworksSyncToken = "";
                                        string Email = "";

                                        if (logindata is OkObjectResult okResult)
                                        {
                                            var userData = okResult.Value as RegistrationData;
                                            LinnworksSyncToken = userData.LinnworksSyncToken;
                                            Email = userData.Email;
                                        }
                                        var webhookOrder1 = new WebhookOrder
                                        {
                                            sequence = 1,
                                            order = $"Token:{LinnworksSyncToken}",
                                            CreatedAt = DateTime.Now,
                                            UpdatedAt = DateTime.Now
                                        };
                                        _webhookOrder.Add(webhookOrder1);
                                        _unitOfWork.Context.SaveChanges();

                                        // update on linnworks
                                        if (linnworksorderid.IsValidInt32() && !String.IsNullOrEmpty(LinnworksSyncToken))
                                        {
                                            await _tradingApiOAuthHelper.UpdateOrderExProperty(LinnworksSyncToken, Convert.ToInt32(linnworksorderid), new Dictionary<string, string>() {
                                    {"Stream_runloadid",Stream_runloadid },
                                    {"Stream_runstatus",Stream_runstatus },
                                    {"Stream_rundescription",Stream_rundescription },
                                    {"Stream_orderid",Stream_orderid },
                                    {"Stream_driverName",Stream_driverName },
                                    {"Stream_driver",Stream_driver},
                                    {"Stream_vehicle",Stream_vehicle },
                                    {"Stream_vehicleName",Stream_vehicleName },
                                    {"Stream_status",Stream_status },
                                    {"Stream_driverNotes",Stream_driverNotes },
                                    {"Stream_estimateArrivalDateTime",Stream_estimateArrivalDateTime },
                                    {"Stream_vehicleType",Stream_vehicleType },
                                    {"Stream_dispatched",Stream_dispatched },
                                    {"Stream_departed",Stream_departed },
                                    {"Stream_completed",Stream_completed},
                                    {"Stream_startActualDateTime", Stream_startActualDateTime},
                                    {"Stream_startPlannedDateTime", Stream_startPlannedDateTime},
                                    {"Stream_endActualDateTime",Stream_endActualDateTime },
                                    { "Stream_endPlannedDateTime",Stream_endPlannedDateTime }
                                });
                                            if (gr.estimateArrivalDateTime != "0")
                                            {
                                                await UpdateOrderIdentifier(LinnworksSyncToken, Convert.ToInt32(linnworksorderid),
                                                DateTime.Parse(gr.estimateArrivalDateTime.Replace("-00Z", ":00Z"), null, System.Globalization.DateTimeStyles.RoundtripKind).DayOfWeek.ToString());
                                                //await _tradingApiOAuthHelper.DispatchOrderInLinnworks(user, Convert.ToInt32(linnworksorderid), LinnworksSyncToken, "Stream", Stream_trackingId, Stream_trackingURL, gr.estimateArrivalDateTime.Replace("-00Z", ":00Z"));

                                            }
                                            else
                                            {
                                                await UpdateOrderIdentifier(LinnworksSyncToken, Convert.ToInt32(linnworksorderid),
                                           DateTime.Parse(Regex.Split(gr.planned.fromDateTime, "T")[0], null, System.Globalization.DateTimeStyles.RoundtripKind).DayOfWeek.ToString());
                                                //await _tradingApiOAuthHelper.DispatchOrderInLinnworks(user, Convert.ToInt32(linnworksorderid), LinnworksSyncToken, "Stream", Stream_trackingId, Stream_trackingURL, Regex.Split(gr.planned.fromDateTime, "T")[0]);

                                            }
                                            var alldata = _reportsController.GetReportData(new ReportModelReq() { email = Email }).Result;
                                            if (alldata.Count(d => d.LinnNumOrderId == linnworksorderid && d.IsLinnOrderCreatedInStream == true && d.IsLinnOrderDispatchInStream == false) > 0)
                                            {
                                                alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).IsLinnOrderDispatchInStream = true;
                                                alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).DispatchLinnOrderInStream = DateTime.Now;
                                                alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).updatedDate = DateTime.Now;
                                                await _tradingApiOAuthHelper.SaveReportData(JsonConvert.SerializeObject(alldata), Email);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                EmailHelper.SendEmail("Error Webhook Update","User Authentication Issue - "+ user.AuthorizationToken+" Data:"+ data);
                            }


                        }
                    }
                    else
                    {
                        string Stream_runloadid = output.webhook.run.loadId;
                        string Stream_runstatus = output.webhook.run.status;
                        string Stream_rundescription = output.webhook.run.description;
                        string Stream_orderid = output.webhook.order.id;
                        // call order api to get driver detail or driver detail
                        string json = AwsS3.GetS3File("Authorization", "StreamParty/" + output.webhook.subscription.party_id + ".json");
                        var user = JsonConvert.DeserializeObject<AuthorizationConfigClass>(json);
                        var logindata = await _configController.Get(user.Email);
                        var strorderdaat = await _streamController.GetStreamOrder(user.AuthorizationToken, Stream_orderid);
                        if (strorderdaat.response.valid)
                        {
                            foreach (var gr in strorderdaat.response.order.groups)
                            {
                                string Stream_trackingURL = strorderdaat.response.order.trackingURL;
                                string Stream_trackingId = strorderdaat.response.order.trackingId;
                                string Stream_driverName = gr.runDetails.driverName;
                                string Stream_driver = gr.runDetails.driver;
                                string Stream_vehicle = gr.runDetails.vehicle;
                                string Stream_vehicleName = gr.runDetails.vehicleName;
                                string Stream_status = gr.status;
                                string Stream_driverNotes = gr.driverNotes;
                                string Stream_estimateArrivalDateTime = gr.estimateArrivalDateTime != "0" ? gr.estimateArrivalDateTime : gr.planned.fromDateTime.Replace("T00-01Z", "");
                                string Stream_vehicleType = gr.runDetails.vehicleType;
                                string Stream_dispatched = gr.runDetails.dispatched ? "Yes" : "No";
                                string Stream_departed = gr.runDetails.departed ? "Yes" : "No";
                                string Stream_completed = gr.runDetails.completed ? "Yes" : "No";
                                string Stream_startActualDateTime = gr.runDetails.start.actualDateTime;
                                string Stream_startPlannedDateTime = gr.runDetails.start.plannedDateTime;
                                string Stream_endActualDateTime = gr.runDetails.end.actualDateTime;
                                string Stream_endPlannedDateTime = gr.runDetails.end.plannedDateTime;
                                string linnworksorderid = strorderdaat.response.order.header.orderNo;

                                string LinnworksSyncToken = "";
                                string Email = "";

                                if (logindata is OkObjectResult okResult)
                                {
                                    var userData = okResult.Value as RegistrationData;
                                    LinnworksSyncToken = userData.LinnworksSyncToken;
                                    Email = userData.Email;
                                }
                                // update on linnworks
                                if (linnworksorderid.IsValidInt32() && !String.IsNullOrEmpty(LinnworksSyncToken))
                                {
                                    await _tradingApiOAuthHelper.UpdateOrderExProperty(LinnworksSyncToken, Convert.ToInt32(linnworksorderid), new Dictionary<string, string>() {
                                {"Stream_runloadid",Stream_runloadid },
                                {"Stream_runstatus",Stream_runstatus },
                                {"Stream_rundescription",Stream_rundescription },
                                {"Stream_orderid",Stream_orderid },
                                {"Stream_driverName",Stream_driverName },
                                {"Stream_driver",Stream_driver},
                                {"Stream_vehicle",Stream_vehicle },
                                {"Stream_vehicleName",Stream_vehicleName },
                                {"Stream_status",Stream_status },
                                {"Stream_driverNotes",Stream_driverNotes },
                                {"Stream_estimateArrivalDateTime",Stream_estimateArrivalDateTime },
                                {"Stream_vehicleType",Stream_vehicleType },
                                {"Stream_dispatched",Stream_dispatched },
                                {"Stream_departed",Stream_departed },
                                {"Stream_completed",Stream_completed},
                                {"Stream_startActualDateTime", Stream_startActualDateTime},
                                {"Stream_startPlannedDateTime", Stream_startPlannedDateTime},
                                {"Stream_endActualDateTime",Stream_endActualDateTime },
                                { "Stream_endPlannedDateTime",Stream_endPlannedDateTime }
                            });
                                    if (gr.estimateArrivalDateTime != "0")
                                    {
                                        await UpdateOrderIdentifier(LinnworksSyncToken, Convert.ToInt32(linnworksorderid),
                                        DateTime.Parse(gr.estimateArrivalDateTime.Replace("-00Z", ":00Z"), null, System.Globalization.DateTimeStyles.RoundtripKind).DayOfWeek.ToString());
                                        //await _tradingApiOAuthHelper.DispatchOrderInLinnworks(user, Convert.ToInt32(linnworksorderid), LinnworksSyncToken, "Stream", Stream_trackingId, Stream_trackingURL, gr.estimateArrivalDateTime.Replace("-00Z", ":00Z"));

                                    }
                                    else
                                    {
                                        await UpdateOrderIdentifier(LinnworksSyncToken, Convert.ToInt32(linnworksorderid),
                                        DateTime.Parse(Regex.Split(gr.planned.fromDateTime, "T")[0], null, System.Globalization.DateTimeStyles.RoundtripKind).DayOfWeek.ToString());
                                        //await _tradingApiOAuthHelper.DispatchOrderInLinnworks(user, Convert.ToInt32(linnworksorderid), LinnworksSyncToken, "Stream", Stream_trackingId, Stream_trackingURL, Regex.Split(gr.planned.fromDateTime, "T")[0]);

                                    }
                                    var alldata = _reportsController.GetReportData(new ReportModelReq() { email = Email }).Result;
                                    if (alldata.Count(d => d.LinnNumOrderId == linnworksorderid && d.IsLinnOrderCreatedInStream == true && d.IsLinnOrderDispatchInStream == false) > 0)
                                    {
                                        alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).IsLinnOrderDispatchInStream = true;
                                        alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).DispatchLinnOrderInStream = DateTime.Now;
                                        alldata.FirstOrDefault(f => f.LinnNumOrderId == linnworksorderid).updatedDate = DateTime.Now;
                                        await _tradingApiOAuthHelper.SaveReportData(JsonConvert.SerializeObject(alldata), Email);
                                    }
                                }
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {

                //  EmailHelper.SendEmail("Error Json", data + " Data error " + ex.Message);
            }
            return new Dictionary<string, string>();
        }

    }
}
