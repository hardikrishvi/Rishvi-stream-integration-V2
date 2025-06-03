using Rishvi.Core.Data;

namespace Rishvi.Models;

public class Address : IModificationHistory
{
    public Guid Id { get; set; }
    public string EmailAddress { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string Town { get; set; }
    public string Region { get; set; }
    public string PostCode { get; set; }
    public string Country { get; set; }
    public string Continent { get; set; }
    public string FullName { get; set; }
    public string Company { get; set; }
    public string PhoneNumber { get; set; }
    public string temp { get; set; }
    public Guid? CountryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
