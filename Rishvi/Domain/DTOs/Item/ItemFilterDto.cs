using Rishvi.Modules.Core.Filters;

namespace Rishvi.DTOs.Item;

public class ItemFilterDto : BaseFilterDto
{
    public ItemFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public string ItemNumber { get; set; }
    public string SKU { get; set; }
    public string Title { get; set; }
    public int? Quantity { get; set; }
    public string CategoryName { get; set; }
    public bool? StockLevelsSpecified { get; set; }
    public int? OnOrder { get; set; }
    public int? InOrderBook { get; set; }
    public int? Level { get; set; }
    public int? MinimumLevel { get; set; }
    public int? AvailableStock { get; set; }
    public double? PricePerUnit { get; set; }
    public double? UnitCost { get; set; }
    public double? Cost { get; set; }
    public double? CostIncTax { get; set; }
}