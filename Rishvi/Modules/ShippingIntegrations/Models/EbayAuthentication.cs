namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class EbayAuthentication
    {
        public string access_token { get; set; }
        public string ExpirationTime { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public int refresh_token_expires_in { get; set; }
        public string token_type { get; set; }
    }
}
