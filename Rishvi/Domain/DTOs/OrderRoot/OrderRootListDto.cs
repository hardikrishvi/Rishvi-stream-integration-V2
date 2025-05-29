namespace Rishvi.DTOs.OrderRoot;

public class OrderRootListDto
{
    public Guid OrderId { get; set; } 
    public int? NumOrderId { get; set; }
    public Models.GeneralInfo GeneralInfo { get; set; }
    public Models.ShippingInfo ShippingInfo { get; set; }
    public Models.CustomerInfo CustomerInfo { get; set; }
    public Models.TotalsInfo TotalsInfo { get; set; }
    public Models.TaxInfo TaxInfo { get; set; }
    public List<string> FolderName { get; set; }
    public bool? IsPostFilteredOut { get; set; }
    public bool? CanFulfil { get; set; }
    public Models.Fulfillment Fulfillment { get; set; }
    public List<Models.Item> Items { get; set; }
    public bool? HasItems { get; set; }
    public int? TotalItemsSum { get; set; }
}