using LinnworksAPI;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using static Rishvi.Modules.ShippingIntegrations.Core.Helper.ServiceHelper;

namespace Rishvi.Modules.ShippingIntegrations.Core.Helper
{
    public interface IServiceHelper
    {
        string HashPassword(string password);
        string TransformEmail(string email);
        FieldsFilter CreateFilters(int linnhour);
        Task ManageIdentifier(string linntoken, string identifier);
        Task CreateWebhook(AuthorizationConfigClass user, WebhookSubscription webhook, string token);
    }
}
