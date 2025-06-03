using Rishvi.Modules.Core.Filters;

namespace Rishvi.Domain.DTOs.Ebay;

public class EbayFilterDto : BaseFilterDto
{
    public EbayFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public bool DownloadOrderFromEbay { get; set; }
    public bool SendOrderToStream { get; set; }
    public bool UpdateInformationFromEbayToStream { get; set; }
    public bool DispatchOrderFromEbay { get; set; }
    public bool UpdateTrackingDetailsFromStream { get; set; }
}