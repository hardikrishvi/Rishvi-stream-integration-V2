using Rishvi.Modules.Core.Filters;

namespace Rishvi.Domain.DTOs.SyncSettings;

public class SyncSettingsFilterDto : BaseFilterDto
{
    public SyncSettingsFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public bool SyncEbayOrder { get; set; }
    public bool SyncLinnworksOrder { get; set; }
    public bool CreateEbayOrderToStream { get; set; }
    public bool CreateLinnworksOrderToStream { get; set; }
    public bool DispatchLinnworksOrderFromStream { get; set; }
    public bool DispatchEbayOrderFromStream { get; set; }

}