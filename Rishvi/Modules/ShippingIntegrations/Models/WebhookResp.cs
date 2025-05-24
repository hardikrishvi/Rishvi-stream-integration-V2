namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class WebhookResp
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Event
        {
            public string event_code { get; set; }
            public string event_code_desc { get; set; }
            public string event_desc { get; set; }
            public string event_date { get; set; }
            public string event_time { get; set; }
            public string event_text { get; set; }
            public string event_link { get; set; }
        }

        public class Order
        {
            public string id { get; set; }
        }

        public class Root
        {
            public Webhook webhook { get; set; }
        }

        public class Subscription
        {
            public string party_id { get; set; }
            public int id { get; set; }
            public string @event { get; set; }
            public string event_type { get; set; }
            public string url_path { get; set; }
            public string http_method { get; set; }
        }

        public class Webhook
        {
            public Subscription subscription { get; set; }
            public Event @event { get; set; }
            public Order order { get; set; }
        }


    }
}
