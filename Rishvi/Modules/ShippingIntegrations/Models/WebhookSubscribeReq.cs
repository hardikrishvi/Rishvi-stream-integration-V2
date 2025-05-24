namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class WebhookSubscribeReq
    {
        public string @event { get; set; }
        public string event_type { get; set; }
        public string url_path { get; set; }
        public string http_method { get; set; }
        public string content_type { get; set; }
        public string auth_header { get; set; }
    }
}
