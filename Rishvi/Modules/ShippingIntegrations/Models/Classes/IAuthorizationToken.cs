namespace Rishvi.Modules.ShippingIntegrations.Models.Classes
{
    public interface IAuthorizationToken
    {
        AuthorizationConfigClass Load(string AuthorizationToken);

        void Delete(string AuthorizationToken);
        AuthorizationConfigClass CreateNew(string email, string SessionID,
            string LinnworksUniqueIdentifier, string accountName, string clientid = null, string secret = null,
            string state = null);
    }
}
