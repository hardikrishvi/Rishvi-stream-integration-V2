using Rishvi.Modules.Core.Filters;

namespace Rishvi.DTOs.TaxInfo;

public class TaxInfoFilterDto : BaseFilterDto
{
    public TaxInfoFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public Guid TaxInfoId { get; set; }
    public string TaxNumber { get; set; }
}