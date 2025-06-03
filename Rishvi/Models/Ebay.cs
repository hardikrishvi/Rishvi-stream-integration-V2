using Rishvi.Core.Data;

namespace Rishvi.Models;

public class Ebay : IModificationHistory
{
    public Guid Id { get; set; }

    public bool DownloadOrderFromEbay { get; set; }
    public bool SendOrderToStream { get; set; }
    public bool UpdateInformationFromEbayToStream { get; set; }
    public bool DispatchOrderFromEbay { get; set; }
    public bool UpdateTrackingDetailsFromStream { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}