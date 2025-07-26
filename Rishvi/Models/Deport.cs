namespace Rishvi.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class DAddress
    {
        public string name { get; set; }
        public string country { get; set; }
        public string postcode { get; set; }
        public double lat { get; set; }
        public double @long { get; set; }
    }

    public class Contact
    {
        public string name { get; set; }
        public string email { get; set; }
    }

    public class DeliveryMethod
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }

    public class Depot
    {
        public string id { get; set; }
        public Address address { get; set; }
        public Contact contact { get; set; }
        public StockLocation stockLocation { get; set; }
        public List<DeliveryMethod> deliveryMethods { get; set; }
    }

    public class DepotResponse
    {
        public bool valid { get; set; }
        public List<object> errors { get; set; }
        public List<Depot> depots { get; set; }
    }

    public class DeportRoot
    {
        public DepotResponse response { get; set; }
    }

    public class StockLocation
    {
        public string id { get; set; }
        public string name { get; set; }
    }


}
