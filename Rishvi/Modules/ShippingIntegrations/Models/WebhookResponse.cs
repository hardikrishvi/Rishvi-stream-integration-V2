namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class WebhookResponse
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

        public class Group
        {
            public int int_seq { get; set; }
            public string loadId { get; set; }
        }

        public class Order
        {
            public int sequence { get; set; }
            public string order { get; set; }
        }

        public class SingleOrder
        {
            public string id { get; set; }
        }

        public class Root
        {
            public Webhook webhook { get; set; }
        }

        public class Run
        {
            public string loadId { get; set; }
            public string status { get; set; }
            public string description { get; set; }
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
            public Run run { get; set; }
            public Group group { get; set; }
            public List<Order> orders { get; set; }
            public SingleOrder order { get; set; }
        }


    }
}
