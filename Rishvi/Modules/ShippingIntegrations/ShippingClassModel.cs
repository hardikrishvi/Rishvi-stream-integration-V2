namespace Rishvi.Modules.ShippingIntegrations
{
    public class ShippingClassModel
    {
        public class ShippingTag
        {
            public string FriendlyName { get; set; }
            public string Tag { get; set; }
            public string Site { get; set; }
        }
        public class StreamRequest
        {
            public string authToken { get; set; }
            public string email { get; set; }
            public string accountName { get; set; }
            public string clientId { get; set; }

            public string clientSecret { get; set; }
        }
    }
}
