using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Rishvi.Core.Data;

namespace Rishvi.Models;

[Table("Orders")]
public class OrderRoot : IModificationHistory
{
    [Key]
    public Guid OrderId { get; set; }
    public int NumOrderId { get; set; }
    public GeneralInfo GeneralInfo { get; set; }
    public ShippingInfo ShippingInfo { get; set; }
    public CustomerInfo CustomerInfo { get; set; }
    public TotalsInfo TotalsInfo { get; set; }
    public TaxInfo TaxInfo { get; set; }
    public List<string> FolderName { get; set; }
    public bool? IsPostFilteredOut { get; set; }
    public bool? CanFulfil { get; set; }
    public Fulfillment Fulfillment { get; set; }
    public List<Item> Items { get; set; }
    public bool? HasItems { get; set; }
    public int? TotalItemsSum { get; set; }

    public string TempColumn { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
