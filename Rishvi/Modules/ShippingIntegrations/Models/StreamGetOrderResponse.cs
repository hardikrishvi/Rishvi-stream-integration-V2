namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class StreamGetOrderResponse
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Address
        {
            public string name { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string address3 { get; set; }
            public string address4 { get; set; }
            public string address5 { get; set; }
            public string country { get; set; }
            public string postcode { get; set; }
            public double lat { get; set; }
            public double @long { get; set; }
            public string nuts { get; set; }
            public string externalAddressId { get; set; }
            public string locationNotes { get; set; }
            public string vehicleType { get; set; }
        }

        public class Altemail
        {
            public string secondary { get; set; }
            public string operations { get; set; }
            public string financial { get; set; }
        }

        public class Contact
        {
            public string name { get; set; }
            public string tel1 { get; set; }
            public string tel2 { get; set; }
            public string mobile { get; set; }
            public string fax { get; set; }
            public string email { get; set; }
            public bool optOutEmail { get; set; }
            public bool optOutSms { get; set; }
            public List<Altemail> altemail { get; set; }
        }

        public class Container
        {
            public string containerId { get; set; }
        }

        public class Customer
        {
            public Address address { get; set; }
            public Contact contact { get; set; }
        }

        public class End
        {
            public string actualDateTime { get; set; }
            public string plannedDateTime { get; set; }
            public string postcode { get; set; }
            public double lat { get; set; }
            public double @long { get; set; }
        }

        public class Error
        {
            public string code { get; set; }
            public string description { get; set; }
            public string severity { get; set; }
        }

        public class Group
        {
            public int id { get; set; }
            public string type { get; set; }
            public string status { get; set; }
            public string deliveryMethod { get; set; }
            public string driverNotes { get; set; }
            public Required required { get; set; }
            public Planned planned { get; set; }
            public bool confirmedPlanning { get; set; }
            public string liveDate { get; set; }
            public int quantity { get; set; }
            public decimal weight { get; set; }
            public double cube { get; set; }
            public TimeOnSite timeOnSite { get; set; }
            public bool bookingRequired { get; set; }
            public string estimateArrivalDateTime { get; set; }
            public Address address { get; set; }
            public Contact contact { get; set; }
            public RunDetails runDetails { get; set; }
            public List<Item> items { get; set; }
        }

        public class Header
        {
            public string consignmentNo { get; set; }
            public string orderDate { get; set; }
            public string deliveryDate { get; set; }
            public string ownerId { get; set; }
            public string ownerName { get; set; }
            public Partner partner { get; set; }
            public string orderNo { get; set; }
            public string customerOrderNo { get; set; }
            public decimal orderWeight { get; set; }
            public bool assemblyRequired { get; set; }
            public string orderStatus { get; set; }
            public string collectionStatus { get; set; }
            public string stockStatus { get; set; }
            public string orderType { get; set; }
            public bool invoiced { get; set; }
            public string invoiceReference { get; set; }
            public string nutsCode { get; set; }
            public string serviceLevel { get; set; }
            public List<OrderNote> orderNotes { get; set; }
            public string routeInfo { get; set; }
            public string externalClientId { get; set; }
            public Customer customer { get; set; }
        }

        public class Item
        {
            public int sequence { get; set; }
            public int parentSequence { get; set; }
            public int externalSequence { get; set; }
            public string code { get; set; }
            public string description { get; set; }
            public int quantity { get; set; }
            public decimal weight { get; set; }
            public double cube { get; set; }
            public string stockLocation { get; set; }
            public string onHandDate { get; set; }
            public string packageId { get; set; }
            public string packageType { get; set; }
            public bool delivered { get; set; }
            public string deliveredDateTime { get; set; }
            public bool collected { get; set; }
            public string collectedDateTime { get; set; }
            public string notes { get; set; }
            public Container container { get; set; }
        }

        public class Order
        {
            public Header header { get; set; }
            public List<Group> groups { get; set; }
            public string trackingURL { get; set; }
            public string trackingId { get; set; }
        }

        public class OrderNote
        {
            public string code { get; set; }
            public string desc { get; set; }
        }

        public class Partner
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Planned
        {
            public string fromDateTime { get; set; }
            public string toDateTime { get; set; }
        }

        public class Required
        {
            public string fromDateTime { get; set; }
            public string toDateTime { get; set; }
        }

        public class Response
        {
            public bool valid { get; set; }
            public Order order { get; set; }
            public List<Error> errors { get; set; }
        }

        public class Root
        {
            public Response response { get; set; }
        }

        public class RunDetails
        {
            public string id { get; set; }
            public string description { get; set; }
            public int groupSequence { get; set; }
            public string vehicle { get; set; }
            public string vehicleName { get; set; }
            public string vehicleType { get; set; }
            public string driver { get; set; }
            public string driverName { get; set; }
            public bool dispatched { get; set; }
            public bool departed { get; set; }
            public bool completed { get; set; }
            public Start start { get; set; }
            public End end { get; set; }
        }

        public class Start
        {
            public string actualDateTime { get; set; }
            public string plannedDateTime { get; set; }
            public string postcode { get; set; }
            public double lat { get; set; }
            public double @long { get; set; }
        }

        public class TimeOnSite
        {
            public int unload { get; set; }
            public int assembly { get; set; }
        }


    }
}
