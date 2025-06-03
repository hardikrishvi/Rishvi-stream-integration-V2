using Rishvi.Modules.Core.Filters;

namespace Rishvi.Domain.DTOs.LinnworksSettings;

public class LinnworksSettingsFilterDto : BaseFilterDto
{
    public LinnworksSettingsFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "desc";
    }

    public bool DownloadOrderFromStream { get; set; }
    public bool DownloadOrderFromEbay { get; set; }
    public bool PrintLabelFromStream { get; set; }
    public bool PrintLabelFromLinnworks { get; set; }
    public bool DispatchOrderFromStream { get; set; }
    public bool DispatchOrderFromEbay { get; set; }
    public bool SendChangeToEbay { get; set; }
    public bool SendChangeToStream { get; set; }
}

