using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;

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

      
    }

}
