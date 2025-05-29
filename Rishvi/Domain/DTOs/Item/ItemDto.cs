namespace Rishvi.DTOs.Item;

public class ItemDto
{
    public Guid? ItemId { get; set; }
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
    public List<Models.Item> CompositeSubItems { get; set; }
    public double? Weight { get; set; }
    public string BarcodeNumber { get; set; }
    public string ChannelSKU { get; set; }
    public string ChannelTitle { get; set; }
    public string BinRack { get; set; }
    public string ImageId { get; set; }
    public Guid? RowId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? StockItemId { get; set; }
    public int? StockItemIntId { get; set; }
}