using Azure.Core;
using LinnworksAPI;
using LinnworksMacroHelpers.Classes;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rishvi.Domain.DTOs.Event;
using Rishvi.Domain.DTOs.Run;
using Rishvi.Domain.DTOs.Subscription;
using Rishvi.Domain.DTOs.Webhook;
using Rishvi.Models;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Core.Helper;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Rishvi.Modules.ShippingIntegrations.Core.Helper.ServiceHelper;

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
        private readonly ManageToken _managetoken;
        private readonly IRepository<Rishvi.Models.Authorization> _authorization;
        public LinnworksController(TradingApiOAuthHelper tradingApiOAuthHelper, IAuthorizationToken authToken,
            ConfigController configController, StreamController streamController,
            IServiceHelper serviceHelper, ReportsController reportsController, IRepository<Subscription> subscription, IUnitOfWork unitOfWork,
            IRepository<WebhookOrder> webhookOrder, IRepository<Run> run, IRepository<Event> event_repo, SqlContext dbSqlCContext, ManageToken managetoken,
            IRepository<Authorization> authorization)
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
            _managetoken = managetoken;
            _authorization = authorization;
        }

        [HttpPost, Route("GetLinnOrderForStream")]
        public async Task<IActionResult> GetLinnOrderForStream(Authorization res, string orderids)
        {
            try
            {
                
                //var user = _authToken.Load(token);
                string requestJson = JsonConvert.SerializeObject(res);
                var linntoken = res.LinnworksToken;

                if (String.IsNullOrEmpty(linntoken))
                {
                    string errorMessage = "Linnworks token is missing.";
                    SqlHelper.SystemLogInsert("GetLinnOrderForStream", null, requestJson, null, "TokenMissing", errorMessage, true, res.AuthorizationToken);
                    return BadRequest(errorMessage);
                }

                var obj = new LinnworksBaseStream(linntoken);

                var email = obj.authorized.Email;

                if (!String.IsNullOrEmpty(orderids))
                {
                    var orderlist = Regex.Split(orderids, ",");

                    foreach (var linnorderid in orderlist)
                    {
                        var orderdata = obj.Api.Orders.GetOrderDetailsByNumOrderId(Convert.ToInt32(linnorderid));
                        var newjson = JsonConvert.SerializeObject(orderdata);
                        await _tradingApiOAuthHelper.SaveLinnOrder(newjson, res.AuthorizationToken, res.Email, linntoken, linnorderid.ToString());
                    }
                    string successMessage = "Processed orders by IDs.";
                    SqlHelper.SystemLogInsert("GetLinnOrderForStream", null, requestJson, null, "ProcessedByIds", successMessage, false, res.AuthorizationToken);
                }
                else
                {
                    var filters = _serviceHelper.CreateFilters(res.LinnDays * 24);

                    var allorder = obj.Api.Orders.GetAllOpenOrders(filters, null, Guid.Empty, "");
                    var allorderdetails = obj.Api.Orders.GetOrders(allorder.Skip(0).Take(res.LinnPage).ToList(), Guid.Empty, true, true);
                    foreach (var _order in allorderdetails)
                    {

                        var orderexists = _dbSqlCContext.ReportModel
                          .Where(x => x.LinnNumOrderId == _order.NumOrderId.ToString())
                          .ToList().Count;

                        if (orderexists == 0)
                        {
                            var newjson = JsonConvert.SerializeObject(_order);

                            try
                            {
                                await _tradingApiOAuthHelper.SaveLinnOrder(newjson, res.AuthorizationToken, res.Email, linntoken, _order.NumOrderId.ToString());
                            }
                            catch (Exception ex)
                            {
                                string errorMessage = $"Error saving order { _order.NumOrderId.ToString() }: {ex.Message}";
                                SqlHelper.SystemLogInsert("GetLinnOrderForStream", null, newjson, null, "ErrorSavingOrder", errorMessage, true, res.AuthorizationToken);

                            }

                        }
                    }
                    string successMessage = "Processed open orders.";
                    SqlHelper.SystemLogInsert("GetLinnOrderForStream", null, requestJson, null, "ProcessedOpenOrders", successMessage, false, res.AuthorizationToken);
                }
                return Ok("Orders processed successfully.");
            }
            catch (Exception ex)
            {
                // Log the error (use ILogger for production)
                string errorMessage = $"Error while processing Linn orders: {ex.Message}";
                SqlHelper.SystemLogInsert("GetLinnOrderForStream", null, JsonConvert.SerializeObject(res), null, "Error", errorMessage, true, res.AuthorizationToken);
                return StatusCode(500, "An unexpected error occurred.");
            }

        }

        [HttpGet, Route("CreateLinnworksOrdersToStream")]
        public async Task<IActionResult> CreateLinnworksOrdersToStream(string token, string orderids)
        {
            try
            {
                // Load user configuration
                string requestJson = $"token: {token}, orderids: {orderids}";
                var user = _authToken.Load(token);
                if (string.IsNullOrEmpty(user?.ClientId))
                {
                    string errorMessage = "Invalid client ID.";
                    SqlHelper.SystemLogInsert("CreateLinnworksOrdersToStream", null, requestJson, null, "InvalidClientId", errorMessage, true, token);
                    return BadRequest(errorMessage);
                }

                if (string.IsNullOrEmpty(orderids))
                {
                    // Get pending orders if no order IDs are provided
                    // var reportData = await _reportsController.GetReportData(new ReportModelReq { email = user.Email });

                    var reportData = _dbSqlCContext.ReportModel
                      .Where(x => x.email == user.Email)
                      .ToList();

                    var pendingOrders = reportData.Where(f => !f.IsLinnOrderCreatedInStream && !string.IsNullOrEmpty(f.LinnNumOrderId));

                    foreach (var pendingOrder in pendingOrders)
                    {
                        string ab = JsonConvert.SerializeObject(pendingOrder);

                        SqlHelper.SystemLogInsert("CreateLinnworksOrdersToStream", null, ab, null, "ProcessingPendingOrder", $"Processing pending order: {pendingOrder.LinnNumOrderId}", false, token);

                        await _tradingApiOAuthHelper.CreateLinnworksOrdersToStream(user, pendingOrder.LinnNumOrderId.ToString());
                    }
                }
                else
                {
                    // Process provided order IDs
                    var orderidlist = Regex.Split(orderids, ",");
                    foreach (var _ord in orderidlist)
                    {
                        SqlHelper.SystemLogInsert("CreateLinnworksOrdersToStream", null, _ord, null, "ProcessingOrderId", $"Processing order ID: {_ord}", false, token);
                        await _tradingApiOAuthHelper.CreateLinnworksOrdersToStream(user, _ord);
                    }
                }
                SqlHelper.SystemLogInsert("CreateLinnworksOrdersToStream", null, requestJson, null, "Success", "Linnworks orders successfully created.", false, token);
                return Ok("Linnworks orders successfully created.");
            }
            catch (Exception ex)
            {
                // Log exception (replace Console.WriteLine with ILogger for production)
                Console.WriteLine($"Error creating Linnworks orders: {ex.Message}");
                string errorMessage = $"Error creating Linnworks orders: {ex.Message}";
                SqlHelper.SystemLogInsert("CreateLinnworksOrdersToStream", null, $"token: {token}, orderids: {orderids}", null, "Error", errorMessage, true, token);
                return StatusCode(500, "An unexpected error occurred.");
            }

        }

        [HttpGet, Route("UpdateLinnworksOrdersToStream")]
        public async Task<IActionResult> UpdateLinnworksOrdersToStream(string token, string linntoken, string orderids)
        {
            try
            {
                string requestJson = $"token: {token}, linntoken: {linntoken}, orderids: {orderids}";

                // Load user configuration
                var user = _authToken.Load(token);
                if (string.IsNullOrEmpty(user?.ClientId))
                {
                    string errorMessage = "Invalid client ID.";
                    SqlHelper.SystemLogInsert("UpdateLinnworksOrdersToStream", null, requestJson, null, "InvalidClientId", errorMessage, true, token);
                    return BadRequest(errorMessage);
                }
                linntoken = string.IsNullOrEmpty(linntoken) ? user.LinnworksToken : linntoken;

                if (String.IsNullOrEmpty(linntoken))
                {
                    string errorMessage = "Linnworks token is missing.";
                    SqlHelper.SystemLogInsert("UpdateLinnworksOrdersToStream", null, requestJson, null, "TokenMissing", errorMessage, true, token);
                    return BadRequest(errorMessage);
                }

                var obj = new LinnworksBaseStream(linntoken);


                if (string.IsNullOrEmpty(orderids))
                {
                    // Get pending orders if no order IDs are provided
                    var reportData = _dbSqlCContext.ReportModel
                        .Where(x => x.email == user.Email)
                        .ToList();

                    var pendingOrders = reportData.Where(f => f.IsLinnOrderCreatedInStream && !f.IsLinnOrderDispatchFromStream && !string.IsNullOrEmpty(f.LinnNumOrderId));
                    foreach (var pendingOrder in pendingOrders)
                    {
                        var orderdetails = obj.Api.Orders.GetOrderDetailsByNumOrderId(Convert.ToInt32(pendingOrder.LinnNumOrderId));
                        var newjson = JsonConvert.SerializeObject(orderdetails);
                        var updatedOrder = JsonConvert.DeserializeObject<OrderRoot>(newjson);
                        SqlHelper.SystemLogInsert("UpdateLinnworksOrdersToStream", null, newjson, null, "ProcessingPendingOrder", $"Processing pending order: {pendingOrder.LinnNumOrderId}", false, token);

                        await _tradingApiOAuthHelper.UpdateOrderRootFullAsync(updatedOrder);
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
                            var orderdata = obj.Api.Orders.GetOrderDetailsByNumOrderId(Convert.ToInt32(linnOrders.LinnNumOrderId));
                            var newjson = JsonConvert.SerializeObject(orderdata);
                            var updatedOrder = JsonConvert.DeserializeObject<OrderRoot>(newjson);
                            SqlHelper.SystemLogInsert("UpdateLinnworksOrdersToStream", null, newjson, null, "ProcessingOrderId", $"Processing order ID: {_ord}", false, token);

                            await _tradingApiOAuthHelper.UpdateOrderRootFullAsync(updatedOrder);
                            await _tradingApiOAuthHelper.UpdateLinnworksOrdersToStream(user, _ord, linnOrders.StreamOrderId);
                        }
                    }
                }
                SqlHelper.SystemLogInsert("UpdateLinnworksOrdersToStream", null, requestJson, null, "Success", "Linnworks orders successfully updated.", false, token);

                return Ok("Linnworks orders successfully created.");
            }
            catch (Exception ex)
            {
                // Log exception (replace Console.WriteLine with ILogger for production)
                Console.WriteLine($"Error creating Linnworks orders: {ex.Message}");
                string errorMessage = $"Error updating Linnworks orders: {ex.Message}";
                SqlHelper.SystemLogInsert("UpdateLinnworksOrdersToStream", null, $"token: {token}, linntoken: {linntoken}, orderids: {orderids}", null, "Error", errorMessage, true, token);
                return StatusCode(500, "An unexpected error occurred.");
            }

        }

        [HttpGet(), Route("DispatchLinnworksOrdersFromStream")]
        public async Task<IActionResult> DispatchLinnworksOrdersFromStream(string token, string orderids, string linntoken)
        {
            try
            {
                string requestJson = $"token: {token}, orderids: {orderids}, linntoken: {linntoken}";
                // Load user configuration
                Rishvi.Models.Authorization user = _authToken.Load(token);
                if (string.IsNullOrEmpty(user?.ClientId))
                {
                    string errorMessage = "Invalid or missing client ID.";
                    SqlHelper.SystemLogInsert("DispatchLinnworksOrdersFromStream", null, requestJson, null, "InvalidClientId", errorMessage, true, token);
                    return BadRequest(errorMessage);
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
                        SqlHelper.SystemLogInsert("DispatchLinnworksOrdersFromStream", null, $"LinnNumOrderId: {_linnord.LinnNumOrderId}", null, "ProcessingDispatchOrder", $"Dispatching order: {_linnord.LinnNumOrderId}", false, token);
                        await DispatchOrderInner(user, _linnord.LinnNumOrderId, linntoken, token);
                    }

                }
                else
                {
                    // Process provided order IDs
                    var orderidlist = Regex.Split(orderids, ",");
                    foreach (var _ord in orderidlist)
                    {
                        SqlHelper.SystemLogInsert("DispatchLinnworksOrdersFromStream", null, $"OrderId: {_ord}", null, "ProcessingDispatchOrder", $"Dispatching order: {_ord}", false, token);

                        await DispatchOrderInner(user, _ord, linntoken, token);
                    }
                }
                SqlHelper.SystemLogInsert("DispatchLinnworksOrdersFromStream", null, requestJson, null, "Success", "Linnworks orders successfully dispatched.", false, token);

                return Ok("Orders dispatched successfully.");
            }
            catch (Exception ex)
            {
                // Log the error (replace Console.WriteLine with ILogger for production)
                Console.WriteLine($"Error dispatching Linnworks orders: {ex.Message}");
                string errorMessage = $"Error dispatching Linnworks orders: {ex.Message}";
                SqlHelper.SystemLogInsert("DispatchLinnworksOrdersFromStream", null, $"token: {token}, orderids: {orderids}, linntoken: {linntoken}", null, "Error", errorMessage, true, token);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        private async Task DispatchOrderInner(Rishvi.Models.Authorization user, string orderId, string linntoken, string token)
        {
            try
            {
                //var linnDispatchPath = $"LinnDispatch/{token}_linndispatch_{orderId}.json";
                string requestJson = $"user: {user?.Email}, orderId: {orderId}, linntoken: {linntoken}";
                var linnStreamPath = $"LinnStreamOrder/_streamorder_{orderId}.json";
                var streamOrderExists = await _dbSqlCContext.StreamOrderRecord
                    .AnyAsync(x => x.LinnworksOrderId == orderId);

                var dispatchDone = await _dbSqlCContext.ReportModel
                    .Where(x => x.LinnNumOrderId == orderId)
                    .Select(x => x.IsLinnOrderDispatchFromStream)
                    .FirstOrDefaultAsync();

            if (!dispatchDone && streamOrderExists)
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
                    SqlHelper.SystemLogInsert("DispatchOrderInner", null, $"OrderId: {orderId}", null, "DispatchSuccess", $"Order {orderId} successfully dispatched to Linnworks", false, token);

                }
                else
                {
                    // Log failure when stream data is null or invalid
                    SqlHelper.SystemLogInsert("DispatchOrderInner", null, $"OrderId: {orderId}", null, "StreamDataError", "Stream data is null or invalid", true, token);
                }
            }
            else
            {
                SqlHelper.SystemLogInsert("DispatchOrderInner", null, requestJson, null, "DispatchSkipped", $"Order {orderId} was skipped due to dispatch status or stream order not found.", false, token);
            }
            }
            catch (Exception ex)
            {
                // Log exception
                string errorMessage = $"Error dispatching Linnworks order {orderId}: {ex.Message}";
                SqlHelper.SystemLogInsert("DispatchOrderInner", null, $"orderId: {orderId}", null, "Error", errorMessage, true, token);
            }
            
        }

        [HttpGet, Route("DispatchOrder")]
        public async Task<IActionResult> DispatchOrder([FromQuery] OrderDispatchReq value)
        {
            try
            {
                string requestJson = JsonConvert.SerializeObject(value);
                // Validate the token and user configuration
                var user = _authToken.Load(value.token);
                if (string.IsNullOrEmpty(user?.access_token))
                {
                    string errorMessage = "Invalid or missing access token.";
                    SqlHelper.SystemLogInsert("DispatchOrder", null, requestJson, null, "InvalidAccessToken", errorMessage, true, value.token);
                    return BadRequest(errorMessage);
                }

                // Validate the order reference
                if (!value.IsValidInt32())
                {
                    string errorMessage = "Invalid order reference.";
                    SqlHelper.SystemLogInsert("DispatchOrder", null, requestJson, null, "InvalidOrderReference", errorMessage, true, value.token);
                    return BadRequest(errorMessage);
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
                SqlHelper.SystemLogInsert("DispatchOrder", null, requestJson, null, "Success", $"Order {value.orderref} dispatched successfully.", false, value.token);

                return Ok("Order dispatched successfully.");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error dispatching order: {ex.Message}");
                string errorMessage = $"Error dispatching order: {ex.Message}";
                SqlHelper.SystemLogInsert("DispatchOrder", null, JsonConvert.SerializeObject(value), null, "Error", errorMessage, true, value.token);
                return StatusCode(500, "An unexpected error occurred while dispatching the order.");
            }
        }
       
        [NonAction]
        public async Task UpdateOrderIdentifier(string linntoken, int orderid, string identifier)
        {
            try
            {
                string requestJson = $"linntoken: {linntoken}, orderid: {orderid}, identifier: {identifier}";
                // Validate inputs
                if (string.IsNullOrWhiteSpace(linntoken))
                {
                    string errorMessage = "Invalid Linnworks token.";
                    SqlHelper.SystemLogInsert("UpdateOrderIdentifier", null, requestJson, null, "InvalidToken", errorMessage, true, linntoken);
                    throw new ArgumentException(errorMessage);
                }
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    string errorMessage = "Identifier cannot be null or empty.";
                    SqlHelper.SystemLogInsert("UpdateOrderIdentifier", null, requestJson, null, "InvalidIdentifier", errorMessage, true, linntoken);
                    throw new ArgumentException(errorMessage);
                }

                // Initialize Linnworks API object
                var obj = new LinnworksBaseStream(linntoken);

                // Fetch the Linnworks order details
                var linnOrderDetails = obj.Api.Orders.GetOrderDetailsByNumOrderId(orderid);
                if (linnOrderDetails == null)
                {
                    string errorMessage = $"Order with ID {orderid} not found.";
                    SqlHelper.SystemLogInsert("UpdateOrderIdentifier", null, requestJson, null, "OrderNotFound", errorMessage, true, linntoken);
                    throw new Exception(errorMessage);
                }

                // Standardize identifier tag
                var identifierTag = identifier.ToUpper();

                // Check if the identifier already exists
                var identifiers = obj.Api.OpenOrders.GetIdentifiers();
                if (!identifiers.Any(d => d.Tag == identifierTag))
                {
                    // Save the new identifier
                    SqlHelper.SystemLogInsert("UpdateOrderIdentifier", null, requestJson, null, "SaveIdentifier", $"Saving new identifier {identifierTag}.", false, linntoken);
                    await SaveNewIdentifier(obj, identifierTag);
                }

                // Assign the identifier to the order
                obj.Api.OpenOrders.AssignOrderIdentifier(new ChangeOrderIdentifierRequest
                {
                    OrderIds = new[] { linnOrderDetails.OrderId },
                    Tag = identifierTag
                });
                SqlHelper.SystemLogInsert("UpdateOrderIdentifier", null, requestJson, null, "Success", $"Order {orderid} successfully updated with identifier {identifierTag}.", false, linntoken);

            }
            catch (Exception ex)
            {
                string errorMessage = $"Error updating order identifier: {ex.Message}";
                SqlHelper.SystemLogInsert("UpdateOrderIdentifier", null, $"linntoken: {linntoken}, orderid: {orderid}, identifier: {identifier}", null, "Error", errorMessage, true, linntoken);

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

        [NonAction]
        public async Task UpdateDispatchDate(string linntoken, int orderid, DateTime dispatchdate)
        {
            try
            {
                string requestJson = $"linntoken: {linntoken}, orderid: {orderid}, dispatchdate: {dispatchdate}";

                // Validate inputs
                if (string.IsNullOrWhiteSpace(linntoken))
                {
                    string errorMessage = "Invalid Linnworks token.";
                    SqlHelper.SystemLogInsert("UpdateDispatchDate", null, requestJson, null, "InvalidToken", errorMessage, true, linntoken);
                    throw new ArgumentException(errorMessage);
                }
                // Initialize Linnworks API object
                var obj = new LinnworksBaseStream(linntoken);

                // Fetch the Linnworks order details
                var linnOrderDetails = obj.Api.Orders.GetOrderDetailsByNumOrderId(orderid);
                if (linnOrderDetails == null)
                {
                    string errorMessage = $"Order with ID {orderid} not found.";
                    SqlHelper.SystemLogInsert("UpdateDispatchDate", null, requestJson, null, "OrderNotFound", errorMessage, true, linntoken);
                    throw new Exception(errorMessage);
                }

                // Standardize identifier tag
                var generalInfo = linnOrderDetails.GeneralInfo;
                generalInfo.DespatchByDate = dispatchdate;

                // Update the order's GeneralInfo with the new DespatchByDate
                obj.Api.Orders.SetOrderGeneralInfo(linnOrderDetails.OrderId, generalInfo, false);
                SqlHelper.SystemLogInsert("UpdateDispatchDate", null, requestJson, null, "Success", $"Successfully updated dispatch date for order {orderid} to {dispatchdate}.", false, linntoken);


            }
            catch (Exception ex)
            {
                string errorMessage = $"Error updating dispatch date for order {orderid}: {ex.Message}";
                SqlHelper.SystemLogInsert("UpdateDispatchDate", null, $"linntoken: {linntoken}, orderid: {orderid}, dispatchdate: {dispatchdate}", null, "Error", errorMessage, true, linntoken);
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
            string requestJson = $"identifierTag: {identifierTag}";
            var standardizedIdentifierTag = identifierTag.ToUpper();

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
            SqlHelper.SystemLogInsert("SaveNewIdentifier", null, requestJson, null, "Success", $"New identifier {standardizedIdentifierTag} saved successfully.", false, obj.Api.GetSessionId().ToString());

        }

        [HttpGet, Route("CreateIdentifier")]
        public async Task<IActionResult> CreateIdentifier(string linntoken, string identifier)
        {
            try
            {
                string requestJson = $"linntoken: {linntoken}, identifier: {identifier}";

                // Validate inputs
                if (string.IsNullOrWhiteSpace(linntoken))
                {
                    string errorMessage = "Invalid Linnworks token.";
                    SqlHelper.SystemLogInsert("CreateIdentifier", null, requestJson, null, "InvalidToken", errorMessage, true, linntoken);
                    return BadRequest(errorMessage);
                }

                if (string.IsNullOrWhiteSpace(identifier))
                {
                    string errorMessage = "Identifier cannot be null or empty.";
                    SqlHelper.SystemLogInsert("CreateIdentifier", null, requestJson, null, "InvalidIdentifier", errorMessage, true, linntoken);
                    return BadRequest(errorMessage);
                }
                // Handle "days" case
                if (identifier.ToLower() == "days")
                {
                    var daysOfWeek = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                    foreach (var day in daysOfWeek)
                    {
                        SqlHelper.SystemLogInsert("CreateIdentifier", null, $"linntoken: {linntoken}, identifier: {day}", null, "ProcessingDayIdentifier", $"Processing day identifier: {day}", false, linntoken);

                        await _serviceHelper.ManageIdentifier(linntoken, day);
                    }
                }
                else
                {
                    // Handle single identifier
                    SqlHelper.SystemLogInsert("CreateIdentifier", null, $"linntoken: {linntoken}, identifier: {identifier}", null, "ProcessingSingleIdentifier", $"Processing identifier: {identifier}", false, linntoken);

                    await _serviceHelper.ManageIdentifier(linntoken, identifier);
                }
                
                return Ok("Identifier(s) processed successfully.");

            }
            catch (Exception ex)
            {
                // Log the error (replace Console.WriteLine with ILogger in production)
                Console.WriteLine($"Error creating identifier: {ex.Message}");
                string errorMessage = $"Error creating identifier: {ex.Message}";
                SqlHelper.SystemLogInsert("CreateIdentifier", null, $"linntoken: {linntoken}, identifier: {identifier}", null, "Error", errorMessage, true, linntoken);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet, Route("SubscribeWebhook")]
        public async Task<IActionResult> SubscribeWebhook(string token)
        {
            try
            {
                // Validate token
                string requestJson = $"token: {token}";

                if (string.IsNullOrWhiteSpace(token))
                {
                    string errorMessage = "Token is required.";
                    SqlHelper.SystemLogInsert("SubscribeWebhook", null, requestJson, null, "InvalidToken", errorMessage, true, token);
                    return BadRequest(errorMessage);
                }

                var user = _authToken.Load(token);

                var streamAuth = _managetoken.GetToken(user);
                if (string.IsNullOrWhiteSpace(streamAuth?.AccessToken))
                {
                    string errorMessage = "Invalid or missing access token.";
                    SqlHelper.SystemLogInsert("SubscribeWebhook", null, requestJson, null, "InvalidAccessToken", errorMessage, true, token);
                    return BadRequest(errorMessage);
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
                string errorMessage = $"Error subscribing to webhooks: {ex.Message}";
                SqlHelper.SystemLogInsert("SubscribeWebhook", null, $"token: {token}", null, "Error", errorMessage, true, token);
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
            //await _tradingApiOAuthHelper.SaveWebhook(data, output.webhook.subscription.party_id, DateTime.Now.ToString("ddMMyyyyhhmmss"));
            SqlHelper.SystemLogInsert("Webhook_riddhi", "", JsonConvert.SerializeObject(output).Replace("'", "''"), "", "Webhook", "", false, "Webhook");

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
            Run run = new Run();
            if (output.webhook.run!=null)
            {


             run = new Run
            {
                loadId = output.webhook.run.loadId,
                status = output.webhook.run.status,
                description = output.webhook.run.description,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };
                _run.Add(run);
            }
            SqlHelper.SystemLogInsert("Webhook", "", $"Subscription: {JsonConvert.SerializeObject(subscription)}, Event: {JsonConvert.SerializeObject(@event)}, Run: { JsonConvert.SerializeObject(run)}", "", "ProcessedWebhook", "Webhook data processed", false, "Webhook");

            _subscription.Add(subscription);
            _event.Add(@event);
            
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
            try
            {


                if (output.webhook.@event.event_code == "OPENPLANNING" ||
                    output.webhook.@event.event_code == "LOCKPLANNING" ||
                    output.webhook.@event.event_code == "CLOSEPLANNING" ||
                    output.webhook.@event.event_code == "DISPATCHED" ||
                    output.webhook.@event.event_code == "PLANNEDDELIVERY" ||
                    output.webhook.@event.event_code == "PLANNEDGROUP"
                    )
                {
                    SqlHelper.SystemLogInsert("Webhook", "", $"Handling specific event code: {output.webhook.@event.event_code}", "", "EventHandling", "Handling specific event code in webhook", false, "Webhook");

                    var webhookOrder = new WebhookOrder
                    {
                        sequence = 0,
                        order = output.webhook.@event.event_code,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _webhookOrder.Add(webhookOrder);
                    _unitOfWork.Context.SaveChanges();
                    if (output.webhook.orders != null)
                    {
                        foreach (var strorder in output.webhook.orders)
                        {
                            try
                            {
                                SqlHelper.SystemLogInsert("Webhook", "", $"Processing order {strorder.order}", "", "OrderProcessing", $"Processing order {strorder.order} in specific event", false, "Webhook");


                                // need to update on linn order
                                string Stream_runloadid = output.webhook.run.loadId;
                                string Stream_runstatus = output.webhook.run.status;
                                string Stream_rundescription = output.webhook.run.description;
                                string Stream_orderid = strorder.order;
                                // call order api to get driver detail or driver detail
                                //string json = AwsS3.GetS3File("Authorization", "StreamParty/" + output.webhook.subscription.party_id + ".json");
                                Authorization user = _authorization.Get().Where(x => x.ClientId == output.webhook.subscription.party_id).FirstOrDefault();
                                //var user = JsonConvert.DeserializeObject<AuthorizationConfigClass>(json);
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

                                var logindata = _dbSqlCContext.Authorizations.Where(x => x.Email == user.Email).FirstOrDefault();

                               // var logindata = await _configController.Get(user.Email);
                                var strorderdaat = await _streamController.GetStreamOrder(user.AuthorizationToken, Stream_orderid);
                                
                                if (strorderdaat != null)
                                {
                                    if (strorderdaat.response.valid)  
                                    {
                                        foreach (var gr in strorderdaat.response.order.groups)
                                        {
                                            SqlHelper.SystemLogInsert("Webhook", "", $"Updating Linnworks order {Stream_orderid} with tracking and driver info", "", "LinnworksUpdate", "Updating Linnworks order with driver and vehicle details", false, "Webhook");

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
                                            string stream_GroupSequence = gr.runDetails.groupSequence.ToString();





                                            string LinnworksSyncToken = "";
                                            string Email = "";

                                            if (logindata !=null)
                                            {
                                               // var userData = okResult.Value as RegistrationData;
                                                LinnworksSyncToken = logindata.LinnworksToken;
                                                Email = logindata.Email;
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
                                            Task.Delay(5000);
                                            // update on linnworks
                                            if (linnworksorderid.IsValidInt32() && !String.IsNullOrEmpty(LinnworksSyncToken))
                                            {
                                                SqlHelper.SystemLogInsert("Webhook", "", $"Updating Linnworks order {linnworksorderid} with tracking info", "", "LinnworksTrackingUpdate", $"Updating Linnworks order {linnworksorderid} with tracking ID {Stream_trackingId}", false, "Webhook");

                                                await _tradingApiOAuthHelper.UpdateOrderExProperty(LinnworksSyncToken, Convert.ToInt32(linnworksorderid), new Dictionary<string, string>() {
                                                    //{"Stream_runloadid",Stream_runloadid },
                                                    //{"Stream_runstatus",Stream_runstatus },
                                                    //{"Stream_rundescription",Stream_rundescription },
                                                    //{"Stream_orderid",Stream_orderid },
                                                    {"Stream_driverName",Stream_driverName },
                                                   // {"Stream_driver",Stream_driver},
                                                   // {"Stream_vehicle",Stream_vehicle },
                                                    {"Stream_vehicleName",Stream_vehicleName },
                                                   // {"Stream_status",Stream_status },
                                                    {"Stream_driverNotes",Stream_driverNotes },
                                                    //{"Stream_estimateArrivalDateTime",Stream_estimateArrivalDateTime },
                                                   // {"Stream_vehicleType",Stream_vehicleType },
                                                    //{"Stream_dispatched",Stream_dispatched },
                                                    //{"Stream_departed",Stream_departed },
                                                    //{"Stream_completed",Stream_completed},
                                                    //{"Stream_startActualDateTime", Stream_startActualDateTime},
                                                    //{"Stream_startPlannedDateTime", Stream_startPlannedDateTime},
                                                    //{"Stream_endActualDateTime",Stream_endActualDateTime },
                                                    //{ "Stream_endPlannedDateTime",Stream_endPlannedDateTime }
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
                                                if (Stream_rundescription!=null)
                                                {
                                                    await UpdateOrderIdentifier(LinnworksSyncToken, Convert.ToInt32(linnworksorderid), Stream_rundescription);
                                                }

                                                if (gr.planned.fromDateTime != null)
                                                {
                                                    var dte = DateTime.Parse(gr.planned.fromDateTime.Substring(0, 11) + gr.planned.fromDateTime.Substring(11).Replace("-", ":"), null, System.Globalization.DateTimeStyles.RoundtripKind);

                                                    await UpdateDispatchDate(LinnworksSyncToken, Convert.ToInt32(linnworksorderid),dte);
                                                    //await _tradingApiOAuthHelper.DispatchOrderInLinnworks(user, Convert.ToInt32(linnworksorderid), LinnworksSyncToken, "Stream", Stream_trackingId, Stream_trackingURL, gr.estimateArrivalDateTime.Replace("-00Z", ":00Z"));

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
                                    EmailHelper.SendEmail("Error Webhook Update", "User Authentication Issue - " + user.AuthorizationToken + " Data:" + data);
                                }

                            }
                            catch (Exception ex)
                            {
                                SqlHelper.SystemLogInsert("Webhook", "", $"Error processing order {strorder.order}: {ex.Message}", "", "OrderProcessingError", "Error processing order in webhook", true, "Webhook");

                                EmailHelper.SendEmail("Error Webhook Update", "webhook error Data:" + data + "  ex: " + ex.ToString());
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
                        //string json = AwsS3.GetS3File("Authorization", "StreamParty/" + output.webhook.subscription.party_id + ".json");
                        Authorization user = _authorization.Get().Where(x => x.ClientId == output.webhook.subscription.party_id).FirstOrDefault();
                        //var user = JsonConvert.DeserializeObject<AuthorizationConfigClass>(json);
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
                                Task.Delay(5000);
                                // update on linnworks
                                if (linnworksorderid.IsValidInt32() && !String.IsNullOrEmpty(LinnworksSyncToken))
                                {
                                    await _tradingApiOAuthHelper.UpdateOrderExProperty(LinnworksSyncToken, Convert.ToInt32(linnworksorderid), new Dictionary<string, string>() {
                                //{"Stream_runloadid",Stream_runloadid },
                                //{"Stream_runstatus",Stream_runstatus },
                                //{"Stream_rundescription",Stream_rundescription },
                                //{"Stream_orderid",Stream_orderid },
                                {"Stream_driverName",Stream_driverName },
                               // {"Stream_driver",Stream_driver},
                               // {"Stream_vehicle",Stream_vehicle },
                                {"Stream_vehicleName",Stream_vehicleName },
                               // {"Stream_status",Stream_status },
                                {"Stream_driverNotes",Stream_driverNotes },
                                //{"Stream_estimateArrivalDateTime",Stream_estimateArrivalDateTime },
                               // {"Stream_vehicleType",Stream_vehicleType },
                                //{"Stream_dispatched",Stream_dispatched },
                                //{"Stream_departed",Stream_departed },
                                //{"Stream_completed",Stream_completed},
                                //{"Stream_startActualDateTime", Stream_startActualDateTime},
                                //{"Stream_startPlannedDateTime", Stream_startPlannedDateTime},
                                //{"Stream_endActualDateTime",Stream_endActualDateTime },
                                //{ "Stream_endPlannedDateTime",Stream_endPlannedDateTime }
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
                                    if (Stream_rundescription != null)
                                    {
                                        await UpdateOrderIdentifier(LinnworksSyncToken, Convert.ToInt32(linnworksorderid), Stream_rundescription);
                                    }

                                    if (gr.planned.fromDateTime != null)
                                    {
                                        var dte = DateTime.Parse(gr.planned.fromDateTime.Substring(0, 11) + gr.planned.fromDateTime.Substring(11).Replace("-", ":"), null, System.Globalization.DateTimeStyles.RoundtripKind);

                                        await UpdateDispatchDate(LinnworksSyncToken, Convert.ToInt32(linnworksorderid), dte);
                                        //await _tradingApiOAuthHelper.DispatchOrderInLinnworks(user, Convert.ToInt32(linnworksorderid), LinnworksSyncToken, "Stream", Stream_trackingId, Stream_trackingURL, gr.estimateArrivalDateTime.Replace("-00Z", ":00Z"));

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
                string errorMessage = $"Error handling webhook data: {ex.Message}";
                SqlHelper.SystemLogInsert("Webhook", "", $"data: {data}", "", "Error", errorMessage, true, "Webhook");

                EmailHelper.SendEmail("Error Json", data + " Data error " + ex.Message);
            }
            return new Dictionary<string, string>();
        }

    }
}
