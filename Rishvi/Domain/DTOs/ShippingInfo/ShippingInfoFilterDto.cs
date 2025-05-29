using Rishvi.Modules.Core.Filters;

namespace Rishvi.DTOs.ShippingInfo;

public class ShippingInfoFilterDto : BaseFilterDto
{
    public ShippingInfoFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public Guid? ShippingId { get; set; }
    public string Vendor { get; set; }
    public Guid? PostalServiceId { get; set; }
    
}