using Rishvi.Modules.Core.Filters;

namespace Rishvi.DTOs.CustomerInfo;

public class CustomerInfoFilterDto : BaseFilterDto
{
    public CustomerInfoFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public Guid CustomerInfoId { get; set; }
    public string ChannelBuyerName { get; set; }
}