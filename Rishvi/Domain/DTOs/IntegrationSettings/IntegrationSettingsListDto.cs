using Rishvi.Models;

namespace Rishvi.Domain.DTOs.IntegrationSettings;

public class IntegrationSettingsListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string AuthorizationToken { get; set; }
    public string LinnworksSyncToken { get; set; }
    public Models.LinnworksSettings Linnworks { get; set; }
    public Models.StreamSettings Stream { get; set; }
    public object Ebay { get; set; } // Replace `object` with a class if Ebay settings will be defined later
    public Models.SyncSettings Sync { get; set; }
    public DateTime LastSyncOnDate { get; set; }
    public string LastSyncOn { get; set; }
    public int ebaypage { get; set; }
    public int ebayhour { get; set; }
    public int linnpage { get; set; }
    public int linnhour { get; set; }
}