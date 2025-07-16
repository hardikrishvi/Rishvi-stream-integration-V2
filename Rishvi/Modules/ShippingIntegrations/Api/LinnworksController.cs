using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Rishvi.Models;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Core.Helper;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;

namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinnworksController : ControllerBase
    {
        private readonly TradingApiOAuthHelper _tradingApiOAuthHelper;
        private readonly IAuthorizationToken _authToken;
        private readonly SqlContext _dbSqlContext;
        private readonly IServiceHelper _serviceHelper;

        public LinnworksController(
            TradingApiOAuthHelper tradingApiOAuthHelper,
            IAuthorizationToken authToken,
            SqlContext dbSqlContext, IServiceHelper serviceHelper)
        {
            _tradingApiOAuthHelper = tradingApiOAuthHelper;
            _authToken = authToken;
            _dbSqlContext = dbSqlContext;
            _serviceHelper = serviceHelper;
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
                        var orderexists = _dbSqlContext.ReportModel
                            .Where(x => x.LinnNumOrderId == _order.NumOrderId.ToString())
                            .ToList().Count;

                        if (orderexists == 0)
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
                Console.WriteLine($"Error while processing Linn orders: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }


        [HttpGet("CreateLinnworksOrdersToStream")]
        public async Task<IActionResult> CreateLinnworksOrdersToStream(string token, string orderIds)
        {
            try
            {
                var user = _authToken.Load(token);
                if (user == null)
                    return BadRequest("Invalid token.");

                if (string.IsNullOrEmpty(orderIds))
                {
                    var orders = _dbSqlContext.ReportModel
                        .Where(x => x.email == user.Email && !x.IsLinnOrderCreatedInStream)
                        .ToList();

                    foreach (var ord in orders)
                    {
                        await _tradingApiOAuthHelper.DispatchLinnOrdersFromStream(user, ord.LinnNumOrderId, user.LinnworksToken);
                    }
                }
                else
                {
                    foreach (var orderId in orderIds.Split(','))
                    {
                        await _tradingApiOAuthHelper.DispatchLinnOrdersFromStream(user, orderId, user.LinnworksToken);
                    }
                }

                return Ok("Linnworks orders dispatched to Stream successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in CreateLinnworksOrdersToStream: {ex.Message}");
                return StatusCode(500, "Internal error.");
            }
        }

        [HttpGet(), Route("DispatchLinnworksOrdersFromStream")]
        public async Task<IActionResult> DispatchLinnworksOrdersFromStream(string token, string orderids, string linntoken)
        {
            try
            {
                var user = _authToken.Load(token);
                if (user == null)
                    return BadRequest("Invalid token.");

                if (string.IsNullOrEmpty(orderids))
                {
                    var pendingOrders = _dbSqlContext.ReportModel
                        .Where(x => x.email == user.Email && !x.IsLinnOrderDispatchFromStream)
                        .ToList();

                    foreach (var ord in pendingOrders)
                    {
                        await _tradingApiOAuthHelper.DispatchLinnOrdersFromStream(user, ord.LinnNumOrderId, user.LinnworksToken);
                    }
                }
                else
                {
                    foreach (var orderId in orderids.Split(','))
                    {
                        await _tradingApiOAuthHelper.DispatchLinnOrdersFromStream(user, orderId, user.LinnworksToken);
                    }
                }

                return Ok("Orders dispatched successfully from Stream.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in DispatchLinnworksOrdersFromStream: {ex.Message}");
                return StatusCode(500, "Internal error.");
            }
        }

        [HttpGet, Route("UpdateLinnworksOrdersToStream")]
        public async Task<IActionResult> UpdateLinnworksOrdersToStream(string token, string orderids)
        {
            try
            {
                var user = _authToken.Load(token);
                if (string.IsNullOrEmpty(user?.ClientId))
                {
                    return BadRequest("Invalid client ID.");
                }

                if (string.IsNullOrEmpty(orderids))
                {
                    var reportData = _dbSqlContext.ReportModel
                        .Where(x => x.email == user.Email)
                        .ToList();

                    var pendingOrders = reportData
                        .Where(f => f.IsLinnOrderCreatedInStream && !f.IsLinnOrderDispatchFromStream && !string.IsNullOrEmpty(f.LinnNumOrderId));

                    foreach (var pendingOrder in pendingOrders)
                    {
                        await _tradingApiOAuthHelper.UpdateLinnworksOrdersToStream(user, pendingOrder.LinnNumOrderId, pendingOrder.StreamOrderId);
                    }
                }
                else
                {
                    var orderidlist = Regex.Split(orderids, ",");
                    var reportData = await _dbSqlContext.ReportModel
                        .Where(f => f.IsLinnOrderCreatedInStream && orderidlist.Contains(f.LinnNumOrderId))
                        .ToListAsync();

                    foreach (var _ord in orderidlist)
                    {
                        var linnOrder = reportData.FirstOrDefault(f => f.LinnNumOrderId == _ord);
                        if (linnOrder != null)
                        {
                            await _tradingApiOAuthHelper.UpdateLinnworksOrdersToStream(user, _ord, linnOrder.StreamOrderId);
                        }
                    }
                }

                return Ok("Linnworks orders successfully updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Linnworks orders: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }


        [HttpGet("SubscribeWebhook")]
        public async Task<IActionResult> SubscribeWebhook(string token)
        {
            try
            {
                var user = _authToken.Load(token);
                if (user == null)
                    return BadRequest("Invalid token.");

                await _tradingApiOAuthHelper.CreateStreamWebhook(
                    user,
                    "ORDERSTATUS", "CONSHEADER",
                    "https://stream-api-stg.rishvi.app/api/Linnworks/webhook",
                    "POST", "application/json",
                    $"Bearer {user.AuthorizationToken}"
                );

                return Ok("Webhook subscribed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error subscribing webhook: {ex.Message}");
                return StatusCode(500, "Internal error.");
            }
        }
    }
}
