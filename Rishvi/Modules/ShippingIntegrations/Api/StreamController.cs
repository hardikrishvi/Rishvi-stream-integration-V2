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
        private readonly ILogger<StreamController> _logger;
        public StreamController(ReportsController reportsController, TradingApiOAuthHelper tradingApiOAuthHelper, IAuthorizationToken authorizationToken, ILogger<StreamController> logger)
        {
            _authorizationToken = authorizationToken;
            _reportsController = reportsController;
            _tradingApiOAuthHelper = tradingApiOAuthHelper;
            _logger = logger;
        }

        [HttpGet, Route("GetStreamOrder")]
        public async Task<StreamGetOrderResponse.Root> GetStreamOrder(string token, string orderids)
        {
            // Load user authorization
            _logger.LogInformation("Attempting to load user authorization with token: {Token}", token);
            var user = _authorizationToken.Load(token);
            if (user == null || string.IsNullOrEmpty(user.ClientId))
            {
                _logger.LogWarning("Invalid user or missing Client ID for token: {Token}", token);
                return null; // Invalid user or missing Client ID
            }

            // Extract the first order ID from the list
            var orderidlist = Regex.Split(orderids, ",");
            var firstOrderId = orderidlist.FirstOrDefault();
            if (!string.IsNullOrEmpty(firstOrderId))
            {
                _logger.LogInformation("Fetching stream order for user: {UserEmail}, Order ID: {OrderId}", user.Email, firstOrderId);
                return await _tradingApiOAuthHelper.GetStreamOrder(user, firstOrderId);
            }
            _logger.LogWarning("No valid order IDs provided for user: {UserEmail}", user.Email);
            return null; // No order IDs were provided
        }

    }

}
