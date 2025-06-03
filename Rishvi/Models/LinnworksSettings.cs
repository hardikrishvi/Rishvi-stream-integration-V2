using Rishvi.Core.Data;

namespace Rishvi.Models;

public class LinnworksSettings : IModificationHistory
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