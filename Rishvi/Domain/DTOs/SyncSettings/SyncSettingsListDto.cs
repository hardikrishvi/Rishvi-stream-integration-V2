namespace Rishvi.Domain.DTOs.SyncSettings;

public class SyncSettingsListDto
{
    public Guid Id { get; set; }
    public bool SyncEbayOrder { get; set; }
    public bool SyncLinnworksOrder { get; set; }
    public bool CreateEbayOrderToStream { get; set; }
    public bool CreateLinnworksOrderToStream { get; set; }
    public bool DispatchLinnworksOrderFromStream { get; set; }
    public bool DispatchEbayOrderFromStream { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}