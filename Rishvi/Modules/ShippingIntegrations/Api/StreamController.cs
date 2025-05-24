using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using System.Text.RegularExpressions;
using static Rishvi.Modules.ShippingIntegrations.Core.TradingApiOAuthHelper;

namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/Stream")]
    public class StreamController : ControllerBase
    {
        private readonly IAuthorizationToken _authorizationToken;

        private readonly TradingApiOAuthHelper _tradingApiOAuthHelper;
        private readonly ReportsController _reportsController;
        public StreamController(ReportsController reportsController, TradingApiOAuthHelper tradingApiOAuthHelper, IAuthorizationToken authorizationToken)
        {
            _authorizationToken = authorizationToken;
            _reportsController = reportsController;
            _tradingApiOAuthHelper = tradingApiOAuthHelper;
        }

        [HttpGet, Route("GetStreamOrder")]
        public async Task<StreamGetOrderResponse.Root> GetStreamOrder(string token, string orderids)
        {
            // Load user authorization
            var user = _authorizationToken.Load(token);
            if (user == null || string.IsNullOrEmpty(user.ClientId))
            {
                return null; // Invalid user or missing Client ID
            }

            // Extract the first order ID from the list
            var orderidlist = Regex.Split(orderids, ",");
            var firstOrderId = orderidlist.FirstOrDefault();
            if (!string.IsNullOrEmpty(firstOrderId))
            {
                return await _tradingApiOAuthHelper.GetStreamOrder(user, firstOrderId);
            }

            return null; // No order IDs were provided
        }

        //[HttpGet, Route("CreateEbayOrdersToStream")]
        //public async Task<IActionResult> CreateEbayOrdersToStream(string token, string orderids)
        //{
        //    try
        //    {
        //        // Validate token
        //        if (string.IsNullOrWhiteSpace(token))
        //        {
        //            return BadRequest("Authorization token is required.");
        //        }

        //        // Load user authorization
        //        var user = _authorizationToken.Load(token);
        //        if (user == null || string.IsNullOrEmpty(user.ClientId))
        //        {
        //            return Unauthorized("Invalid or missing Client ID.");
        //        }

        //        // Process orders
        //        if (string.IsNullOrWhiteSpace(orderids))
        //        {
        //            // Fetch all pending eBay orders
        //            var reportData = await _reportsController.GetReportData(new ReportModelReq { email = user.Email });
        //            var pendingOrders = reportData
        //                .Where(f => !f.IsEbayOrderCreatedInStream && !string.IsNullOrEmpty(f.EbayChannelOrderRef))
        //                .Select(f => f.EbayChannelOrderRef);

        //            await ProcessEbayOrders(user, pendingOrders);
        //        }
        //        else
        //        {
        //            // Process specific orders
        //            var orderIdList = Regex.Split(orderids, ",");
        //            await ProcessEbayOrders(user, orderIdList);
        //        }

        //        return Ok("eBay orders successfully processed.");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error (replace with ILogger for production)
        //        Console.WriteLine($"Error creating eBay orders: {ex.Message}");
        //        return StatusCode(500, "An unexpected error occurred.");
        //    }
        //}

        //private async Task ProcessEbayOrders(AuthorizationConfigClass user, IEnumerable<string> orderIds)
        //{
        //    var tasks = orderIds
        //        .Where(orderId => !string.IsNullOrWhiteSpace(orderId)) // Ensure valid order IDs
        //        .Select(orderId => _tradingApiOAuthHelper.CreateEbayOrdersToStream(user, orderId));

        //    await Task.WhenAll(tasks); // Process all orders in parallel
        //}
    }

}
