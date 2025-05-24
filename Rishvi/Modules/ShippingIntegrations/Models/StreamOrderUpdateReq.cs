namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class StreamOrderUpdateReq
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Account
        {
            public string name { get; set; }
            public string number { get; set; }
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

        public class Customer
        {
            public string name { get; set; }
            public Address address { get; set; }
            public Contact contact { get; set; }
            public Account account { get; set; }
        }

        public class DriverLink
        {
            public string description { get; set; }
            public string link { get; set; }
        }

        public class Header
        {
            public string orderDate { get; set; }
            public Partner partner { get; set; }
            public string serviceLevel { get; set; }
            public Customer customer { get; set; }
            public string customerOrderNo { get; set; }
            public string orderNotes { get; set; }
            public string driverNotes { get; set; }
            public string routeInfo { get; set; }
            public List<DriverLink> driverLinks { get; set; }
        }

        public class Partner
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Root
        {
            public Header header { get; set; }
        }


    }
}
