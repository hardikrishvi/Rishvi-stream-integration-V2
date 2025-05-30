using System.ComponentModel.DataAnnotations;
using Rishvi.Core.Data;

namespace Rishvi.Models;

public class ShippingInfo : IModificationHistory
{
    [Key]
    public Guid? ShippingId { get; set; }
    public string Vendor { get; set; }
    public Guid? PostalServiceId { get; set; }
    public string PostalServiceName { get; set; }
    public double? TotalWeight { get; set; }
    public double? ItemWeight { get; set; }
    public Guid? PackageCategoryId { get; set; }
    public string PackageCategory { get; set; }
    public Guid? PackageTypeId { get; set; }
    public string PackageType { get; set; }
    public double? PostageCost { get; set; }
    public double? PostageCostExTax { get; set; }
    public string TrackingNumber { get; set; }
    public bool? ManualAdjust { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}