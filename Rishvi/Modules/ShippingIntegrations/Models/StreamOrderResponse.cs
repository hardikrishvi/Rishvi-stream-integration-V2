namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class StreamOrderResponse
    {
        public Response response { get; set; }

        public string AuthorizationToken { get; set; }
        public string ItemId { get; set; }
    }
    public class Response
    {
        public bool valid { get; set; }
        public string consignmentNo { get; set; }
        public string orderNo { get; set; }
        public string customerOrderNo { get; set; }
        public string trackingURL { get; set; }
        public string trackingId { get; set; }
        public List<Error> errors { get; set; }
    }

    public class Error
    {
        public string code { get; set; }
        public string description { get; set; }
        public string severity { get; set; }
    }
}

