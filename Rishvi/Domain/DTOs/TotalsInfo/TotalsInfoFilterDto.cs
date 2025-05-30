using Rishvi.Modules.Core.Filters;

namespace Rishvi.DTOs.TotalsInfo;

public class TotalsInfoFilterDto : BaseFilterDto
{
    public TotalsInfoFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public double? Subtotal { get; set; }
    public double? PostageCost { get; set; }
    public double? PostageCostExTax { get; set; }
    public double? Tax { get; set; }
}