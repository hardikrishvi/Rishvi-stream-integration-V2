using Rishvi.Core.Data;

namespace Rishvi.Models;

public class IntegrationSettings : IModificationHistory
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string AuthorizationToken { get; set; }
    public string LinnworksSyncToken { get; set; }
    public LinnworksSettings Linnworks { get; set; }
    public StreamSettings Stream { get; set; }
    public Ebay Ebay { get; set; } // Replace `object` with a class if Ebay settings will be defined later
    public SyncSettings Sync { get; set; }
    public DateTime LastSyncOnDate { get; set; }
    public string LastSyncOn { get; set; }
    public int ebaypage { get; set; }
    public int ebayhour { get; set; }
    public int linnpage { get; set; }
    public int linnhour { get; set; }
    public Guid SyncId { get; set; }


    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}