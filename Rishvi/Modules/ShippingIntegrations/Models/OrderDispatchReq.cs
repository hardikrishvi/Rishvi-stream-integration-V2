namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class OrderDispatchReq
    {
       public string token { get; set; }
        public string orderref { get; set; }
        public string itemid { get; set; }
        public string service { get; set; }
        public string trackingnumber { get; set; }
        public string trackingurl { get; set; }
        public string linntoken { get; set; }
    }
}
