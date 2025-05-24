namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class StreamOrderCollectionRequest
    {
        public Header header { get; set; } = new Header();
        public Collection collection { get; set; } = new Collection();
    }


    public class Collection
    {
        public Address address { get; set; } = new Address();
        public Contact contact { get; set; } = new Contact();
        public Required required { get; set; } = new Required();
        public string collectionMethod { get; set; }
        public Timeonsite timeOnSite { get; set; } = new Timeonsite();
        public bool bookingRequired { get; set; }
        public List<StreamOrderItem> items { get; set; } = new List<StreamOrderItem>();
    }

}
