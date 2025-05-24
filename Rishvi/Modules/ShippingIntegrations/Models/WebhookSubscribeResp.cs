namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class WebhookSubscribeResp
    {
        public class Error
        {
            public string code { get; set; }
            public string description { get; set; }
            public string severity { get; set; }
        }

        public class Response
        {
            public bool valid { get; set; }
            public List<Error> errors { get; set; }
            public int id { get; set; }
        }

        public class Root
        {
            public Response response { get; set; }
        }
    }
}
