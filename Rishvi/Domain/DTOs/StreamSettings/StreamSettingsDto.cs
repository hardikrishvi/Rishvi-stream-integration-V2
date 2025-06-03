namespace Rishvi.Domain.DTOs.StreamSettings;

public class StreamSettingsDto
{
    public bool GetTrackingDetails { get; set; }
    public bool EnableWebhook { get; set; }
    public bool SendChangeFromLinnworksToStream { get; set; }
    public bool SendChangesFromEbayToStream { get; set; }
    public bool CreateProductToStream { get; set; }
    public bool DownloadProductFromStreamToLinnworks { get; set; }
    public bool GetRoutePlanFromStream { get; set; }
    public bool GetDepotListFromStream { get; set; }
}