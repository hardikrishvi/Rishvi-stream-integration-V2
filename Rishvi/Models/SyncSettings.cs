using Rishvi.Core.Data;

namespace Rishvi.Models;

public class SyncSettings : IModificationHistory
{
    public Guid Id { get; set; }
    public bool SyncEbayOrder { get; set; }
    public bool SyncLinnworksOrder { get; set; }
    public bool CreateEbayOrderToStream { get; set; }
    public bool CreateLinnworksOrderToStream { get; set; }
    public bool DispatchLinnworksOrderFromStream { get; set; }
    public bool DispatchEbayOrderFromStream { get; set; }
    public bool UpdateLinnworksOrderToStream { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}