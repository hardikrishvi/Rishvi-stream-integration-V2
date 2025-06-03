namespace Rishvi.Domain.DTOs.LinnworksSettings;

public class LinnworksSettingsListDto
{
    public Guid Id { get; set; }
    public bool DownloadOrderFromStream { get; set; }
    public bool DownloadOrderFromEbay { get; set; }
    public bool PrintLabelFromStream { get; set; }
    public bool PrintLabelFromLinnworks { get; set; }
    public bool DispatchOrderFromStream { get; set; }
    public bool DispatchOrderFromEbay { get; set; }
    public bool SendChangeToEbay { get; set; }
    public bool SendChangeToStream { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}