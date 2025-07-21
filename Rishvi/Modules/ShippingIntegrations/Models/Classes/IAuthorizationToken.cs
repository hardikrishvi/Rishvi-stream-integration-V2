namespace Rishvi.Modules.ShippingIntegrations.Models.Classes
{
    public interface IAuthorizationToken
    {
        Rishvi.Models.Authorization Load(string AuthorizationToken);

        void Delete(string AuthorizationToken);
        Rishvi.Models.Authorization CreateNew(string email, string SessionID,
            string LinnworksUniqueIdentifier, string accountName, string clientid = null, string secret = null,
            string state = null);
    }
}
