namespace Rishvi.Models
{
    public class Subscription
    {
        public string party_id { get; set; }
        public int id { get; set; }
        public string @event { get; set; }
        public string event_type { get; set; }
        public string url_path { get; set; }
        public string http_method { get; set; }
    }
}
