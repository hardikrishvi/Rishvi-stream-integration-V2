namespace Rishvi.Domain.DTOs.SyncSettings;

public class SyncSettingsDto
{
    public bool SyncEbayOrder { get; set; }
    public bool SyncLinnworksOrder { get; set; }
    public bool CreateEbayOrderToStream { get; set; }
    public bool CreateLinnworksOrderToStream { get; set; }
    public bool DispatchLinnworksOrderFromStream { get; set; }
    public bool DispatchEbayOrderFromStream { get; set; }
}