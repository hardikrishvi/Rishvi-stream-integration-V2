namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class StreamOrderRequest
    {
        public Header header { get; set; } = new Header();
        public Delivery delivery { get; set; } = new Delivery();
    }

    public class Header
    {
        public string orderNo { get; set; }
        public string orderDate { get; set; }
        public Partner partner { get; set; } = new Partner();
        public string orderType { get; set; }
        public string orderCategory { get; set; }
        public string serviceLevel { get; set; }
        public List<Service> services { get; set; } = new List<Service>();
        public Customer customer { get; set; } = new Customer();
        public string customerOrderNo { get; set; }
        public string orderNotes { get; set; }
        public string driverNotes { get; set; }
        public bool cutOffTimeMet { get; set; }
        public string routeInfo { get; set; }
        public string externalClientId { get; set; }
        public List<Driverlink> driverLinks { get; set; }
        public bool updateAddresses { get; set; }
    }

    public class Partner
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Customer
    {
        public string name { get; set; }
        public Address address { get; set; } = new Address();
        public Contact contact { get; set; } = new Contact();
        public Account account { get; set; } = new Account();
    }

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
        public float lat { get; set; }
        public float _long { get; set; }
        public string nuts { get; set; }
        public string externalAddressId { get; set; }
        public string locationNotes { get; set; }
        public string addressRef { get; set; }
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
        public List<Altemail> altemail { get; set; } = new List<Altemail>();
    }

    public class Altemail
    {
        public string secondary { get; set; }
        public string operations { get; set; }
        public string financial { get; set; }
    }

    public class Account
    {
        public string name { get; set; }
        public string number { get; set; }
    }

    public class Service
    {
        public string code { get; set; }
    }

    public class Driverlink
    {
        public string description { get; set; }
        public string link { get; set; }
    }

    public class Delivery
    {
        public Address address { get; set; } = new Address();
        public Contact contact { get; set; } = new Contact();
        public Required required { get; set; } = new Required();
        public string deliveryMethod { get; set; }
        public Timeonsite timeOnSite { get; set; } = new Timeonsite();
        public bool bookingRequired { get; set; }
        public bool updateAddresses { get; set; }
        public List<StreamOrderItem> items { get; set; } = new List<StreamOrderItem>();
    }

    public class Required
    {
        public string fromDateTime { get; set; }
        public string toDateTime { get; set; }
    }

    public class Timeonsite
    {
        public int unload { get; set; }
        public int assembly { get; set; }
    }

    public class StreamOrderItem
    {
        public int sequence { get; set; }
        public int parentSequence { get; set; }
        public int consolidatedLevel { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public decimal weight { get; set; }
        public float cube { get; set; }
        public int assemblyTime { get; set; }
        public string stockLocation { get; set; }
        public string onHandDate { get; set; }
        public string packageId { get; set; }
        public string notes { get; set; }
        public string packageType { get; set; }
    }
}
