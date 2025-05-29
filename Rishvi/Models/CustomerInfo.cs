using Rishvi.Core.Data;

namespace Rishvi.Models;

public class CustomerInfo : IModificationHistory
{
    public Guid CustomerInfoId { get; set; }
    public string ChannelBuyerName { get; set; }
    public Address Address { get; set; }
    public Address BillingAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}