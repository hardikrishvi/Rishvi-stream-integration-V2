using Rishvi.Modules.Core.Filters;

namespace Rishvi.DTOs.GeneralInfo;

public class GeneralInfoFilterDto : BaseFilterDto
{
    public GeneralInfoFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public int? Status { get; set; }
    public bool? LabelPrinted { get; set; }
    public string LabelError { get; set; }
}