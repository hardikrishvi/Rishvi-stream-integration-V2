using Rishvi.Modules.Core.Filters;

namespace Rishvi.DTOs.OrderRoot;

public class OrderRootFilterDto : BaseFilterDto
{
    public OrderRootFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public int? NumOrderId { get; set; }
    public bool? IsPostFilteredOut { get; set; }
    public bool? CanFulfil { get; set; }
    public bool? HasItems { get; set; }
    public int? TotalItemsSum { get; set; }
}