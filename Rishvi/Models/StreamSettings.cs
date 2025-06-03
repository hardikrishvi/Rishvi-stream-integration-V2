using Rishvi.Core.Data;

namespace Rishvi.Models;

public class StreamSettings : IModificationHistory
{
    public Guid Id { get; set; }
    public bool GetTrackingDetails { get; set; }
    public bool EnableWebhook { get; set; }
    public bool SendChangeFromLinnworksToStream { get; set; }
    public bool SendChangesFromEbayToStream { get; set; }
    public bool CreateProductToStream { get; set; }
    public bool DownloadProductFromStreamToLinnworks { get; set; }
    public bool GetRoutePlanFromStream { get; set; }
    public bool GetDepotListFromStream { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}