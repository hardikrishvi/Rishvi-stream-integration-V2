using Rishvi.Core.Data;

namespace Rishvi.Models;

public class CustomerInfo : IModificationHistory
{
    public Guid CustomerInfoId { get; set; }
    public string ChannelBuyerName { get; set; }

    public Guid AddressId { get; set; }           // Required FK
    public Address Address { get; set; }

    public Guid BillingAddressId { get; set; }    // Required FK
    public Address BillingAddress { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}