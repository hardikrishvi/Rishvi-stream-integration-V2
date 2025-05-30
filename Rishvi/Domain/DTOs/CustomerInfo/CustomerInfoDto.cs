
namespace Rishvi.DTOs.CustomerInfo;

public class CustomerInfoDto
{
    public Guid CustomerInfoId { get; set; }
    public string ChannelBuyerName { get; set; }
    public Models.Address Address { get; set; }
    public Models.Address BillingAddress { get; set; }
}